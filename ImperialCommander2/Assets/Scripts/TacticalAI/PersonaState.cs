// PersonaState - the single Unity-side access point for the imperial
// PERSONAS (Thomas decisions 01.11./02.11., CONCEPT_personas.md). Every
// other file (bridge, deploy popups, campaign overview, IMPERIUM panel)
// asks HERE; nobody else parses the asset or resolves ranks.
//
// Storage law (mirrors the aiMode pattern the app already ships):
//  * global default  -> TacticalAISettings.PersonaDefault (PlayerPrefs)
//  * per campaign    -> SagaCampaign.persona (campaign JSON via
//                       SaveCampaignState); missing field in old saves = ""
//                       = follow the default - exactly how aiMode migrates.
// Rank law: pure function of (persona, threat of the mission being played)
// - no XP, no purchase, no memory (Core.Personas header).
using System;
using System.Collections.Generic;
using System.Linq;
using TacticalAI.Core;
using UnityEngine;

namespace TacticalAI
{
    public static class PersonaState
    {
        public const string None = "";                 // follow global default
        public const string ExplicitNone = "none";     // explicit "no persona"

        // live-mission subsidy identity (see MissionSubsidy)
        static string _subsidyKey;
        static long _grantedSubsidy;

        /// <summary>The persona asset (null when it failed to load - every
        /// method below fails open to 'no persona').</summary>
        static PersonaTable Table()
        {
            try { return TacticalAIAssets.PersonasRoot; }
            catch { return null; }
        }

        static Saga.SagaCampaign Live()
        {
            try { return Saga.RunningCampaign.sagaCampaign; }
            catch { return null; }
        }

        /// <summary>All selectable defs, asset order.</summary>
        public static List<PersonaDef> Defs()
        {
            var t = Table();
            return t == null ? new List<PersonaDef>() : t.Personas;
        }

        public static PersonaDef Def(string id)
        {
            var t = Table();
            return t == null ? null : t.Find(id);
        }

        /// <summary>What the campaign picker stores ("" = follow default).</summary>
        public static string StoredFor(Saga.SagaCampaign camp)
        {
            return camp == null ? None : (camp.persona ?? None);
        }

        /// <summary>The id in effect for a campaign: explicit field first,
        /// else the global default; unknown ids degrade to none (the asset
        /// is the validator - a stale save id never crashes anything).</summary>
        public static string EffectiveId(Saga.SagaCampaign camp)
        {
            var raw = string.IsNullOrEmpty(StoredFor(camp)) ? SettingsDefault() : StoredFor(camp);
            if (string.IsNullOrEmpty(raw) || raw == ExplicitNone) return ExplicitNone;
            return Def(raw) == null ? ExplicitNone : raw;
        }

        public static string SettingsDefault()
        {
            try { return TacticalAISettings.PersonaDefault; }
            catch { return ""; }
        }

        /// <summary>Set the campaign persona (picker). Same value = no-op;
        /// persists like every other campaign field.</summary>
        public static void SetFor(Saga.SagaCampaign camp, string id)
        {
            if (camp == null || camp.persona == id) return;
            camp.persona = id ?? None;
            try { camp.SaveCampaignState(); }
            catch (Exception e) { Debug.LogWarning("PersonaState::save failed " + e.Message); }
        }

        /// <summary>Cycle the campaign picker: follow-default -> none ->
        /// each asset persona (asset order) -> follow-default.</summary>
        public static string Cycle(Saga.SagaCampaign camp)
        {
            if (camp == null) return None;
            var ids = new List<string> { None, ExplicitNone };
            ids.AddRange(Defs().Select(d => d.Id));
            var cur = camp.persona ?? None;
            var i = ids.IndexOf(cur);
            if (i < 0) i = 0;
            var next = ids[(i + 1) % ids.Count];
            SetFor(camp, next);
            return next;
        }

        /// <summary>Cycle the GLOBAL default (settings row): none -> each
        /// asset persona (asset order) -> none. The campaign field "" keeps
        /// following whatever this says.</summary>
        public static string CycleDefault()
        {
            var ids = new List<string> { None, ExplicitNone };
            ids.AddRange(Defs().Select(d => d.Id));
            var cur = SettingsDefault();
            var i = ids.IndexOf(cur);
            if (i < 0) i = 0;
            var next = ids[(i + 1) % ids.Count];
            try { TacticalAISettings.PersonaDefault = next; } catch { }
            return next;
        }

        /// <summary>Settings-row label for the global default (asset is the
        /// validator: an unknown stored id displays as none).</summary>
        public static string DescribeDefault()
        {
            var lang = L();
            var raw = SettingsDefault();
            if (string.IsNullOrEmpty(raw) || raw == ExplicitNone || Def(raw) == null)
                return lang != null ? lang.personaNone : "none";
            return DisplayName(raw);
        }

