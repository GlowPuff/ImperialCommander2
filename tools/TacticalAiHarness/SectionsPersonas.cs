// Personas section: the (persona, threat) rank/cost/guard/free/signature
// rule and the deterministic doctrine-flip draw on the SHIPPED persona
// asset. Expect blocks are authored from the documented rule via
// wiki_data/tools/tacai_persona_twin.py ('authored, not recorded' route:
// two independent implementations per vector - if Core and the twin ever
// diverge, this section FAILs, it cannot silently agree). The roster for
// the shipped-roster cases is the same CardData/enemies.json the game loads
// for deploymentCards, read directly (trailing commas are MiniJson flavour
// and tolerated by the parser).
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TacticalAI.Core;

namespace TacticalAiHarness
{
    public static partial class Program
    {
        static void SecPersonas()
        {
            var v = LoadJson("personas.json");
            // seam: pin against the SHIPPED asset, never a fixture copy
            // (same doctrine as the M12 asset primings; LoadLangJson is the
            // trailing-comma-tolerant reader).
            Personas.PrimeDoc(Json.AsDict(LoadLangJson("En", "tactical_personas.json")));
            var table = Personas.Load();
            if (table.Personas.Count == 0)
                throw new InvalidOperationException("persona asset empty/not primed");

            // ---------------------------------------------------- rank ladder
            foreach (var raw in Json.AsList(v["rank_cases"]))
            {
                var c = Json.AsDict(raw);
                var view = table.Resolve(Json.AsStr(c["persona"]), Json.AsInt(c["threat"]));
                Check("rank[" + Json.AsStr(c["persona"]) + "@" + Json.AsInt(c["threat"]) + "]",
                      Json.AsLong(c["expect_rank"]), view.Rank);
                Check("rank_effects[" + Json.AsStr(c["persona"]) + "@" + Json.AsInt(c["threat"]) + "]",
                      Json.AsLong(c["expect_active_count"]), view.Active.Count);
            }

            // ------------------------------------------------------- costs
            foreach (var raw in Json.AsList(v["cost_cases"]))
            {
                var c = Json.AsDict(raw);
                var view = table.Resolve(Json.AsStr(c["persona"]), Json.AsInt(c["threat"]));
                Check("cost[" + Json.AsStr(c["persona"]) + "@" + Json.AsInt(c["threat"])
                      + "|" + Json.AsStr(c["id"]) + "]",
                      Json.AsLong(c["expect"]),
                      view.CostFor(Json.AsStr(c["name"]), Json.AsStr(c["faction"]),
                                   Json.AsInt(c["printed"])));
            }

            // ------------------------------------------------------- guards
            foreach (var raw in Json.AsList(v["guard_cases"]))
            {
                var c = Json.AsDict(raw);
                var view = table.Resolve(Json.AsStr(c["persona"]), Json.AsInt(c["threat"]));
                Check("guard[" + Json.AsStr(c["persona"]) + "@" + Json.AsInt(c["threat"])
                      + "|" + Json.AsStr(c["name"]) + "]",
                      Json.AsLong(c["expect"]),
                      view.GuardPercentFor(Json.AsStr(c["name"]), Json.AsStr(c["faction"])));
            }

            // -------------------------------------------- free (shipped hand)
            var roster = ShippedRosterRows();
            foreach (var raw in Json.AsList(v["free_cases"]))
            {
                var c = Json.AsDict(raw);
                var view = table.Resolve(Json.AsStr(c["persona"]), Json.AsInt(c["threat"]));
                Check("free[" + Json.AsStr(c["persona"]) + "@" + Json.AsInt(c["threat"]) + "]",
                      Json.AsLong(c["expect"]), view.FreeCreditCost(roster));
            }

            // -------------------------------------------- free (synthetic)
            var synth = Json.AsDict(v["synth_cases"]);
            var synthRows = new List<string[]>();
            foreach (var rraw in Json.AsList(synth["rows"]))
            {
                var cells = new List<string>();
                foreach (var cell in Json.AsList(rraw)) cells.Add(Json.AsStr(cell));
                synthRows.Add(cells.ToArray());
            }
            foreach (var raw in Json.AsList(synth["cases"]))
            {
                var c = Json.AsDict(raw);
                var view = table.Resolve(Json.AsStr(c["persona"]), Json.AsInt(c["threat"]));
                Check("free_synth[" + Json.AsStr(c["persona"]) + "@" + Json.AsInt(c["threat"]) + "]",
                      Json.AsLong(c["expect"]), view.FreeCreditCost(synthRows));
            }

            // ---------------------------------------------------- signatures
            foreach (var raw in Json.AsList(v["signature_cases"]))
            {
                var c = Json.AsDict(raw);
                var view = table.Resolve(Json.AsStr(c["persona"]), Json.AsInt(c["threat"]));
                var got = Json.AsStr(c["expect"]);
                Check("signature[" + Json.AsStr(c["persona"]) + "@" + Json.AsInt(c["threat"]) + "]",
                      got ?? "", view.SignatureKey() ?? "");
            }

            // ------------------------------------------------- pattern flips
            foreach (var raw in Json.AsList(v["pattern_flip_cases"]))
            {
                var c = Json.AsDict(raw);
                var view = table.Resolve(Json.AsStr(c["persona"]), Json.AsInt(c["threat"]));
                var seed = Convert.ToUInt64(Json.AsStr(c["seed_hex"]), 16);
                Check("flip[" + Json.AsStr(c["persona"]) + "|" + Json.AsStr(c["id"])
                      + "|" + Json.AsStr(c["seed_hex"]).Substring(12)
                      + "|r" + Json.AsInt(c["round"]) + "|" + Json.AsStr(c["base"]) + "]",
                      Json.AsStr(c["expect"]),
                      view.PatternFor(Json.AsStr(c["name"]), Json.AsStr(c["faction"]),
                                      Json.AsStr(c["id"]), Json.AsStr(c["base"]),
                                      seed, Json.AsInt(c["round"])));
            }
        }

        // (internalName, faction, cost-as-text) rows of the shipped deploy
        // roster - the exact public deploy-hand data FreeCreditCost reads.
        // Same file the game loads for deploymentCards (CardData/enemies.json).
        static List<string[]> ShippedRosterRows()
        {
            // enemies.json is legacy JS-export flavour: trailing commas can
            // sit in ARRAYS too (surges/keywords) - MiniJson's object-only
            // tolerance is not enough here, so strip the same way LoadLangJson
            // does (full [}\]]-class regex).
            var txt = System.Text.RegularExpressions.Regex.Replace(
                File.ReadAllText(Path.Combine(LanguagesDir(), "..",
                                             "CardData", "enemies.json")),
                @",\s*([}\]])", "$1");
            var root = Json.Parse(txt);
            var rows = new List<string[]>();
            foreach (var raw in Json.AsList(root))
            {
                var d = raw as Dictionary<string, object>;
                if (d == null) continue;
                object name, faction, cost;
                if (!d.TryGetValue("name", out name) || name == null) continue;
                d.TryGetValue("faction", out faction);
                d.TryGetValue("cost", out cost);
                rows.Add(new[] { Json.AsStr(name),
                    faction == null ? "" : Json.AsStr(faction),
                    cost == null ? "0" : Json.AsLong(cost).ToString() });
            }
            return rows;
        }
    }
}
