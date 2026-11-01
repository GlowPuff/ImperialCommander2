// M13u section (variant c, Thomas 01.11.): the per-group doctrine dice on
// the SHIPPED tactical_pattern_weights.json. Authored invariants only -
// row sums, defend-modifier arithmetic (hand-computed expect blocks in the
// fixture), full shipped-roster coverage (enemies.json + villains.json -
// the same files the game loads), printed-law filter, per-round draw
// determinism + replay, reachability of every weighted pattern, the
// authored monotone law "defend raises the guard/blocker share" and the
// no-asset fallback (empty table = legacy branch authoritative).
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TacticalAI.Core;

namespace TacticalAiHarness
{
    public static partial class Program
    {
        static ulong TestSeed(int i) { return PatternWeightsTwinSeedBase + (ulong)i * 0x9E3779B97F4A7C15UL; }
        static readonly ulong PatternWeightsTwinSeedBase =
            0x0123456789ABCDEFUL;

        static List<Dictionary<string, object>> ShippedCardRecords()
        {
            var all = new List<Dictionary<string, object>>();
            foreach (var f in new[] { "enemies.json", "villains.json" })
            {
                // CardData dir, same trailing-comma strip as SectionsPersonas.
                var txt = System.Text.RegularExpressions.Regex.Replace(
                    File.ReadAllText(Path.Combine(LanguagesDir(), "..",
                                                 "CardData", f)),
                    @",\s*([}\]])", "$1");
                foreach (var raw in Json.AsList(Json.Parse(txt)))
                {
                    var d = raw as Dictionary<string, object>;
                    if (d != null) all.Add(d);
                }
            }
            return all;
        }

