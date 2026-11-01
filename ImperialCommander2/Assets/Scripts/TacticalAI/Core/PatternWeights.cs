// M13u (Thomas 01.11., Variante c): the per-group doctrine dice. One rule
// engine for EVERY mission: each deployment group carries a printed-flavour
// weight distribution (tactical_pattern_weights.json, PUBLIC reviewable
// data - fairness rule, concept section 2), and the mission structure
// (posture "defend": a terminal or an escort target is in play) shifts it
// through DATA ROWS (postureModifiers), never through a code branch. The
// roll is per round, seeded (mission seed + card id + round): the same
// mission replays every draw, and EXPLAIN can disclose it.
//
// Layer order in the bridge (ClassifyPattern): printed movement law (Tripod)
// BEFORE the dice; the rolled pattern feeds the guard band and the persona
// multipliers unchanged. No asset / no matching row -> the legacy
// ClassifyPattern branch stays authoritative (bit-identical fallback).
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TacticalAI.Core
{
    public sealed class PatternWeightRow
    {
        public string IdMatch;        // exact card id (optional; then the row is id-only)
        public string NameMatch;      // case-insensitive regex over the internal name (optional)
        public Dictionary<string, int> Weights = new Dictionary<string, int>();

        /// Row authoring law: an id row hits exactly its card; a name row
        /// hits by regex; a row with neither is the ordered catch-all.
        /// Illegal regex never throws (it simply never matches).
        public bool Matches(string cardId, string name)
        {
            if (!string.IsNullOrEmpty(IdMatch))
                return string.Equals(IdMatch, cardId, StringComparison.Ordinal);
            if (string.IsNullOrEmpty(NameMatch)) return true;
            try
            {
                return Regex.IsMatch(name ?? "", NameMatch, RegexOptions.IgnoreCase);
            }
            catch (ArgumentException) { return false; }
        }
    }

    public sealed class PatternWeightTable
    {
        public List<PatternWeightRow> Rows = new List<PatternWeightRow>();
        public Dictionary<string, Dictionary<string, int>> PostureMods =
            new Dictionary<string, Dictionary<string, int>>();

        public bool IsActive { get { return Rows.Count > 0; } }

        /// First matching row wins (authored order = priority law, same as
        /// the persona effect order). Returns a FRESH copy - callers mutate
        /// it through Effective/Filter/Remove.
        public Dictionary<string, int> WeightsFor(string cardId, string name)
        {
            foreach (var r in Rows)
                if (r.Matches(cardId, name))
                    return new Dictionary<string, int>(r.Weights);
            return null;
        }

        /// Posture shift (the data row that replaced the defend code
        /// branch): additive per pattern, clamped at 0, zero rows dropped,
        /// no renormalisation needed (the roll works on raw weights). A
        /// POSITIVE mod may open a pattern the row never carried (the
        /// defend case can put a guard on a pure-attack roster - authored
        /// 20.11 concept: "Wache +20" makes it possible, not just likelier).
        /// Unknown posture -> untouched copy.
        public Dictionary<string, int> Effective(Dictionary<string, int> dist, string posture)
        {
            var d = new Dictionary<string, int>(dist ?? new Dictionary<string, int>());
            Dictionary<string, int> mods;
            if (string.IsNullOrEmpty(posture) || !PostureMods.TryGetValue(posture, out mods))
                return d;
            foreach (var kv in mods)
            {
                int cur;
                d.TryGetValue(kv.Key, out cur);
                int v = cur + kv.Value;
                if (v <= 0) d.Remove(kv.Key);
                else d[kv.Key] = v;
            }
            return d;
        }

        /// Printed-law filter: patterns the card CANNOT play. Massive
        /// bases never chase (hunter); Tripod is gated before the dice.
        public static Dictionary<string, int> FilterIllegal(Dictionary<string, int> dist,
                                                            bool isMassive)
        {
            if (dist == null) return null;
            if (isMassive) dist.Remove("hunter");
            return dist;
        }

        /// The per-round draw. Same hash family as the persona pattern flip
        /// (Fnv1a64 over a public key, XOR the mission seed, 53-bit draw).
        /// Deterministic pattern order (ordinal key sort) so the cumulative
        /// pick never depends on Dictionary enumeration order. Empty/zero
        /// distribution -> null (caller falls back to the legacy branch).
        public string Roll(Dictionary<string, int> dist, ulong missionSeed,
                           int roundNo, string cardId, out int drawPct)
        {
            drawPct = -1;
            if (dist == null || dist.Count == 0) return null;
            var keys = new List<string>(dist.Keys);
            keys.Sort(StringComparer.Ordinal);
            long total = 0;
            foreach (var k in keys) if (dist[k] > 0) total += dist[k];
            if (total <= 0) return null;
            var h = ImperialDecks.Fnv1a64("pw|" + (cardId ?? "") + "|r" + roundNo) ^ missionSeed;
            var draw = (h >> 11) * (1.0 / 9007199254740992.0);   // [0,1), 53 bits
            double target = draw * total;
            double acc = 0;
            foreach (var k in keys)
            {
                if (dist[k] <= 0) continue;
                acc += dist[k];
                if (target < acc)
                {
                    drawPct = (int)(draw * 100);
                    return k;
                }
            }
            drawPct = (int)(draw * 100);
            return keys[keys.Count - 1];        // float guard: last pattern
        }
    }

    /// Asset seam (PrimeDoc/DocOrNull pattern of the tactical modules): the
    /// Unity side loads the TextAsset, Core parses, the harness pins against
    /// the SHIPPED asset.
    public static class PatternWeights
    {
        static Dictionary<string, object> _doc;
        static PatternWeightTable _table;

        public static void PrimeDoc(Dictionary<string, object> doc)
        {
            _doc = doc;
            _table = null;      // re-parse on next Load (settings reload)
        }
        public static Dictionary<string, object> DocOrNull() { return _doc; }

        public static PatternWeightTable Load()
        {
            if (_table != null) return _table;
            var t = new PatternWeightTable();
            if (_doc != null)
            {
                object modsObj;
                // postureModifiers is an object keyed by posture name
                if (_doc.TryGetValue("postureModifiers", out modsObj) &&
                    modsObj is Dictionary<string, object>)
                    foreach (var kv in (Dictionary<string, object>)modsObj)
                    {
                        var row = kv.Value as Dictionary<string, object>;
                        if (row == null) continue;
                        var d = new Dictionary<string, int>();
                        foreach (var e in row)
                        {
                            if (e.Value == null) continue;
                            d[e.Key] = (int)Json.AsLong(e.Value);
                        }
                        t.PostureMods[kv.Key] = d;
                    }
                object rowsObj;
                if (_doc.TryGetValue("rows", out rowsObj) && rowsObj != null)
                    foreach (var raw in Json.AsList(rowsObj))
                    {
                        var d = raw as Dictionary<string, object>;
                        if (d == null) continue;
                        var r = new PatternWeightRow();
                        object v;
                        if (d.TryGetValue("id", out v) && v != null) r.IdMatch = Json.AsStr(v);
                        if (d.TryGetValue("nameMatch", out v) && v != null)
                            r.NameMatch = Json.AsStr(v);
                        if (d.TryGetValue("weights", out v) && v is Dictionary<string, object>)
                            foreach (var e in (Dictionary<string, object>)v)
                                if (e.Value != null && Patterns.IsPattern(e.Key))
                                    r.Weights[e.Key] = (int)Json.AsLong(e.Value);
                        if (r.Weights.Count > 0) t.Rows.Add(r);
                    }
            }
            _table = t;
            return _table;
        }
    }
}
