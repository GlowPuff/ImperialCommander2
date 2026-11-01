// M12b Imperial class decks (FFG Rules Reference, campaign setup step 3
// "Choose Imperial Class"): the Imperial side of a campaign picks ONE class
// deck; its basic card (xp null = 0 XP) is in play from that moment on, and
// further cards are bought with Imperial XP in the Imperial Upgrade Stage
// between missions. The rule-true deck choice of the TACTICAL mode is
// computed here, deterministically, from the campaign profile plus the
// campaign-derived deck seed - never UnityEngine.Random, never wall time.
//
// Data source: Resources/Languages/En/tactical_imperial_decks.json
// (schema tacai.imperial_decks/1; 1:1 projection of
// wiki_data/imperial_class_cards.json by wiki_data/tools/import_imperial_decks.py;
// deck texts verbatim per wiki_data/LICENSE_DATA.md Punkt 5 - printed card
// text is board-game content and stays a build asset, Thomas-Doktrin M11g).
//
// SCORE MODEL (all features are countable facts from the campaign save, all
// weights are fixed integers - the same table in the M12b canon fixture):
//   synergy(deck) =
//       3 * sum over UNIT families {TROOPER, VEHICLE, DROID, HEAVY WEAPON,
//           MINE, OFFICER, GUARDIAN, COMMANDER}:
//           min(#villains of that family in the campaign, 4)
//         * min(#deck cards mentioning that family, 4)
//     + 2 * min(heroCount, 4)   * min(deck HERO mentions, 4)   // anti-hero decks
//     + 1 * min(heroCount, 4)   * min(deck REBEL mentions, 4)  // reaction value
//     + 2 * min(agendaMissions, 4) * min(deck AGENDA mentions, 4) // mission kinds
//     + 1 * min(missionCount, 8)   * min(deck MISSION mentions, 4)
//   noise(deck)   = DeterministicRandom(seed ^ Fnv1a64("noise|" + deckId))
//                   .RandBelow(4)              // 0..3: bounded tie jitter only
//   total(deck)   = synergy + noise
// The unit-family term is the strategy fit: cards that buff/attach to the
// families the Overlord actually fields across the campaign are worth more.
// HERO/REBEL scale with the campaign's hero count (more rebels on the board
// = more triggers for reaction and anti-hero cards). AGENDA/MISSION carry
// the chosen campaign's mission kinds; the Era-1 corpus contains no AGENDA
// token, so that term is honestly 0 for every Era-1 deck (rule slot kept
// complete, pinned in canon so a future corpus cannot silently shift it).
// noise < 4 can only flip near-ties, and the ranking is fully re-derivable:
// same seed + same campaign => bit-identical choice (load-backfill).
//
// The chosen deck persists in the campaign store (Saga.ImperialDeckState);
// "no deck" (null state) is a VALID state: non-tactical campaigns never
// store AI-owned deck data, and the UI may preview this pure choice without
// saving anything.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TacticalAI.Core
{
    /// <summary>One printed Imperial class-deck card (asset fields 1:1).</summary>
    public sealed class ImperialClassCard
    {
        public string Id;                      // "<deck_slug>__<card_slug>"
        public string Name;
        public bool Basic;                     // basic card: lies face-up on deck choice
        public long? Xp;                       // null = basic (0 XP); else 1/2/3/4
        public string Restriction;             // e.g. "Attachment: Trooper only"
        public string Keyword;                 // italic keyword, e.g. "TROOPER ONLY"
        public List<string> AbilityTexts = new List<string>();
        public bool NeedsReview;               // M12a honesty flag (NOT weighted)
    }

    /// <summary>One of the 12 Era-1 Imperial class decks.</summary>
    public sealed class ImperialClassDeck
    {
        public string Id;                      // slug of the printed deck name
        public string Name;
        // Noldorion PR #194 review 2.11: the content box that printed this
        // deck, as the app's Expansion enum code ("Core", "Jabba", ...).
        // Asset-derived (importer, watermarked raw-URL provenance); missing
        // or unknown field values degrade to "Core" (always eligible).
        public string Expansion = "Core";
        public List<ImperialClassCard> Cards = new List<ImperialClassCard>();

        public IEnumerable<ImperialClassCard> BasicCards
        {
            get { return Cards.Where(c => c.Basic); }
        }
    }

    /// <summary>Loaded tacai.imperial_decks/1 table (deck order = source order,
    /// which the importer pins to printed-name order).</summary>
    public sealed class ImperialClassDeckTable
    {
        public List<ImperialClassDeck> Decks = new List<ImperialClassDeck>();
        public int CardCount { get { return Decks.Sum(d => d.Cards.Count); } }

        public ImperialClassDeck Find(string id)
        {
            return Decks.FirstOrDefault(d => d.Id == id);
        }

        public static ImperialClassDeckTable FromDoc(object doc)
        {
            var table = new ImperialClassDeckTable();
            if (doc == null) return table;
            var root = Json.AsDict(doc);
            foreach (var rawDeck in Json.AsList(root["decks"]))
            {
                var dd = Json.AsDict(rawDeck);
                var deck = new ImperialClassDeck
                {
                    Id = Json.AsStr(dd["id"]),
                    Name = Json.AsStr(dd["name"]),
                };
                if (dd.ContainsKey("expansion") && dd["expansion"] != null)
                    deck.Expansion = Json.AsStr(dd["expansion"]);
                foreach (var rawCard in Json.AsList(dd["cards"]))
                {
                    var dc = Json.AsDict(rawCard);
                    var card = new ImperialClassCard
                    {
                        Id = Json.AsStr(dc["id"]),
                        Name = Json.AsStr(dc["name"]),
                        Basic = dc.ContainsKey("basic") && Json.AsBool(dc["basic"]),
                        Xp = dc.ContainsKey("xp") && dc["xp"] != null
                             ? (long?)Json.AsLong(dc["xp"]) : null,
                        Restriction = dc.ContainsKey("restriction")
                                      && dc["restriction"] != null
                                      ? Json.AsStr(dc["restriction"]) : null,
                        Keyword = dc.ContainsKey("keyword") && dc["keyword"] != null
                                  ? Json.AsStr(dc["keyword"]) : null,
                        NeedsReview = dc.ContainsKey("needs_review")
                                      && Json.AsBool(dc["needs_review"]),
                    };
                    if (dc.ContainsKey("abilities"))
                        foreach (var rawAb in Json.AsList(dc["abilities"]))
                        {
                            var da = Json.AsDict(rawAb);
                            card.AbilityTexts.Add(da.ContainsKey("text") && da["text"] != null
                                                  ? Json.AsStr(da["text"]) : "");
                        }
                    deck.Cards.Add(card);
                }
                table.Decks.Add(deck);
            }
            return table;
        }
    }

    /// <summary>The campaign facts the deck choice reads. Built by the Unity
    /// adapter from SagaCampaign (heroes, resolved villain families, mission
    /// kinds) and by the harness from the fixture's save-shaped block - the
    /// SAME mapping function (ProfileFromCampaignSave) so the canon exercises
    /// the real mapping; the live adapter only enriches villain families with
    /// the app's own trait table before handing the profile over.</summary>
    public sealed class ImperialCampaignProfile
    {
        public int HeroCount;
        public Dictionary<string, int> VillainFamilies =
            new Dictionary<string, int>(StringComparer.Ordinal);   // family -> villain count
        public int MissionCount;
        public int AgendaMissionCount;
        // Noldorion PR #194 review 2.11: the content boxes the player's
        // collection activated (app Expansion enum names). Choose() then
        // only offers decks whose box is owned. NULL = content ownership
        // unknown (console harness/fixtures, saves without the enrichment):
        // every deck stays eligible - the canon vectors predate the gate
        // and pin the ungated behaviour. The live Unity adapter fills it
        // from DataStore.ownedExpansions (Core always present).
        public HashSet<string> OwnedExpansions;

        public int VillainFamilyCount(string family)
        {
            int n;
            return VillainFamilies.TryGetValue(family, out n) ? n : 0;
        }

        // Saga.MissionType in CampaignSerialization order (Interfaces-Enums.cs:
        // Story, Side, Forced, Introduction, Interlude, Finale). Saves store
        // the enum as int. NOTE: this enum has NO Agenda member - agenda
        // missions are flagged via isAgendaMission (+ agendaType Rebel/
        // Imperial), never via missionType. The "Agenda" name still maps for
        // hand-written profile inputs; a real save can never produce it.
        static readonly string[] MissionTypeNames =
        {
            "Story", "Side", "Forced", "Introduction", "Interlude", "Finale",
        };

        /// <summary>Map a save-shaped campaign block (GUID / campaignHeroes /
        /// campaignVillains / campaignStructure, Newtonsoft field names) onto
        /// the profile. Reads ONLY the fields a real save carries
        /// (campaignVillains serialize WITHOUT traits - [JsonIgnore] - so the
        /// family signal from a save is the printed card name; the live Unity
        /// adapter may add the DataStore-resolved trait families on top,
        /// which is an enrichment, never a re-derivation).</summary>
        public static ImperialCampaignProfile ProfileFromCampaignSave(object saveDict)
        {
            var p = new ImperialCampaignProfile();
            if (saveDict == null) return p;
            var d = Json.AsDict(saveDict);

            if (d.ContainsKey("campaignHeroes") && d["campaignHeroes"] != null)
                foreach (var raw in Json.AsList(d["campaignHeroes"]))
                {
                    var hd = Json.AsDict(raw);
                    if (hd.ContainsKey("heroID") && hd["heroID"] != null
                        && Json.AsStr(hd["heroID"]).Length > 0)
                        p.HeroCount++;
                }

            if (d.ContainsKey("campaignVillains") && d["campaignVillains"] != null)
                foreach (var raw in Json.AsList(d["campaignVillains"]))
                {
                    var vd = Json.AsDict(raw);
                    var name = vd.ContainsKey("name") && vd["name"] != null
                               ? Json.AsStr(vd["name"]) : "";
                    var fams = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var kv in ImperialDecks.FamilyCounts(name)) fams.Add(kv.Key);
                    // explicit families ride along ONLY when stated (adapter
                    // enrichment seam; absent in real saves)
                    if (vd.ContainsKey("families") && vd["families"] != null)
                        foreach (var rawF in Json.AsList(vd["families"]))
                            fams.Add(Json.AsStr(rawF));
                    foreach (var f in fams)
                    {
                        int n;
                        p.VillainFamilies.TryGetValue(f, out n);
                        p.VillainFamilies[f] = n + 1;
                    }
                }

            if (d.ContainsKey("campaignStructure") && d["campaignStructure"] != null)
                foreach (var raw in Json.AsList(d["campaignStructure"]))
                {
                    var sd = Json.AsDict(raw);
                    p.MissionCount++;
                    var type = "";
                    if (sd.ContainsKey("missionType") && sd["missionType"] != null)
                    {
                        if (sd["missionType"] is string ms) type = ms;
                        else
                        {
                            int mi = (int)Json.AsLong(sd["missionType"]);
                            type = mi >= 0 && mi < MissionTypeNames.Length
                                   ? MissionTypeNames[mi] : "";
                        }
                    }
                    bool agenda = type == "Agenda"
                        || (sd.ContainsKey("isAgendaMission") && Json.AsBool(sd["isAgendaMission"]));
                    if (agenda) p.AgendaMissionCount++;
                }
            return p;
        }
    }

    /// <summary>One ranked score row; the full list is the persisted
    /// Begruendung (Begründung) of the choice.</summary>
    public sealed class ImperialDeckScoreRow
    {
        public string Deck;
        public long Synergy;
        public long Noise;
        public long Total;

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>
            {
                { "deck", Deck },
                { "synergy", Synergy },
                { "noise", Noise },
                { "total", Total },
            };
        }
    }

    /// <summary>Result of one deterministic choice: chosen deck + full ranked
    /// score tuple + human-readable reason (model data, not a UI string).</summary>
    public sealed class ImperialDeckChoice
    {
        public string SeedHex;
        public string DeckId;
        public string Reason;
        public List<ImperialDeckScoreRow> Scores = new List<ImperialDeckScoreRow>();

        public Dictionary<string, object> ToDict()
        {
            var rows = new List<object>();
            foreach (var s in Scores) rows.Add(s.ToDict());
            return new Dictionary<string, object>
            {
                { "seed", SeedHex },
                { "deck", DeckId },
                { "scores", rows },
            };
        }
    }

    /// <summary>Doc cache with the HeroProfiles/HeroCards seam: harness and
    /// live gate PRIME from file, the app loader (TacticalAIAssets) primes
    /// from Resources - whoever runs first wins deterministically.</summary>
    public static class ImperialDecks
    {
        static Dictionary<string, object> _docCache;
        static ImperialClassDeckTable _table;

        public static void PrimeDoc(Dictionary<string, object> doc)
        {
            _docCache = doc;
            _table = null;
        }

        public static Dictionary<string, object> DocOrNull() { return _docCache; }

        public static ImperialClassDeckTable Load()
        {
            if (_table == null) _table = ImperialClassDeckTable.FromDoc(_docCache);
            return _table;
        }

        // ------------------------------------------------------- family tokens
        // Canonical unit-family vocabulary shared by deck texts and villain
        // names. Word-boundary, plural-tolerant, uppercase scan (the printed
        // card texts use SMALL CAPS for unit families, e.g. "TROOPERS").
        static readonly Dictionary<string, Regex> FamilyPatterns =
            new Dictionary<string, Regex>(StringComparer.Ordinal)
            {
                // plural rule is (?:ES|S)? - a bare 'S?' after HERO would mean
                // "HEROES? = HERO+ES?" and MISS plain "HERO" (caught by the
                // M12b authoring twin before canon was pinned)
                { "TROOPER",       new Regex(@"\bTROOPER(?:ES|S)?\b") },
                { "REBEL",         new Regex(@"\bREBEL(?:ES|S)?\b") },
                { "VEHICLE",       new Regex(@"\bVEHICLE(?:ES|S)?\b") },
                { "DROID",         new Regex(@"\bDROID(?:ES|S)?\b") },
                { "HEAVY WEAPON",  new Regex(@"\bHEAVY\s+WEAPON(?:ES|S)?\b") },
                { "MINE",          new Regex(@"\bMINE(?:ES|S)?\b") },
                { "HERO",          new Regex(@"\bHERO(?:ES|S)?\b") },
                { "OFFICER",       new Regex(@"\bOFFICER(?:ES|S)?\b") },
                { "GUARDIAN",      new Regex(@"\bGUARDIAN(?:ES|S)?\b") },
                { "COMMANDER",     new Regex(@"\bCOMMANDER(?:ES|S)?\b") },
                { "AGENDA",        new Regex(@"\bAGENDA(?:ES|S)?\b") },
                { "MISSION",       new Regex(@"\bMISSION(?:ES|S)?\b") },
            };

        /// <summary>The UNIT families the synergy term sums over (HERO/REBEL/
        /// AGENDA/MISSION have their own terms; the vocabulary is shared).</summary>
        public static readonly string[] UnitFamilies =
        {
            "TROOPER", "VEHICLE", "DROID", "HEAVY WEAPON", "MINE",
            "OFFICER", "GUARDIAN", "COMMANDER",
        };

        /// <summary>family -> mention count (zero-count families omitted).</summary>
        public static Dictionary<string, int> FamilyCounts(string text)
        {
            var outp = new Dictionary<string, int>(StringComparer.Ordinal);
            AddFamilyCounts(outp, text, add: false);
            return outp;
        }

        /// <summary>Add one text's mentions into an accumulator (sum mode).</summary>
        public static void AddFamilyCounts(Dictionary<string, int> into, string text)
        {
            AddFamilyCounts(into, text, add: true);
        }

        static void AddFamilyCounts(Dictionary<string, int> into, string text, bool add)
        {
            if (string.IsNullOrEmpty(text)) return;
            var up = text.ToUpperInvariant();
            foreach (var kv in FamilyPatterns)
            {
                int n = kv.Value.Matches(up).Count;
                if (n == 0) continue;
                if (add)
                {
                    int old;
                    into.TryGetValue(kv.Key, out old);
                    into[kv.Key] = old + n;
                }
                else into[kv.Key] = n;
            }
        }

        /// <summary>Deck-level family profile: sum over abilities text +
        /// restriction + italic keyword (card NAMES are excluded - the deck
        /// banner is not a card rule).</summary>
        public static Dictionary<string, int> DeckFamilyCounts(ImperialClassDeck deck)
        {
            var acc = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var c in deck.Cards)
            {
                foreach (var t in c.AbilityTexts) AddFamilyCounts(acc, t);
                AddFamilyCounts(acc, c.Restriction);
                AddFamilyCounts(acc, c.Keyword);
            }
            return acc;
        }

        /// <summary>GroupTraits enum name -> family (the app's OWN trait
        /// table, used by the Unity adapter to enrich villain families).
        /// Traits with no family meaning return null.</summary>
        public static string FamilyFromTrait(string traitName)
        {
            switch (traitName)
            {
                case "Trooper": return "TROOPER";
                case "HeavyWeapon": return "HEAVY WEAPON";
                case "Droid": return "DROID";
                case "Vehicle": return "VEHICLE";
                case "Guardian": return "GUARDIAN";
                case "Leader": return "COMMANDER";
                default: return null;
            }
        }

        // ---------------------------------------------------------------- seed
        // FNV-1a 64 with the bridge's exact constants (TacticalAIBridge.
        // SessionSeed): one hash function, one guard, campaign-wide.
        public static ulong Fnv1a64(string s)
        {
            ulong h = 1469598103934665603UL;
            if (s != null)
                foreach (var c in s) { h ^= c; h *= 1099511628211UL; }
            return h;
        }

        /// <summary>Campaign-wide deck seed: derived from the campaign GUID
        /// ONLY (mission-independent, so the choice cannot drift between
        /// missions). Same 0-guard as the bridge session seed.</summary>
        public static ulong DeckSeedFor(string campaignGuid)
        {
            var h = Fnv1a64((campaignGuid ?? "") + "|imperial-deck");
            return h == 0 ? 0xA5A5A5A5A5A5A5A5UL : h;
        }

        public static string SeedHex(ulong seed)
        {
            return seed.ToString("x16");
        }

        public static ulong ParseSeedHex(string hex)
        {
            return Convert.ToUInt64(hex, 16);
        }

        // ------------------------------------------------------------- scoring
        static long Cap(long v, long cap) { return v < 0 ? 0 : (v > cap ? cap : v); }

        public static long SynergyScore(Dictionary<string, int> deckFamilies,
                                        ImperialCampaignProfile profile)
        {
            long s = 0;
            int famCount(string f)
            {
                int n;
                return deckFamilies.TryGetValue(f, out n) ? n : 0;
            }
            foreach (var f in UnitFamilies)
                s += 3 * Cap(profile.VillainFamilyCount(f), 4) * Cap(famCount(f), 4);
            s += 2 * Cap(profile.HeroCount, 4) * Cap(famCount("HERO"), 4);
            s += 1 * Cap(profile.HeroCount, 4) * Cap(famCount("REBEL"), 4);
            s += 2 * Cap(profile.AgendaMissionCount, 4) * Cap(famCount("AGENDA"), 4);
            s += 1 * Cap(profile.MissionCount, 8) * Cap(famCount("MISSION"), 4);
            return s;
        }

        /// <summary>Bounded tie jitter 0..3, re-derivable from the seed.</summary>
        public static long NoiseFor(ulong seed, string deckId)
        {
            var rng = new DeterministicRandom(seed ^ Fnv1a64("noise|" + deckId));
            return rng.RandBelow(4);
        }

        /// <summary>The deterministic choice: full ranked score tuple over
        /// every deck; rank = total desc, then deck id ascending (ordinal).
        /// Requires a primed table; returns null when none is loaded.</summary>
        public static ImperialDeckChoice Choose(ImperialCampaignProfile profile, ulong seed)
        {
            var table = Load();
            return Choose(table, profile, seed);
        }

        public static ImperialDeckChoice Choose(ImperialClassDeckTable table,
                                                ImperialCampaignProfile profile, ulong seed)
        {
            if (table == null || table.Decks.Count == 0) return null;
            profile = profile ?? new ImperialCampaignProfile();

            // Collection gate (Noldorion PR #194 review 2.11): a deck whose
            // box the player never activated is not an offer - "Hutt
            // Mercenaries" with Jabba's Realm switched off was picked from
            // content the collection says the table does not have. Owned
            // list present -> only owned boxes compete; unknown (null) ->
            // ungated (harness); nothing owned matches -> Core-box decks
            // (the shipped fallback, always at least the three Core decks).
            IEnumerable<ImperialClassDeck> offered = table.Decks;
            if (profile.OwnedExpansions != null)
            {
                var owned = table.Decks
                    .Where(d => d.Expansion == "Core"
                                || profile.OwnedExpansions.Contains(d.Expansion))
                    .ToList();
                offered = owned.Count > 0 ? owned
                    : table.Decks.Where(d => d.Expansion == "Core").ToList();
            }

            var rows = new List<ImperialDeckScoreRow>();
            foreach (var deck in offered)
            {
                var syn = SynergyScore(DeckFamilyCounts(deck), profile);
                var noise = NoiseFor(seed, deck.Id);
                rows.Add(new ImperialDeckScoreRow
                {
                    Deck = deck.Id,
                    Synergy = syn,
                    Noise = noise,
                    Total = syn + noise,
                });
            }
            rows = rows.OrderByDescending(r => r.Total)
                       .ThenBy(r => r.Deck, StringComparer.Ordinal)
                       .ToList();

            var choice = new ImperialDeckChoice
            {
                SeedHex = SeedHex(seed),
                DeckId = rows[0].Deck,
                Scores = rows,
            };
            var top = rows[0];
            var deckRow = table.Find(top.Deck);
            var reason = (deckRow != null ? deckRow.Name : top.Deck)
                + ": synergy=" + top.Synergy + " noise=" + top.Noise
                + " total=" + top.Total + " (seed " + choice.SeedHex + ")";
            choice.Reason = reason;
            return choice;
        }
    }
}
