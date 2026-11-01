// M12b Imperial class-deck section: table integrity on the SHIPPED asset,
// the seed-derivation vectors, the start-of-campaign choices (exact deck id
// + full score tuple) and the load-backfill identity (old save without deck
// data -> two loads -> bit-identical choice). Expect blocks are authored from
// the documented rule via wiki_data/tools/tacai_deck_twin.py (see the
// fixture _comment); this section runs them against the C# original.
using System;
using System.Collections.Generic;
using System.Linq;
using TacticalAI.Core;

namespace TacticalAiHarness
{
    public static partial class Program
    {
        static void SecImperialDeck()
        {
            var v = LoadJson("imperial_deck.json");
            if (ImperialDecks.DocOrNull() == null)
                throw new InvalidOperationException("imperial deck asset not primed");
            var table = ImperialDecks.Load();

            // ------------------------------------------------ table integrity
            var tb = Json.AsDict(v["table"]);
            Check("table.deck_count", Json.AsLong(tb["deck_count"]), table.Decks.Count);
            Check("table.card_count", Json.AsLong(tb["card_count"]), table.CardCount);
            Check("table.deck_order",
                  Json.AsList(tb["deck_order"]).ConvertAll(x => (object)Json.AsStr(x)),
                  table.Decks.Select(d => (object)d.Id).ToList());
            var xpExpect = Json.AsList(tb["paid_xp_expect"])
                                 .ConvertAll<object>(x => Json.AsLong(x));
            foreach (var deck in table.Decks)
            {
                var basics = deck.BasicCards.Count();
                Check("table." + deck.Id + ".basic>=1", true, basics >= 1);
                var paid = deck.Cards.Where(c => !c.Basic && c.Xp.HasValue)
                                     .Select(c => c.Xp.Value).OrderBy(x => x).ToList();
                Check("table." + deck.Id + ".paid_xp", xpExpect, paid);
            }

            // ---------------------------------------------------- seed vectors
            var seeds = Json.AsDict(v["seeds"]);
            Check("seeds.fnv_empty_hex", Json.AsStr(seeds["fnv_empty_hex"]),
                  ImperialDecks.Fnv1a64("").ToString("x16"));
            foreach (var raw in Json.AsList(seeds["vectors"]))
            {
                var c = Json.AsDict(raw);
                var key = Json.AsStr(c["key"]);
                Check("seeds.vector[" + key + "]", Json.AsStr(c["expect"]),
                      ImperialDecks.SeedHex(ImperialDecks.DeckSeedFor(key)));
            }

            // ---------------------------------------------------- start cases
            foreach (var raw in Json.AsList(v["start_cases"]))
            {
                var c = Json.AsDict(raw);
                ChoiceRow(c);
            }

            // ---------------------------------------- collection gate cases
            // (Noldorion PR #194 review 2.11; owned_expansions is injected
            // by ChoiceRow - see the comment there)
            foreach (var raw in Json.AsList(v["gate_cases"]))
            {
                var c = Json.AsDict(raw);
                ChoiceRow(c);
            }

            // ----------------------------------- backfill: two loads, same pick
            foreach (var key in new[] { "backfill", "backfill_enriched", "empty_profile" })
            {
                var c = Json.AsDict(v[key]);
                ChoiceRow(c);
                // the backfill contract: a save WITHOUT deck data is loaded
                // twice; the re-derived choice must be bit-identical.
                var save = Json.AsDict(c["campaign_save"]);
                var seed = ImperialDecks.DeckSeedFor(Json.AsStr(save["GUID"]));
                var p1 = ImperialCampaignProfile.ProfileFromCampaignSave(save);
                var p2 = ImperialCampaignProfile.ProfileFromCampaignSave(save);
                var r1 = ImperialDecks.Choose(table, p1, seed).ToDict();
                var r2 = ImperialDecks.Choose(table, p2, seed).ToDict();
                Check(Json.AsStr(c["name"]) + ".reload_identical",
                      Canonical(r1), Canonical(r2));
            }
        }

        // One case: profile from the save-shaped block -> full choice against
        // the authored expect {seed, deck, scores:[{deck,synergy,noise,total}]}
        static void ChoiceRow(Dictionary<string, object> c)
        {
            var name = Json.AsStr(c["name"]);
            var save = Json.AsDict(c["campaign_save"]);
            var seed = ImperialDecks.DeckSeedFor(Json.AsStr(save["GUID"]));
            var profile = ImperialCampaignProfile.ProfileFromCampaignSave(save);
            // Collection gate vectors (Noldorion PR #194 review 2.11):
            // owned_expansions is NOT save data (it is runtime collection
            // state) - the vector injects it directly, mirroring how the
            // live adapter fills the profile from DataStore.ownedExpansions.
            if (c.ContainsKey("owned_expansions"))
            {
                profile.OwnedExpansions = new HashSet<string>(StringComparer.Ordinal);
                foreach (var e in Json.AsList(c["owned_expansions"]))
                    profile.OwnedExpansions.Add(Json.AsStr(e));
            }
            var choice = ImperialDecks.Choose(profile, seed);
            if (choice == null) { Fail(name, "Choose returned null"); return; }
            var got = choice.ToDict();

            var exp = Json.AsDict(c["expect"]);
            Check(name + ".seed", Json.AsStr(exp["seed"]), got["seed"]);
            Check(name + ".deck", Json.AsStr(exp["deck"]), got["deck"]);
            var expScores = new List<object>();
            foreach (var raw in Json.AsList(exp["scores"]))
            {
                var d = Json.AsDict(raw);
                expScores.Add(new Dictionary<string, object>
                {
                    { "deck", Json.AsStr(d["deck"]) },
                    { "synergy", Json.AsLong(d["synergy"]) },
                    { "noise", Json.AsLong(d["noise"]) },
                    { "total", Json.AsLong(d["total"]) },
                });
            }
            Check(name + ".scores", expScores, got["scores"]);
            // rank invariant: total desc, then deck id ascending (ordinal)
            var rows = Json.AsList(got["scores"]).ConvertAll(raw => Json.AsDict(raw));
            for (int i = 1; i < rows.Count; i++)
            {
                var a = rows[i - 1]; var b = rows[i];
                long ta = Json.AsLong(a["total"]), tbs = Json.AsLong(b["total"]);
                bool ok = ta > tbs || (ta == tbs &&
                    string.CompareOrdinal(Json.AsStr(a["deck"]), Json.AsStr(b["deck"])) < 0);
                Check(name + ".rank[" + i + "]", true, ok);
            }
        }
    }
}