        /// <summary>Threat that drives the rank: the live mission budget
        /// (setupOptions.threatLevel - HUD-visible), -1 without a session
        /// (then there is no rank; the overview shows per-mission values).</summary>
        public static int LiveThreat()
        {
            try
            {
                var s = DataStore.sagaSessionData;
                return s != null && s.setupOptions != null ? s.setupOptions.threatLevel : -1;
            }
            catch { return -1; }
        }

        /// <summary>Resolve the live campaign's persona at the live threat.
        /// Empty view (Rank 0) = no persona / no session / broken asset.</summary>
        public static PersonaView ResolveLive()
        {
            return ResolveFor(EffectiveId(Live()), LiveThreat());
        }

        public static PersonaView ResolveFor(string id, int threat)
        {
            var t = Table();
            if (t == null || threat < 0 || string.IsNullOrEmpty(id) || id == ExplicitNone)
                return new PersonaView();
            try { return t.Resolve(id, threat); }
            catch (Exception e)
            {
                Debug.LogWarning("PersonaState::resolve failed: " + e.Message);
                return new PersonaView();
            }
        }

        /// <summary>Deploy/reinforce price of a printed card for the LIVE
        /// campaign. Fail-open: printed cost (a display or booking site
        /// must never be broken by the persona layer).</summary>
        public static int CostFor(global::DeploymentCard card, int printedCost)
        {
            try
            {
                if (card == null || !TacticalAISettings.Enabled) return printedCost;
                var v = ResolveLive();
                return v.IsEmpty ? printedCost
                                 : v.CostFor(card.name, card.faction, printedCost);
            }
            catch { return printedCost; }
        }

        /// <summary>Personas (M2): grant + announce the mission subsidy at
        /// the deployment popup OPEN - the earliest moment the deploy hand
        /// and the live threat exist, so the credit is really spendable in
        /// the opening deployment (the bridge calls MissionSubsidy again per
        /// round as the idempotent backstop). Mission key = missionID +
        /// threat (a rescaled restart is a NEW mission identity, like every
        /// other booking).</summary>
        public static void EnsureMissionSubsidy()
        {
            try
            {
                var camp = Live();
                string missionKey = null;
                var s = DataStore.sagaSessionData;
                if (s != null && s.setupOptions != null && s.setupOptions.projectItem != null)
                    missionKey = s.setupOptions.projectItem.missionID + "|" + LiveThreat();
                long credit = MissionSubsidy(camp, missionKey);
                if (credit <= 0) return;
                var v = ResolveFor(EffectiveId(camp), LiveThreat());
                var txt = SubsidyText(v, credit);
                if (string.IsNullOrEmpty(txt)) return;
                var q = UnityEngine.Object.FindObjectOfType<QuickMessage>();
                if (q != null) q.Show(txt);
            }
            catch (Exception e) { Debug.LogWarning("PersonaState::subsidy failed " + e.Message); }
        }

        /// <summary>Mission-start subsidy: cheapest matching printed cost of
        /// the CURRENT deploy hand x count (the asset's free_deployment
        /// channel - see Core.Personas header for why it is a credit).
        /// Booked exactly once per mission identity (campaign GUID +
        /// mission key + threat), so a mid-mission reload cannot
        /// double-grant; the QM states every grant openly.</summary>
        public static long MissionSubsidy(Saga.SagaCampaign camp, string missionKey)
        {
            try
            {
                var v = ResolveFor(EffectiveId(camp), LiveThreat());
                if (v.IsEmpty) { _subsidyKey = null; _grantedSubsidy = 0; return 0; }
                long credit = v.FreeCreditCost(HandRows());
                var key = (camp != null ? camp.GUID.ToString() : "solo")
                          + "|" + (missionKey ?? "") + "|" + LiveThreat();
                if (credit > 0 && _subsidyKey != key)
                {
                    _subsidyKey = key;
                    _grantedSubsidy = credit;
                    DataStore.sagaSessionData.ModifyThreat((int)Math.Min(credit, 100));
                    Debug.Log("PersonaState::subsidy +" + _grantedSubsidy
                              + " persona=" + v.Id + " rank=" + v.Rank);
                }
                return _grantedSubsidy;
            }
            catch (Exception e)
            {
                Debug.LogWarning("PersonaState::subsidy failed: " + e.Message);
                return 0;
            }
        }

