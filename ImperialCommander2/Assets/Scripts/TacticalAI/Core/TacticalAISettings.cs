// M1 settings seam (concept section 4.2 "Settings"). Pure C# - NO UnityEngine
// imports: the same class compiles into the console harness csproj and the
// Unity Assembly-CSharp. Persistence is injected by the Unity side via
// SetSaveHook (PlayerPrefs binding in TacticalAISettingsUnity).
//
// HARD RULE: every Tactical-AI behaviour in the game checks
//   if (TacticalAI.TacticalAISettings.Enabled)
// and keeps the original legacy path verbatim as the else/default branch.
// DEFAULT IS OFF ("legacy") - behaviour bit-compatible with the shipped game.
//
// NOTE (documented deviation): the class lives in the parent namespace
// `TacticalAI` (not `TacticalAI.Core`) so the hook-table condition
// `TacticalAI.TacticalAISettings.Enabled` compiles verbatim in every hook
// site without a using-directive. The FILE still lives under Core/ because
// it is pure C# and belongs to the Core compilation.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TacticalAI
{
    public static class TacticalAISettings
    {
        public static readonly string[] ModeValues = { "legacy", "reference", "overlord" };
        public static readonly string[] SkillValues = { "S1", "S2", "S3", "S4", "S5" };
        public static readonly string[] ExplainLevelValues = { "none", "post_hoc", "full" };

        // ---- defaults: OFF, legacy, printed rules only -------------------
        public const bool DefaultEnabled = false;
        // dev 20: no stored mode dial; kept as legacy token for missions/tests.
        public const string DefaultMode = "legacy";
        public const string DefaultSkillLevel = "S1";
        // M13h (Thomas 28.10): default is 'none' - explanations must be asked
        // for, not served unprompted. Post-hoc/full stay selectable per row.
        public const string DefaultExplainLevel = "none";
        // M13c (Thomas 30.10): the general RR info block (attack/door/basic-
        // attack rule) under the action blocks gets its own switch, like the
        // explanation. DEFAULT ON: Thomas explicitly wants it visible by
        // default; OFF renders the activation without the info block.
        public const bool DefaultInfoText = true;

        static bool _enabled = DefaultEnabled;
        // Dev 20 (option A, Thomas): the procedural mode is DERIVED from the
        // difficulty ladder - two switches for one screw was the flaw he
        // named. S1-S3 = reference (book execution, growing discretion
        // quality), S4-S5 = overlord (score optimisation on top). The Mode
        // setter survives only as an explicit test/console override seam.
        static string _modeOverride;
        static string _skillLevel = DefaultSkillLevel;
        static string _explainLevel = DefaultExplainLevel;
        static bool _infoText = DefaultInfoText;

        static Func<string, string> _saveHook;   // (jsonToStore) -> storedJson|null

        /// <summary>Global master switch. FALSE by default: every hook falls
        /// through to the untouched legacy path.</summary>
        public static bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; Save(); }
        }

        /// <summary>Derived from the difficulty ladder (dev 20): "reference"
        /// for Rekrut..Veteran (S1-S3), "overlord" for Kommandant/General
        /// (S4-S5) - unless a test/console override is set. Never "legacy":
        /// legacy-ness is decided by Enabled/EffectiveMode.</summary>
        public static string Mode
        {
            get { return _modeOverride ?? (SkillRank >= 4 ? "overlord" : "reference"); }
        }

        /// <summary>Test/console seam: force a procedural mode independent of
        /// the ladder; null/empty restores derivation. Unknown values are
        /// ignored (never throw). "legacy" here is accepted for the mission-
        /// badge test paths only - the settings UI cannot produce it.</summary>
        public static void SetModeOverride(string mode)
        {
            if (string.IsNullOrEmpty(mode)) { _modeOverride = null; Save(); return; }
            _modeOverride = Validate(mode, ModeValues, null);
            Save();
        }

        public static bool HasModeOverride { get { return _modeOverride != null; } }

        /// <summary>1..5 for "S1".."S5" (unknown -> 1).</summary>
        public static int SkillRank
        {
            get { int i = IndexOf(_skillLevel, SkillValues); return i < 0 ? 1 : i + 1; }
        }

        /// <summary>"S1".."S5" - the error-band skill of the reference level
        /// (concept 3.2). Only consulted when Mode != "legacy".</summary>
        public static string SkillLevel
        {
            get { return _skillLevel; }
            set { _skillLevel = Validate(value, SkillValues, DefaultSkillLevel); Save(); }
        }

        /// <summary>"none" | "post_hoc" (round summary after the round) |
        /// "full" (per-activation why-strings). Printed info only.</summary>
        public static string ExplainLevel
        {
            get { return _explainLevel; }
            set { _explainLevel = Validate(value, ExplainLevelValues, DefaultExplainLevel); Save(); }
        }

        /// <summary>M13c: show/hide the general RR info block (one merged
        /// paragraph: basic attack rule + attack line + door law) under the
        /// action blocks. TRUE by default; independent of ExplainLevel.</summary>
        public static bool InfoText
        {
            get { return _infoText; }
            set { _infoText = value; Save(); }
        }

        /// <summary>Personas (Thomas 02.11.): global DEFAULT persona for
        /// campaigns that carry no explicit choice. "" = none (the exact
        /// pre-persona behaviour); "none" is accepted as a synonym so the
        /// settings row and the campaign field speak the same language.
        /// The asset is the validator (PersonaState applies it) - the
        /// settings layer never hardcodes persona ids.</summary>
        public static string PersonaDefault
        {
            get { return _personaDefault; }
            set { _personaDefault = string.IsNullOrEmpty(value) ? "" : value; Save(); }
        }
        static string _personaDefault = "";

        /// <summary>Injected by the Unity side: the hook receives the
        /// serialized settings, stores them, and returns the previously
        /// stored JSON (or null when nothing was stored yet).</summary>
        public static void SetSaveHook(Func<string, string> hook) { _saveHook = hook; }

        /// <summary>Push current state through the save hook (no-op when no
        /// hook is installed, e.g. in the console harness).</summary>
        public static void Save()
        {
            if (_saveHook == null) return;
            _saveHook(ToJson());
        }

        /// <summary>Pull state through the save hook (pass null to read).
        /// Invalid/unknown fields keep their defaults; nothing throws.</summary>
        public static void Load()
        {
            if (_saveHook == null) return;
            string json = _saveHook(null);
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var d = Core.Json.AsDict(Core.Json.Parse(json));
                _enabled = Core.Json.AsBool(Core.Json.Get(d, "enabled", (object)DefaultEnabled));
                // "mode" is no longer a stored dial (dev 20 derivation); only
                // an explicit override round-trips, and only when valid.
                _modeOverride = Validate(Core.Json.AsStr(Core.Json.Get(d, "modeOverride", "")), ModeValues, null);
                _skillLevel = Validate(Core.Json.AsStr(Core.Json.Get(d, "skill", DefaultSkillLevel)), SkillValues, DefaultSkillLevel);
                // M13g (Thomas 28.10): the deployment-mode switch is GONE -
                // the app is a companion to the board game and deployments
                // are ALWAYS OPEN. Old save files may still carry a
                // "deployment" field: Load reads ONLY the named fields below
                // via Json.Get (TryGetValue) and never enumerates unknown
                // keys, so the stale key is silently ignored - no throw, no
                // warning, no effect on any state. The harness pins this
                // route in settings_load.json.
                _explainLevel = Validate(Core.Json.AsStr(Core.Json.Get(d, "explain", DefaultExplainLevel)), ExplainLevelValues, DefaultExplainLevel);
                _infoText = Core.Json.AsBool(Core.Json.Get(d, "info", (object)DefaultInfoText));
                // Personas (02.11.): free-form id - validity (asset membership)
                // is applied by PersonaState; the settings layer only stores.
                var pd = Core.Json.AsStr(Core.Json.Get(d, "persona", (object)""));
                _personaDefault = pd ?? "";
            }
            catch (Exception)
            {
                ResetDefaults();   // corrupt save data must NEVER enable the mode
            }
        }

        public static void ResetDefaults()
        {
            _enabled = DefaultEnabled;
            _modeOverride = null;
            _skillLevel = DefaultSkillLevel;
            _explainLevel = DefaultExplainLevel;
            _infoText = DefaultInfoText;
            _personaDefault = "";
        }

        /// <summary>Per-mission override resolution (concept 4.2.7): the
        /// campaign mission card stores CampaignStructure.aiMode; empty or
        /// unknown values fall back to the global Mode. The override can
        /// only pick among the tactical levels - "legacy" always means the
        /// untouched original code path.</summary>
        public static string EffectiveMode(string missionAiMode)
        {
            if (!_enabled) return "legacy";
            if (string.IsNullOrEmpty(missionAiMode)) return Mode;
            if (missionAiMode == "off") return "legacy";
            if (IsMissionSkillToken(missionAiMode))
                return RankOfToken(missionAiMode) >= 4 ? "overlord" : "reference";
            return Validate(missionAiMode, ModeValues, Mode);
        }

        // ------------------------------------------------- mission badges (dev 21)
        // The per-mission badge cycles the difficulty LADDER: "" (AUTO:
        // follow the global settings) -> "off" -> S1..S5 -> AUTO (Thomas:
        // after the dev 20 merge the badge must offer the same knob the
        // settings do). "off" (not "legacy") is the deliberate-OFF token,
        // because untouched old campaigns carry "legacy" as a field default
        // and SagaCampaign normalizes that to AUTO on load - reusing it
        // would silently erase a chosen OFF on every campaign reload.
        public static readonly string[] MissionSkillTokens = { "S1", "S2", "S3", "S4", "S5" };

        /// <summary>Badge cycle order: AUTO -> OFF -> S1..S5 -> AUTO.</summary>
        public static string NextMissionToken(string current)
        {
            if (string.IsNullOrEmpty(current)) return "off";
            if (current == "off") return "S1";
            int i = IndexOf(current, MissionSkillTokens);
            if (i < 0) return "off";                  // legacy/ref/ord -> OFF
            return i + 1 >= MissionSkillTokens.Length ? "" : MissionSkillTokens[i + 1];
        }

        public static bool IsMissionSkillToken(string token)
        {
            return IndexOf(token, MissionSkillTokens) >= 0;
        }

        /// <summary>Skill the mission plays at: its own ladder rung if the
        /// badge picked one, else the global ladder ("" AUTO or legacy tokens).</summary>
        public static string EffectiveSkill(string missionAiMode)
        {
            return IsMissionSkillToken(missionAiMode) ? missionAiMode : _skillLevel;
        }

        public static int RankOfToken(string token)
        {
            int i = IndexOf(token, MissionSkillTokens);
            return i < 0 ? 0 : i + 1;
        }

        public static bool IsTactical(string mode)
        {
            return _enabled && mode != null && mode != "legacy" && IndexOf(mode, ModeValues) >= 0;
        }

        // ------------------------------------------------------------------ JSON

        public static string ToJson()
        {
            var sb = new StringBuilder();
            sb.Append('{');
            sb.Append("\"enabled\":").Append(_enabled ? "true" : "false").Append(',');
            Kv(sb, "mode", Mode); Kv(sb, "skill", _skillLevel);
            if (_modeOverride != null) Kv(sb, "modeOverride", _modeOverride);
            sb.Append("\"explain\":\"").Append(_explainLevel).Append("\",");
            sb.Append("\"info\":").Append(_infoText ? "true" : "false");
            if (!string.IsNullOrEmpty(_personaDefault))
                sb.Append(",\"persona\":\"").Append(_personaDefault).Append('"');
            sb.Append('}');
            return sb.ToString();
        }

        static void Kv(StringBuilder sb, string key, string val)
        {
            sb.Append('"').Append(key).Append("\":\"").Append(val).Append("\",");
        }

        static string Validate(string value, string[] allowed, string fallback)
        {
            if (IndexOf(value, allowed) >= 0) return value;
            return fallback;
        }

        static int IndexOf(string value, string[] allowed)
        {
            if (value == null) return -1;
            for (int i = 0; i < allowed.Length; i++)
                if (string.Equals(allowed[i], value, StringComparison.Ordinal)) return i;
            return -1;
        }
    }
}