        static void SecPatternWeights()
        {
            var v = LoadJson("pattern_weights.json");
            PatternWeights.PrimeDoc(Json.AsDict(LoadLangJson("En", "tactical_pattern_weights.json")));
            var t = PatternWeights.Load();
            if (!t.IsActive) throw new InvalidOperationException("pattern-weight asset empty");

            Check("row_count", Json.AsLong(v["row_count"]), (long)t.Rows.Count);

            // ------------------------------------------------- modifier row
            var dm = t.PostureMods["defend"];
            foreach (var kv in Json.AsDict(v["posture_mod_defend"]))
                Check("defend_mod[" + kv.Key + "]", Json.AsLong(kv.Value), (long)dm[kv.Key]);

            // ------------------------------------- effective distributions
            foreach (var raw in Json.AsList(v["effective_cases"]))
            {
                var c = Json.AsDict(raw);
                var id = Json.AsStr(c["id"]);
                var rec = ShippedCardRecords().First(r => Json.AsStr(r["id"]) == id);
                var dist = t.Effective(t.WeightsFor(id, Json.AsStr(rec["name"])),
                                       Json.AsStr(c["posture"]));
                var want = new SortedDictionary<string, long>();
                foreach (var kv in Json.AsDict(c["expect"])) want[kv.Key] = Json.AsLong(kv.Value);
                var got = new SortedDictionary<string, long>();
                foreach (var kv in dist) got[kv.Key] = kv.Value;
                Check("effective[" + id + "|" + Json.AsStr(c["posture"]) + "]",
                      string.Join(",", want.Select(kv => kv.Key + ":" + kv.Value)),
                      string.Join(",", got.Select(kv => kv.Key + ":" + kv.Value)));
            }

            // ---------------------------------------- row hygiene + sums
            for (int i = 0; i < t.Rows.Count; i++)
            {
                var r = t.Rows[i];
                Check("row[" + i + "].sum", 100L, r.Weights.Values.Sum(x => (long)x));
                foreach (var k in r.Weights.Keys)
                    Check("row[" + i + "].pattern[" + k + "]", true, Patterns.IsPattern(k));
            }
            Check("eweb_guard_only", "guard:100",
                  string.Join(",", new SortedDictionary<string, int>(t.Rows[0].Weights)
                      .Select(kv => kv.Key + ":" + kv.Value)));

            // ------------------------------------------------- full coverage
            var cards = ShippedCardRecords();
            Check("shipped_roster_size", 89L, (long)cards.Count);
            foreach (var rec in cards)
            {
                var id = Json.AsStr(rec["id"]);
                var dist = t.WeightsFor(id, Json.AsStr(rec["name"]));
                Check("covers[" + id + "]", true, dist != null && dist.Count > 0);
            }

            // --------------------------------- printed-law filter (massive)
            var big = t.Effective(t.WeightsFor("DG001", "Stormtrooper"), "hunt");
            big.Remove("aggressive");
            big["hunter"] = 30;
            var filt = PatternWeightTable.FilterIllegal(big, true);
            Check("massive_filter_drops_hunter", false, filt.ContainsKey("hunter"));

            // --------------------------------- determinism + replay (round)
            foreach (var rec in cards)
            {
                var id = Json.AsStr(rec["id"]);
                var dist = t.Effective(t.WeightsFor(id, Json.AsStr(rec["name"])), "hunt");
                for (int round = 1; round <= 3; round++)
                {
                    int draw;
                    var a = t.Roll(dist, TestSeed(7), round, id, out draw);
                    var b = t.Roll(dist, TestSeed(7), round, id, out draw);
                    Check("replay[" + id + "|r" + round + "]", a, b);
                    Check("legal[" + id + "|r" + round + "]", true, Patterns.IsPattern(a));
                }
            }

            // --------------------------------------------------- reachable
            foreach (var raw in Json.AsList(v["reach_cases"]))
            {
                var c = Json.AsDict(raw);
                var id = Json.AsStr(c["id"]);
                var posture = Json.AsStr(c["posture"]);
                var rec2 = ShippedCardRecords().First(r => Json.AsStr(r["id"]) == id);
                var dist = t.Effective(t.WeightsFor(id, Json.AsStr(rec2["name"])), posture);
                var seen = new HashSet<string>();
                for (int i = 0; i < 4000; i++)
                {
                    int draw;
                    seen.Add(t.Roll(dist, TestSeed(i), (i % 13) + 1, id, out draw));
                }
                foreach (var praw in Json.AsList(c["patterns"]))
                    Check("reachable[" + id + "|" + posture + "|" + Json.AsStr(praw) + "]",
                          true, seen.Contains(Json.AsStr(praw)));
            }

            // ------------- monotone law: defend RAISES the hold share (DM)
            var trooper = ShippedCardRecords().First(r => Json.AsStr(r["id"]) == "DG001");
            var dHunt = t.Effective(t.WeightsFor("DG001", Json.AsStr(trooper["name"])), "hunt");
            var dDef = t.Effective(t.WeightsFor("DG001", Json.AsStr(trooper["name"])), "defend");
            int holdHunt = 0, holdDef = 0;
            for (int i = 0; i < 4000; i++)
            {
                int draw;
                var ph = t.Roll(dHunt, TestSeed(i), (i % 13) + 1, "DG001", out draw);
                if (ph == "guard" || ph == "blocker") holdHunt++;
                var pd = t.Roll(dDef, TestSeed(i), (i % 13) + 1, "DG001", out draw);
                if (pd == "guard" || pd == "blocker") holdDef++;
            }
            Check("defend_hold_share_rises", true, holdDef > holdHunt);

            // --------------------------------------- no-asset fallback law
            PatternWeights.PrimeDoc(null);
            var empty = PatternWeights.Load();
            Check("fallback_inactive", false, empty.IsActive);
            Check("fallback_no_row", false,
                  empty.WeightsFor("DG001", "Stormtrooper") != null);
            // restore the shipped doc for any later consumer
            PatternWeights.PrimeDoc(Json.AsDict(LoadLangJson("En", "tactical_pattern_weights.json")));
        }
    }
}