        /// <summary>(internalName, faction, cost-as-text) rows of the live
        /// deploy hand - public data (the deploy popup lists exactly
        /// these).</summary>
        static List<string[]> HandRows()
        {
            var rows = new List<string[]>();
            Action<IEnumerable<global::DeploymentCard>> add = pool =>
            {
                if (pool == null) return;
                foreach (var c in pool)
                    if (c != null && !string.IsNullOrEmpty(c.name))
                        rows.Add(new[] { c.name, c.faction ?? "", c.cost.ToString() });
            };
            add(DataStore.deploymentCards);
            add(DataStore.villainCards);
            return rows;
        }

        // ------------------------------------------------------------ texts
        static UITacticalAI L()
        {
            try { return DataStore.uiLanguage != null ? DataStore.uiLanguage.uiTacticalAI : null; }
            catch { return null; }
        }

        static string TF(string fmt, params object[] args)
        {
            if (string.IsNullOrEmpty(fmt)) return "";
            try { return args.Length > 0 ? string.Format(fmt, args) : fmt; }
            catch (System.FormatException) { return fmt; }
        }

        static string Localized(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            var lang = L();
            if (lang == null) return null;
            var fi = typeof(UITacticalAI).GetField(key);
            return fi == null ? null : fi.GetValue(lang) as string;
        }

        /// <summary>Localized display name of a persona (its asset nameKey
        /// through ui.json; the raw id only as last-ditch fallback).</summary>
        public static string DisplayName(string id)
        {
            var def = Def(id);
            if (def == null) return "";
            var t = Localized(def.NameKey);
            return string.IsNullOrEmpty(t) ? def.Id : t;
        }

        /// <summary>The campaign-overview picker label: what is SELECTED
        /// (not what it currently means - the popup body shows the live
        /// rank). ""-default gets the explicit 'follow global' marker.</summary>
        public static string DescribeSelection(Saga.SagaCampaign camp)
        {
            var lang = L();
            if (lang == null) return "";
            var stored = camp == null ? None : (camp.persona ?? None);
            if (string.IsNullOrEmpty(stored))
            {
                var gd = SettingsDefault();
                var gdName = string.IsNullOrEmpty(gd) || gd == ExplicitNone
                    ? lang.personaNone : DisplayName(gd);
                return TF(lang.personaFollowDefault, gdName);
            }
            if (stored == ExplicitNone) return lang.personaNone;
            return DisplayName(stored);
        }

        /// <summary>The instruction-card/IMPERIUM line for one group:
        /// persona, rank, and the bonuses that bite for THIS card. ui.json
        /// vocabulary only; null when there is nothing to say.</summary>
        public static string DescribeLine(PersonaView v, string internalName, string faction)
        {
            var lang = L();
            if (lang == null || v == null || v.Rank <= 0) return null;
            var name = DisplayName(v.Id);
            if (string.IsNullOrEmpty(name)) return null;
            var head = name + " - " + TF(lang.personaRank, v.Rank);
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(internalName))
            {
                // floor-aware: exactly what the deploy popup will book off
                // a card of THIS group (raw row sums would lie at cost 1).
                var d = v.CostCutFor(internalName, faction, 100);
                if (d > 0) parts.Add(TF(lang.personaBonusCost, d));
            }
            if (v.Active.Any(e => e.Kind == "free_deployment"))
                parts.Add(lang.personaBonusSubsidy);
            if (v.Active.Any(e => e.Kind == "pattern_weight"))
                parts.Add(lang.personaBonusDoctrine);
            var g = v.GuardPercentFor(internalName ?? "", faction);
            if (g >= 0) parts.Add(TF(lang.personaBonusGuard, g));
            var line = parts.Count == 0 ? head : head + " - " + string.Join(" · ", parts);
            var sig = Localized(v.SignatureKey());
            if (!string.IsNullOrEmpty(sig)) line += "\n" + sig;
            return line;
        }

        /// <summary>Rank-change announcement (bridge, once per mission):
        /// up AND down are named openly (rank law, Core.Personas header).</summary>
        public static string RankChangeText(PersonaView v, int oldRank, bool up)
        {
            var lang = L();
            if (lang == null || v == null || v.Rank <= 0) return null;
            var name = DisplayName(v.Id);
            if (string.IsNullOrEmpty(name)) return null;
            return up
                ? TF(lang.personaRankUpQM, name, v.Rank)
                : TF(lang.personaRankDownQM, name, oldRank, v.Rank);
        }

        /// <summary>Subsidy announcement text (QM at mission start).</summary>
        public static string SubsidyText(PersonaView v, long credit)
        {
            var lang = L();
            if (lang == null || v == null || credit <= 0) return null;
            var name = DisplayName(v.Id);
            if (string.IsNullOrEmpty(name)) return null;
            return TF(lang.personaSubsidyQM, name, v.Rank, credit);
        }
    }
}
