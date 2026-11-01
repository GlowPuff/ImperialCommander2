// M3.1 HeroThreatProfile mirror: the SAME checks the console harness runs
// in SectionsC (hero_profiles.json / hero_w.json sections + the profile
// cases of risk.json) against the Python golden vectors. Honest mirrors —
// no new expectations; the console harness (36,630 checks, ALL PASS) stays
// the proof TODAY until a Unity test framework exists (see TacaiFixtures).
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TacticalAI.Core;

namespace TacticalAI.Tests
{
    [Explicit("Requires Test Framework")]
    [TestFixture]
    public class HeroProfileTests
    {
        static Dictionary<string, object> AssetDoc()
        {
            var p = AssetPath();
            return Json.AsDict(Json.Parse(File.ReadAllText(p)));
        }

        // The app's OWN asset under Languages/En (same bytes as the sim's
        // curated sim/data/hero_profiles.json — the harness pins the sha).
        static string AssetPath()
        {
            var env = Environment.GetEnvironmentVariable("TACAI_LANGUAGES_DIR");
            if (!string.IsNullOrEmpty(env))
                return Path.Combine(env, "En", "tactical_hero_profiles.json");
            var d = new DirectoryInfo(TacaiFixtures.Dir);
            for (int i = 0; i < 12 && d != null; i++, d = d.Parent)
            {
                foreach (var rel in new[]
                {
                    Path.Combine("ImperialCommander2", "Assets", "Resources", "Languages"),
                    Path.Combine("Assets", "Resources", "Languages"),
                })
                {
                    var cand = Path.Combine(d.FullName, rel, "En",
                                            "tactical_hero_profiles.json");
                    if (File.Exists(cand)) return cand;
                }
            }
            throw new FileNotFoundException("tactical_hero_profiles.json not found");
        }

