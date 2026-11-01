// Imperial PERSONAS (Thomas decision 01.11./02.11., CONCEPT_personas.md):
// the persona REPLACES the Imperial class-card and agenda-card system in
// the tactical AI completely (Noldorion PR #194 direction reply: a 1:1 card
// port would make the rebels play the Imperial side - so the cards go away
// and a small persona table takes their role).
//
// FAIRNESS LAW (binding, mirrors the concept's Tabu-Grenze): a persona may
// ONLY touch the public channels below. NO dice, NO hidden information, NO
// status tokens, NO stat buffs. Every effect is printed visible: discounts
// in the deploy costs, the doctrine flip in the popup line, the guard share
// in the doctrine display, the signature as ui.json text.
//
//   cost_reduction   -n deploy/reinforce cost per matching card
//                    (floored at 1 - gratis comes ONLY via free_deployment)
//   free_deployment  per mission, an extra threat credit equal to the
//                    CHEAPEST matching printed cost x count (deterministic;
//                    credited at mission start so the deploy math stays
//                    single-sourced - documented deviation from a literal
//                    "free group": same economy, nothing hidden)
//   pattern_weight   seeded per-round share boost of one doctrine pattern
//                    for matching cards (the ONLY randomness in the whole
//                    persona system, and it is dice-free: the same mission
//                    seed replays every flip identically)
//   guard_split      persona share of the defend guard band (default 50)
//   signaturText     ui.json key shown on the instruction cards - pure
//                    flavour, never a mechanic
//
// RANK LAW (Thomas decision 02.11.): the rank follows the threat value of
// the NEXT mission automatically: rank = highest authored rank whose
// minThreat <= threat. No XP, no purchase, no memory - a pure function of
// (persona, threat), so the rank rises AND falls openly with the mission
// design and the mission-start QuickMessage states every change.
//
// MATCHING: internalNameMatch is a case-insensitive regex over the INTERNAL
// card name from CardData/enemies.json (never the translated display names
// under Languages/<L>/DeploymentGroups - those only re-skin the UI);
// factionMatch compares the printed faction string exactly.
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TacticalAI.Core
{
    /// <summary>One effect row of a persona rank (asset schema: the asset's
    /// own _meta block is the normative schema text).</summary>
    public sealed class PersonaEffect
    {
        public string Kind = "";              // cost_reduction | free_deployment | pattern_weight | guard_split | signaturText
        public int Amount = 1;                // cost_reduction amount / free_deployment count
        public double Multiplier = 1.0;       // pattern_weight multiplier (<=1 = no-op)
        public List<string> Patterns = new List<string>();
        public string NameMatch = null;       // regex source or null
        public string FactionMatch = null;    // exact faction or null
        public int GuardPercent = -1;         // guard_split share (0..100)
        public string Key = null;             // signaturText ui.json key
        public string Source = "";            // printed-card derivation (dev docs)

        /// <summary>Does a guard_split effect row apply to this card? Used
        /// by the Unity guard band to decide WHICH advancers the persona
        /// shares (cards without any applicable guard_split row stay in the
        /// default 50 % band logic).</summary>
        public bool GuardMatches(string internalName, string faction)
        {
            return Kind == "guard_split" && GuardPercent >= 0
                   && Matches(internalName, faction);
        }

        /// <summary>internalName/faction may both be null for id-only
        /// contexts; a filter row then matches by what it declares.</summary>
        public bool Matches(string internalName, string faction)
        {
            if (FactionMatch != null && !string.Equals(FactionMatch, faction ?? "", StringComparison.Ordinal))
                return false;
            if (NameMatch != null)
            {
                if (string.IsNullOrEmpty(internalName)) return false;
                try { if (!Regex.IsMatch(internalName, NameMatch, RegexOptions.IgnoreCase)) return false; }
                catch (ArgumentException) { return false; }    // broken regex matches NOTHING (fail-safe)
            }
            return true;
        }
    }

    /// <summary>One persona rank as authored in the asset.</summary>
    public sealed class PersonaRank
    {
        public int Rank;
        public int MinThreat;
        public List<PersonaEffect> Effects = new List<PersonaEffect>();
    }

    /// <summary>One persona (Id = save token, NameKey/MottoKey = ui.json).</summary>
    public sealed class PersonaDef
    {
        public string Id = "";
        public string NameKey = "";
        public string MottoKey = "";
        public List<PersonaRank> Ranks = new List<PersonaRank>();
    }

    /// <summary>Immutable resolution of (persona, threat): the active rank
    /// plus the union of the effects of every rank up to it. An EMPTY view
    /// is "no persona" = the exact pre-persona behaviour.</summary>
    public sealed class PersonaView
    {
        public string Id = "";
        public int Rank;                                   // 0 = none
        public List<PersonaEffect> Active = new List<PersonaEffect>();

        public bool IsEmpty { get { return Rank <= 0 || Active.Count == 0; } }

        /// <summary>Deploy/reinforce cost after every matching reduction,
        /// floored at 1 (fairness law: discounts never stack to free).</summary>
        public int CostFor(string internalName, string faction, int printedCost)
        {
            var total = 0;
            foreach (var e in Active)
                if (e.Kind == "cost_reduction" && e.Matches(internalName, faction))
                    total += e.Amount;
            var v = printedCost - total;
            return v < 1 ? 1 : v;
        }

        /// <summary>How many points this view takes off a printed cost of
        /// <paramref name="printedCost"/> (floor-aware: the discount a
        /// deploy popup will actually book, not the raw row sum).</summary>
        public int CostCutFor(string internalName, string faction, int printedCost)
        {
            return printedCost - CostFor(internalName, faction, printedCost);
        }

        /// <summary>Guard share of a defend deployment for a matching group
        /// (percent); -1 = no override, the caller keeps the 50 % default.</summary>
        public int GuardPercentFor(string internalName, string faction)
        {
            var best = -1;
            foreach (var e in Active)
                if (e.Kind == "guard_split" && e.GuardPercent >= 0
                    && e.Matches(internalName, faction)) best = e.GuardPercent;
            return best;
        }

        /// <summary>Mission-start threat credit: cheapest matching printed
        /// cost x count, summed over the free_deployment effects. cards =
        /// rows of (internalName, faction, printedCostAsText) from the
        /// CURRENT deploy hand (public data). 0 = no credit.</summary>
        public long FreeCreditCost(List<string[]> cards)
        {
            long total = 0;
            foreach (var e in Active)
            {
                if (e.Kind != "free_deployment") continue;
                var cheapest = int.MaxValue;
                foreach (var c in cards ?? new List<string[]>())
                {
                    if (c == null || c.Length < 3) continue;
                    int cost;
                    if (!int.TryParse(c[2], out cost)) continue;
                    if (!e.Matches(c[0], c[1])) continue;
                    if (cost < cheapest) cheapest = cost;
                }
                if (cheapest != int.MaxValue)
                    total += (long)cheapest * Math.Max(1, e.Amount);
            }
            return total;
        }

        /// <summary>Last authored signature key (null = none). Pure ui.json
        /// display text - it changes no number anywhere.</summary>
        public string SignatureKey()
        {
            string key = null;
            foreach (var e in Active)
                if (e.Kind == "signaturText" && !string.IsNullOrEmpty(e.Key)) key = e.Key;
            return key;
        }

        /// <summary>Doctrine share boost: with probability
        /// p = min(0.5, multiplier - 1) the group's round pattern flips to
        /// the first persona pattern that differs from the printed-role
        /// base. Deterministic: the draw is a pure hash of (persona id,
        /// mission seed, round, card id) - the same mission replays every
        /// flip. No persona effect matches -> base pattern unchanged.</summary>
        public string PatternFor(string internalName, string faction,
                                 string cardId, string basePattern,
                                 ulong missionSeed, int roundNo)
        {
            if (IsEmpty || string.IsNullOrEmpty(basePattern)) return basePattern;
            foreach (var e in Active)
            {
                if (e.Kind != "pattern_weight") continue;
                if (e.Multiplier <= 1.0 || e.Patterns.Count == 0) continue;
                if (!e.Matches(internalName, faction)) continue;
                string target = null;
                foreach (var p in e.Patterns)
                    if (!string.IsNullOrEmpty(p) && p != basePattern) { target = p; break; }
                if (target == null) continue;              // already the persona pattern
                var pBoost = e.Multiplier - 1.0;
                if (pBoost > 0.5) pBoost = 0.5;
                var h = ImperialDecks.Fnv1a64(Id + "|r" + roundNo + "|" + cardId) ^ missionSeed;
                var draw = (h >> 11) * (1.0 / 9007199254740992.0);   // [0,1), 53 bits
                if (draw < pBoost) return target;
            }
            return basePattern;
        }
    }

    /// <summary>The parsed persona asset.</summary>
    public sealed class PersonaTable
    {
        public List<PersonaDef> Personas = new List<PersonaDef>();
        public PersonaDef Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var p in Personas)
                if (string.Equals(p.Id, id, StringComparison.Ordinal)) return p;
            return null;
        }

        /// <summary>Rank = highest authored rank with minThreat &lt;= threat
        /// (authored order is authoritative, ties resolve to the LAST such
        /// row); active effects = union over ALL ranks with minThreat &lt;=
        /// threat, in authored order. Unknown/empty id -> EMPTY view.</summary>
        public PersonaView Resolve(string personaId, int threat)
        {
            var view = new PersonaView();
            var def = Find(personaId);
            if (def == null) return view;
            view.Id = def.Id;
            var activeRanks = new List<PersonaRank>();
            foreach (var r in def.Ranks)
                if (r.MinThreat <= threat) activeRanks.Add(r);
            if (activeRanks.Count == 0) return view;       // rank 0 - nothing unlocked
            view.Rank = activeRanks[activeRanks.Count - 1].Rank;
            foreach (var r in activeRanks) view.Active.AddRange(r.Effects);
            return view;
        }
    }

    /// <summary>Asset seam (PrimeDoc/DocOrNull pattern of the tactical
    /// modules): the Unity side loads the TextAsset, Core parses, the
    /// harness pins against the SHIPPED asset.</summary>
    public static class Personas
    {
        static Dictionary<string, object> _doc;

        public static void PrimeDoc(Dictionary<string, object> doc) { _doc = doc; }
        public static Dictionary<string, object> DocOrNull() { return _doc; }

        static string S(Dictionary<string, object> d, string k, string def = null)
        {
            object v; return d != null && d.TryGetValue(k, out v) && v != null ? Json.AsStr(v) : def;
        }
        static int I(Dictionary<string, object> d, string k, int def)
        {
            object v; return d != null && d.TryGetValue(k, out v) && v != null ? Json.AsInt(v) : def;
        }
        static double D(Dictionary<string, object> d, string k, double def)
        {
            object v; return d != null && d.TryGetValue(k, out v) && v != null ? Json.AsDouble(v) : def;
        }

        public static PersonaTable Load()
        {
            var table = new PersonaTable();
            if (_doc == null) return table;
            object rowsObj;
            if (!_doc.TryGetValue("personas", out rowsObj) || rowsObj == null) return table;
            foreach (var raw in Json.AsList(rowsObj))
            {
                var d = raw as Dictionary<string, object>;
                if (d == null) continue;
                var p = new PersonaDef
                {
                    Id = S(d, "id", ""),
                    NameKey = S(d, "nameKey", ""),
                    MottoKey = S(d, "mottoKey", ""),
                };
                object ranksObj;
                if (d.TryGetValue("ranks", out ranksObj) && ranksObj != null)
                    foreach (var rraw in Json.AsList(ranksObj))
                    {
                        var rd = rraw as Dictionary<string, object>;
                        if (rd == null) continue;
                        var r = new PersonaRank
                        {
                            Rank = I(rd, "rank", 0),
                            MinThreat = I(rd, "minThreat", 0),
                        };
                        object effsObj;
                        if (rd.TryGetValue("effects", out effsObj) && effsObj != null)
                            foreach (var eraw in Json.AsList(effsObj))
                            {
                                var ed = eraw as Dictionary<string, object>;
                                if (ed == null) continue;
                                var e = new PersonaEffect
                                {
                                    Kind = S(ed, "kind", ""),
                                    Amount = I(ed, "amount", 1),
                                    Multiplier = D(ed, "multiplier", 1.0),
                                    GuardPercent = I(ed, "guardPercent", -1),
                                    Key = S(ed, "key", null),
                                    NameMatch = S(ed, "internalNameMatch", null),
                                    FactionMatch = S(ed, "factionMatch", null),
                                    Source = S(ed, "source", ""),
                                };
                                object patsObj;
                                if (ed.TryGetValue("patterns", out patsObj) && patsObj != null)
                                    foreach (var pobj in Json.AsList(patsObj))
                                    {
                                        var s = pobj as string;
                                        if (!string.IsNullOrEmpty(s)) e.Patterns.Add(s);
                                    }
                                r.Effects.Add(e);
                            }
                        p.Ranks.Add(r);
                    }
                table.Personas.Add(p);
            }
            return table;
        }
    }
}