        static object LoadLangJson(string lang, string rel)
        {
            var p = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(AssetPath())),
                                 lang, rel);
            var txt = System.Text.RegularExpressions.Regex.Replace(
                File.ReadAllText(p), @",\s*([}\]])", "$1");
            return Json.Parse(txt);
        }

        static World ProbeWorld(Dictionary<string, HeroThreatProfile> store)
            => new World(null, new List<Figure>()) { HeroProfiles = store };

        static List<HeroThreatProfile> SyntheticProfiles(Dictionary<string, object> v)
        {
            var outp = new List<HeroThreatProfile>();
            foreach (var pO in Json.AsList(v["synthetic_profiles"]))
            {
                var p = Json.AsDict(pO);
                outp.Add(new HeroThreatProfile
                {
                    Name = Json.AsStr(p["name"]),
                    Role = Json.AsStr(p["role"]),
                    EvDamagePerRound = HeroProfiles.ParseFrac(p["evDamagePerRound"]),
                    RangeMax = Json.AsInt(p["rangeMax"]),
                    IgnoreLos = Json.AsBool(p["ignoreLos"]),
                    SelfHealPossible = Json.AsBool(p["selfHealPossible"]),
                    Mobility = Json.AsInt(p["mobility"]),
                    Flags = Json.AsList(p["flags"]).ConvertAll(o => Json.AsStr(o)),
                    Knowledge = Json.AsStr(p["knowledge"]),
                });
            }
            return outp;
        }

        [Test]
        public void Asset_Loader_Rows_ByteExact()
        {
            HeroProfiles.PrimeDoc(AssetDoc());
            var v = Json.AsDict(TacaiFixtures.Load("hero_profiles.json"));
            var profiles = HeroProfiles.ProfilesFromDoc(AssetDoc());
            var expect = Json.AsDict(v["profiles"]);
            var probe = ProbeWorld(SyntheticProfiles(v).ToDictionary(p => p.Name, p => p));
            Assert.AreEqual(expect.Count, profiles.Count);
            foreach (var kv in TacaiFixtures.OrdKeys(expect))
            {
                Assert.IsTrue(profiles.TryGetValue(kv.Key, out var p));
                TacaiFixtures.Check("hero.profiles." + kv.Key, kv.Value, p.VectorRow(probe));
            }
        }

        [Test]
        public void Disengage_Flag_Rule_Grid()
        {
            HeroProfiles.PrimeDoc(AssetDoc());
            var v = Json.AsDict(TacaiFixtures.Load("hero_profiles.json"));
            TacaiFixtures.Check("hero.cover_range", v["cover_shelter_max_range"],
                                HeroProfiles.CoverShelterMaxRange());
            int i = 0;
            foreach (var rO in Json.AsList(v["cover_worthless"]))
            {
                var r = Json.AsDict(rO);
                TacaiFixtures.Check("hero.cover_worthless[" + i + "]", r["worthless"],
                      HeroProfiles.CoverIsWorthless(Json.AsBool(r["ignore_los"]),
                                                    Json.AsInt(r["range_max"])));
                i++;
            }
            // Pflichttests 3/4/5 as human-readable anchors:
            Assert.IsTrue(HeroProfiles.CoverIsWorthless(true, 3),
                          "ignoreLos hero makes cover worthless");
            Assert.IsTrue(HeroProfiles.CoverIsWorthless(false, 5),
                          "over-shelter range makes cover worthless");
            Assert.IsFalse(HeroProfiles.CoverIsWorthless(false, 1),
                           "pure melee keeps disengage at full value");
        }

        [Test]
        public void Profile_Cases_Disengage_Grid()
        {
            // risk.json profile_cases: the SAME grid the harness replays —
            // rho INNERMOST; asset doc primed for cover_shelter_max_range.
            // The vector is huge (~240k generated lines) and not shipped on
            // every branch: without it this test skips, it must not fail.
            if (!System.IO.File.Exists(TacaiFixtures.Path2("risk.json")))
                Assert.Ignore("risk.json golden vector not shipped — regenerate " +
                              "via tacai_build tools/export_csharp_vectors.py");
            HeroProfiles.PrimeDoc(AssetDoc());
            var v = Json.AsDict(TacaiFixtures.Load("risk.json"));
            var cases = Json.AsList(v["profile_cases"]);
            Assert.Greater(cases.Count, 0);
            foreach (var caseO in cases)
            {
                var cse = Json.AsDict(caseO);
                var fct = Json.AsDict(cse["facts"]);
                var f = new RiskFacts
                {
                    GroupId = "G1",
                    Pattern = Json.AsStr(fct["pattern"]),
                    Posture = Json.AsStr(fct["posture"]),
                    Counter = fct["counter"] == null ? null : Json.AsStr(fct["counter"]),
                    Vitality = Rational.Parse(Json.AsStr(fct["vitality"])),
                    RingNear = Json.AsBool(fct["ring"]),
                    OnAnchor = true,
                    ReplacesAt = 4,
                    LedgerBalance = 6,
                    VictoryBlocker = Json.AsBool(fct["vb"]),
                    ThreatIgnoreLos = Json.AsBool(fct["ignore_los"]),
                    ThreatRangeMax = Json.AsInt(fct["range_max"]),
                };
                var got = new List<object>();
                foreach (var rho in new[] { 0, 1, 2, 3 })
                    got.Add(RiskPosture.RiskPosition(f, rho).AsDict());
                TacaiFixtures.Check("risk.profile_cases", cse["rho_rows"], got);
            }
        }

        [Test]
        public void HeroW_DefaultOff_StaysByteIdentical()
        {
            // variant "none" rows must equal the plain PostureValue rows —
            // the store-empty regression pin (T1/T2 Hash-Pins parity).
            HeroProfiles.PrimeDoc(AssetDoc());
            foreach (var eO in TacaiFixtures.LoadList("hero_w.json"))
            {
                var e = Json.AsDict(eO);
                var scenario = Json.AsDict(TacaiFixtures.Scenarios()[Json.AsStr(e["scenario"])]);
                var (board, world) = TacaiFixtures.BuildScenarioWorld(scenario);
                var vm = MissionVictoryModel.FromBoard(board);
                if (scenario.ContainsKey("victoryCounter"))
                    vm.Counter = new VictoryCounter(Json.AsStr(scenario["victoryCounter"]));
                var none = Json.AsList(e["variants"]).Select(Json.AsDict)
                    .First(v => Json.AsStr(v["variant"]) == "none");
                var rows = new List<object>();
                foreach (var f in world.Figures.Where(f => f.Team == "imperial"))
                    foreach (var r in world.Figures.Where(r => r.Team == "rebel"))
                        rows.Add(new Dictionary<string, object>
                        {
                            { "att", f.Id }, { "tgt", r.Id },
                            { "w", vm.PostureValue(world, f, r)
                                    .Select(x => TacaiFixtures.Norm(x)).ToList() },
                        });
                TacaiFixtures.Check("hero_w." + Json.AsStr(e["scenario"]) + ".none.w",
                                    none["w"], rows);
                world.HeroProfiles = null;
            }
        }

        [Test]
        public void Campaign_Loadouts_Against_AppCampaignData()
        {
            // v3.11: loadout scoring resolves against the app's OWN
            // CampaignData tables (only ids count — language swappable).
            HeroProfiles.PrimeDoc(AssetDoc());
            var v = Json.AsDict(TacaiFixtures.Load("hero_profiles.json"));
            var refs = new Dictionary<string, object>
            {
                { "items", LoadLangJson("En", "CampaignData/items.json") },
                { "skills", LoadLangJson("En", "CampaignData/skills.json") },
            };
            var baseProfiles = HeroProfiles.ProfilesFromDoc(AssetDoc());
            var loFixture = Json.AsDict(TacaiFixtures.Load("campaign_loadouts.json"));
            var loaded = HeroProfiles.ApplyCampaignLoadouts(
                HeroProfiles.ProfilesFromDoc(AssetDoc()),
                HeroProfiles.LoadoutRows(loFixture), refs);
            var byId = HeroProfiles.ProfilesByAppId(baseProfiles);
            var probe = ProbeWorld(SyntheticProfiles(v).ToDictionary(p => p.Name, p => p));
            foreach (var rO in Json.AsList(v["loadouts"]))
            {
                var r = Json.AsDict(rO);
                string who = Json.AsStr(r["hero"]);
                if (Json.AsBool(r["skipped"]))
                {
                    TacaiFixtures.Check("hero.loadouts[" + who + "].skipped", true,
                                        !baseProfiles.ContainsKey(who));
                    continue;
                }
                var target = baseProfiles.ContainsKey(who) ? baseProfiles[who]
                    : (byId.ContainsKey(who) ? byId[who] : null);
                Assert.IsNotNull(target);
                TacaiFixtures.Check("hero.loadouts[" + who + "].loaded", r["loaded"],
                                    loaded[target.Name].VectorRow(probe));
            }
        }

        // dev 30 playtest regression guard (Thomas, EMPIRE6 crash): the Unity
        // player has NO other loader for the Core doc cache - if the app-side
        // asset loader stops priming, EVERY campaign activation throws
        // 'HeroProfiles.ApplyCampaignLoadouts: no doc loaded' in the live game
        // while harness and gate keep passing (they prime from file).
        [Test]
        public void AppAssetLoaderPrimesCoreDocCache()
        {
            var root = TacticalAI.TacticalAIAssets.HeroProfilesRoot;
            if (root == null) { Assert.Ignore("hero_profiles asset unavailable"); return; }
            Assert.IsNotNull(HeroProfiles.DocOrNull(),
                "TacticalAIAssets.HeroProfilesRoot must prime HeroProfiles doc cache");
        }
    }
}
