// Golden-vector verification harness for the TacticalAI C# core.
//
//   dotnet run --project tools/TacticalAiHarness -- <fixtures-dir>
//
// Every Assets/Tests/Fixtures/tacai/*.json vector file exported 1:1 from the
// Python sim at tacai_build/sim is replayed here against the ported core.
// One PASS/FAIL line per vector file + summary. The Python sim defines
// semantics: on a mismatch, first reproduce in Python, then decide whether
// the CORE diverges (fix core) or the harness call is wrong (fix harness).
//
// Tolerant deep comparison rules (documented per task):
//   * Rational (port of Fraction) compares against BOTH "p/q" fq strings and
//     numbers; string forms are exact (str(Fraction) mirror), numbers use a
//     1e-9 absolute tolerance (covers Python round(x, 6) prints).
//   * strings compare byte-exact (why/reason payloads are fairness-visible).
//   * dict: identical key sets, recursive; list: same order, recursive.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using TacticalAI.Core;

namespace TacticalAiHarness
{
    public static partial class Program
    {
        static string FIX;
        static long TotalChecks, TotalFails;
        static long _secChecks, _secFails;
        static readonly List<string> _diffs = new List<string>();
        static string _sec;
        static int _secFailCount;

        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("usage: TacticalAiHarness <fixtures-dir>");
                return 2;
            }
            FIX = Path.GetFullPath(args[0]);
            // M5: `--record` regenerates the world_feed expect blocks from
            // the Core projection (golden-vector record step; the pinned file
            // then governs harness AND NUnit - vectors stay canonical).
            foreach (var a in args) if (a == "--record") Record = true;
            if (!Directory.Exists(FIX))
            {
                Console.Error.WriteLine("fixtures dir not found: " + FIX);
                return 2;
            }
            // M3.1: prime the curated HeroThreatProfile asset BEFORE any
            // section runs — the sim's cover_shelter_max_range (4) differs
            // from the code default (5), and risk.json profile_cases were
            // exported against the asset (parity with Python _doc()).
            try
            {
                var langDir = LanguagesDir();
                var assetPath = Path.Combine(langDir, "En", "tactical_hero_profiles.json");
                if (File.Exists(assetPath))
                    HeroProfiles.PrimeDoc(Json.AsDict(Json.Parse(File.ReadAllText(assetPath))));
            }
            catch { /* sections that need the doc fail loudly on their own */ }
            // M11b: the printed hero-card asset (threat v2 basis) — same
            // seam: harness pins against the SHIPPED asset, never a fixture copy.
            try
            {
                var langDir = LanguagesDir();
                var cardsPath = Path.Combine(langDir, "En", "tactical_hero_cards.json");
                if (File.Exists(cardsPath))
                    HeroCards.PrimeDoc(Json.AsDict(Json.Parse(File.ReadAllText(cardsPath))));
            }
            catch { /* sections that need the doc fail loudly on their own */ }

            // M12b: the Imperial class-deck asset (deck choice basis) — same
            // seam: harness pins against the SHIPPED asset, never a fixture copy.
            try
            {
                var langDir = LanguagesDir();
                var decksPath = Path.Combine(langDir, "En", "tactical_imperial_decks.json");
                if (File.Exists(decksPath))
                    ImperialDecks.PrimeDoc(Json.AsDict(Json.Parse(File.ReadAllText(decksPath))));
            }
            catch { /* sections that need the doc fail loudly on their own */ }

            // M12d: the classification-table asset (card hooks basis) — same
            // seam: harness pins against the SHIPPED asset, never a fixture copy.
            try
            {
                var langDir = LanguagesDir();
                var effectsPath = Path.Combine(langDir, "En", "tactical_imperial_card_effects.json");
                if (File.Exists(effectsPath))
                    ImperialCardEffects.PrimeDoc(Json.AsDict(Json.Parse(File.ReadAllText(effectsPath))));
            }
            catch { /* sections that need the doc fail loudly on their own */ }

            // M12d2: the agenda-card asset (setup + market basis) — same
            // seam: harness pins against the SHIPPED asset, never a copy.
            try
            {
                var langDir = LanguagesDir();
                var agendaPath = Path.Combine(langDir, "En", "tactical_agenda_cards.json");
                if (File.Exists(agendaPath))
                    AgendaCards.PrimeDoc(Json.AsDict(Json.Parse(File.ReadAllText(agendaPath))));
            }
            catch { /* the agenda section fails loudly on its own */ }

            Run("geometry.json", SecGeometry);
            Run("board.json", SecBoard);
            Run("path.json", SecPath, optional: true);
            Run("legality.json", SecLegality);
            Run("los.json", SecLos);
            Run("dice.json", SecDice);
            Run("threat.json", SecThreat);
            Run("skill.json", SecSkill);
            Run("risk.json", SecRisk, optional: true);
            Run("focus.json", SecFocus);
            Run("victory.json", SecVictory);
            Run("belief.json", SecBelief);
            Run("rings.json", SecRings);
            Run("doctrine.json", SecDoctrine);
            Run("order_plan.json", SecOrderPlan);
            Run("action_plan.json", SecActionPlan);
            Run("hero_profiles.json", SecHeroProfiles);
            Run("hero_w.json", SecHeroW);
            Run("world_feed.json", SecWorldFeed);
            Run("overlord_focus.json", SecOverlordFocus);
            Run("strain_budget.json", SecStrainBudget);
            Run("keyword_rules.json", SecKeywordRules);
            Run("villain_options.json", SecVillainOptions);
            Run("logger_extract.json", SecLoggerExtract);
            Run("imperial_deck.json", SecImperialDeck);
            Run("imperial_purchases.json", SecImperialPurchases);
            Run("agenda.json", SecAgenda);
            Run("imperial_card_effects.json", SecImperialCardEffects);
            Run("optional_deploy.json", SecOptionalDeploy);
            Run("settings_load.json", SecSettingsLoad);
            Run("personas.json", SecPersonas);
            Run("pattern_weights.json", SecPatternWeights);

            Console.WriteLine("== summary: " + (TotalFails == 0 ? "ALL PASS" : "FAILURES") +
                              " | checks=" + TotalChecks + " fails=" + TotalFails);
            return TotalFails == 0 ? 0 : 1;
        }

        static void Run(string file, Action sec, bool optional = false)
        {
            // A golden vector can be unshipped (too large for a public PR —
            // e.g. path.json/risk.json, ~490k generated lines). SKIP is not a
            // FAIL: the vectors regenerate bit-exact from the reference model
            // (tacai_build tools/export_csharp_vectors.py).
            if (optional && !File.Exists(Path.Combine(FIX, file)))
            {
                Console.WriteLine("SKIP " + file + " (golden vector not shipped; " +
                                  "regenerate via export_csharp_vectors.py)");
                return;
            }
            _sec = file; _secChecks = 0; _secFails = 0; _diffs.Clear(); _secFailCount = 0;
            try { sec(); }
            catch (Exception ex)
            {
                _secFails++;
                _diffs.Add("(exception) " + ex.GetType().Name + ": " + ex.Message);
                var st = (ex.StackTrace ?? "").Split('\n').Take(6);
                foreach (var line in st) _diffs.Add("      " + line.Trim());
            }
            TotalChecks += _secChecks; TotalFails += _secFails;
            string status = _secFails == 0 ? "PASS" : "FAIL";
            Console.WriteLine(status + " " + _sec + " (" + _secChecks + " checks" +
                              (_secFails == 0 ? "" : ", " + _secFails + " failed") + ")");
            foreach (var d in _diffs) Console.WriteLine("   - " + d);
        }

        static void Check(string path, object expected, object actual)
        {
            _secChecks++;
            object a = Norm(actual);
            if (!Eq(expected, a))
            {
                _secFails++;
                if (_diffs.Count < 6)
                    _diffs.Add(path + ": expected " + Repr(expected) + ", got " + Repr(a));
            }
        }

        static void Fail(string path, string msg)
        {
            _secChecks++; _secFails++;
            if (_diffs.Count < 6) _diffs.Add(path + ": " + msg);
        }

        // ------------------------------------------------------------- normalize

        static object Norm(object v)
        {
            if (v == null) return null;
            if (v is string) return v;
            if (v is bool) return v;
            if (v is Rational r) return r.ToString();
            if (v is Cell c) return new List<object> { (long)c.X, (long)c.Y };
            if (v is int i0) return (long)i0;
            if (v is long) return v;
            if (v is double) return v;
            if (v is int[] ia) return ia.Select(x => (object)(long)x).ToList();
            if (v is IDictionary<string, object> d)
            {
                var o = new Dictionary<string, object>();
                foreach (var kv in d) o[kv.Key] = Norm(kv.Value);
                return o;
            }
            if (v is IDictionary dd)   // any other dict: stringify keys
            {
                var o = new Dictionary<string, object>();
                foreach (DictionaryEntry e in dd) o[Convert.ToString(e.Key, CultureInfo.InvariantCulture)] = Norm(e.Value);
                return o;
            }
            var t = v.GetType();
            if (t.IsGenericType && t.Name.StartsWith("ValueTuple"))
            {
                var tup = (ITuple)v;
                var o = new List<object>();
                for (int k = 0; k < tup.Length; k++) o.Add(Norm(tup[k]));
                return o;
            }
            if (v is IList list)
            {
                var o = new List<object>();
                foreach (var e in list) o.Add(Norm(e));
                return o;
            }
            if (v is IEnumerable en)
            {
                var o = new List<object>();
                foreach (var e in en) o.Add(Norm(e));
                return o;
            }
            return v;
        }

        static readonly System.Text.RegularExpressions.Regex IntRe =
            new System.Text.RegularExpressions.Regex(@"^-?\d+$");

        static bool Eq(object e, object a)
        {
            if (e == null) return a == null;
            if (e is Dictionary<string, object> ed)
            {
                if (!(a is Dictionary<string, object> ad)) return false;
                if (ed.Count != ad.Count) return false;
                foreach (var kv in ed)
                    if (!ad.ContainsKey(kv.Key) || !Eq(kv.Value, ad[kv.Key])) return false;
                return true;
            }
            if (e is List<object> el)
            {
                if (!(a is List<object> al)) return false;
                if (el.Count != al.Count) return false;
                for (int i = 0; i < el.Count; i++) if (!Eq(el[i], al[i])) return false;
                return true;
            }
            if (e is string es)
            {
                if (a is string as2) return es == as2;
                // "5" expected but actual number 5 (int-valued fq strings)
                if ((a is long || a is double) && IntRe.IsMatch(es))
                    return Convert.ToDouble(a) == double.Parse(es, CultureInfo.InvariantCulture);
                return false;
            }
            if (e is bool eb) return a is bool ab && ab == eb;
            if (e is long || e is double)
            {
                if (a is long || a is double)
                {
                    double x = Convert.ToDouble(e, CultureInfo.InvariantCulture);
                    double y = Convert.ToDouble(a, CultureInfo.InvariantCulture);
                    return Math.Abs(x - y) <= 1e-9;
                }
                if (a is string astr && IntRe.IsMatch(astr) && e is long)
                    return long.Parse(astr, CultureInfo.InvariantCulture) == (long)e;
                return false;
            }
            return Equals(e, a);
        }

        static string Repr(object v)
        {
            string s;
            try { s = Canonical(v); }
            catch { s = Convert.ToString(v, CultureInfo.InvariantCulture); }
            return s != null && s.Length > 160 ? s.Substring(0, 160) + "..." : s;
        }

        // ------------------------------------------------- canonical json (sha)

        // Reproduces json.dumps(obj, sort_keys=True, separators=(",",":"),
        // ensure_ascii=True) byte-for-byte over the value trees AsDict emits.
        public static string Canonical(object v)
        {
            var sb = new StringBuilder();
            W(sb, v);
            return sb.ToString();
        }

        static void W(StringBuilder sb, object v)
        {
            if (v == null) { sb.Append("null"); return; }
            if (v is bool b) { sb.Append(b ? "true" : "false"); return; }
            if (v is string s) { WStr(sb, s); return; }
            if (v is long l) { sb.Append(l.ToString(CultureInfo.InvariantCulture)); return; }
            if (v is int i) { sb.Append(i.ToString(CultureInfo.InvariantCulture)); return; }
            if (v is double d) { sb.Append(PyDouble(d)); return; }
            if (v is Rational r)
            {   // should not appear in plan dicts (Focus AsDict normalizes) — mirror str()
                WStr(sb, r.ToString()); return;
            }
            if (v is Dictionary<string, object> dict)
            {
                sb.Append('{');
                bool first = true;
                foreach (var k in dict.Keys.OrderBy(x => x, StringComparer.Ordinal))
                {
                    if (!first) sb.Append(',');
                    first = false;
                    WStr(sb, k); sb.Append(':'); W(sb, dict[k]);
                }
                sb.Append('}');
                return;
            }
            if (v is IDictionary idict)
            {
                var o = new Dictionary<string, object>();
                foreach (DictionaryEntry e in idict) o[Convert.ToString(e.Key, CultureInfo.InvariantCulture)] = e.Value;
                W(sb, o); return;
            }
            if (v is IList list)
            {
                sb.Append('[');
                for (int k = 0; k < list.Count; k++) { if (k > 0) sb.Append(','); W(sb, list[k]); }
                sb.Append(']');
                return;
            }
            if (v is ITuple tup)
            {
                sb.Append('[');
                for (int k = 0; k < tup.Length; k++)
                {
                    if (k > 0) sb.Append(',');
                    W(sb, Norm(tup[k]));
                }
                sb.Append(']');
                return;
            }
            if (v is IEnumerable en)
            {
                var o = new List<object>();
                foreach (var e in en) o.Add(e);
                W(sb, o); return;
            }
            WStr(sb, Convert.ToString(v, CultureInfo.InvariantCulture));
        }

        static void WStr(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (var ch in s)
            {
                switch (ch)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    default:
                        if (ch < 0x20) sb.Append("\\u").Append(((int)ch).ToString("x4"));
                        else if (ch < 0x7f) sb.Append(ch);
                        else sb.Append("\\u").Append(((int)ch).ToString("x4"));
                        break;
                }
            }
            sb.Append('"');
        }

        static string PyDouble(double d)
        {
            // Python repr(float): shortest round-trip; integral floats keep ".0";
            // exponents use lowercase e with sign and >=2 digits.
            string s = d.ToString("R", CultureInfo.InvariantCulture);
            if (s.Contains("E") || s.Contains("e"))
            {
                var parts = s.Split('E', 'e');
                var mant = parts[0].TrimEnd('0').TrimEnd('.');
                if (mant.Contains(".") == false) mant += ".0";
                int exp = int.Parse(parts[1], CultureInfo.InvariantCulture);
                return mant + "e" + (exp >= 0 ? "+" : "-") + Math.Abs(exp).ToString("D2", CultureInfo.InvariantCulture);
            }
            if (!s.Contains(".") && !s.Contains("n") && !s.Contains("y")) s += ".0";
            return s;
        }

        public static string Sha256Hex(string s)
        {
            using (var sha = SHA256.Create())
            {
                var h = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
                var sb = new StringBuilder();
                foreach (var b in h) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        // ------------------------------------------------------------ fixtures

        static Dictionary<string, object> LoadJson(string file) =>
            Json.AsDict(Json.Parse(File.ReadAllText(Path.Combine(FIX, file))));

        static List<object> LoadJsonList(string file) =>
            Json.AsList(Json.Parse(File.ReadAllText(Path.Combine(FIX, file))));

        static Dictionary<string, object> _scenarios;
        static Dictionary<string, object> Scenarios()
        {
            if (_scenarios == null)
            {
                _scenarios = new Dictionary<string, object>();
                foreach (var f in new[] { "scenarios_m2a1.json", "scenarios_m2a2.json" })
                    foreach (var kv in LoadJson(f))
                        if (kv.Key != "_comment") _scenarios[kv.Key] = kv.Value;
                _scenarios = _scenarios.OrderBy(kv => kv.Key, StringComparer.Ordinal)
                                       .ToDictionary(kv => kv.Key, kv => kv.Value);
            }
            return _scenarios;
        }

        static Dictionary<string, DeploymentCard> _catalog;
        static Dictionary<string, DeploymentCard> Catalog() =>
            _catalog ?? (_catalog = DeploymentCard.LoadCatalog(Json.Parse(
                File.ReadAllText(Path.Combine(FIX, "units_core.json")))));

        static readonly string[] BaseBoards =
            { "open_map_two_entrances", "rancor_corridor", "siege_courtyard", "terminal_two_room" };

        static Dictionary<string, Board> _boards;
        static Dictionary<string, Board> Boards()
        {
            if (_boards == null)
            {
                _boards = new Dictionary<string, Board>();
                foreach (var f in Directory.GetFiles(Path.Combine(FIX, "boards"), "*.json"))
                {
                    var b = Board.FromJson(Json.AsDict(Json.Parse(File.ReadAllText(f))));
                    _boards[b.Name] = b;
                }
            }
            return _boards;
        }

        static Board BoardOf(string name)
        {
            if (Boards().TryGetValue(name, out var b)) return b;
            // scenario boards live in the same dir; parse on demand
            var path = Path.Combine(FIX, "boards", name + ".json");
            var nb = Board.FromJson(Json.AsDict(Json.Parse(File.ReadAllText(path))));
            Boards()[name] = nb;
            return nb;
        }

        /// Inline twin of sim/victory._build_scenario_world + runner.build_world.
        static (Board, World) BuildScenarioWorld(Dictionary<string, object> spec)
        {
            var board = BoardOf(Json.AsStr(spec["board"]));
            var figs = new List<Figure>();
            int idx = 0;
            foreach (var fsO in Json.AsList(spec["figures"]))
            {
                var fs = Json.AsDict(fsO);
                string team = Json.AsStr(fs["team"]);
                Cell at;
                if (team == "npc")
                {
                    var ent = board.ProtectedEntities
                        .First(e => Json.AsStr(e["id"]) == Json.AsStr(fs["id"]));
                    at = FirstEntityCell(ent);
                }
                else at = Json.AsCell(fs["at"]);
                var card = Catalog()[Json.AsStr(fs["card"])];
                var f = new Figure
                {
                    Id = Json.AsStr(fs["id"]),
                    GroupId = Json.AsStr(fs["id"]),
                    Card = card,
                    Team = team,
                    Anchor = at,
                    Orientation = Json.AsInt(Json.Get(fs, "orientation", (long)0)),
                    Index = idx,
                    Alive = true,
                    Pattern = Json.AsStr(Json.Get(fs, "pattern", "aggressive")),
                    Vitality = card.Health,
                };
                figs.Add(f);
                idx++;
            }
            // scenario vitality/wounded overrides (mirrors zip(figures, specs))
            var specFigs = Json.AsList(spec["figures"]);
            for (int i = 0; i < figs.Count && i < specFigs.Count; i++)
            {
                var fs = Json.AsDict(specFigs[i]);
                // M9-F6 mirror of sim _build_scenario_world: "wounded" is
                // the isWounded STATUS; "vitality" is the CURRENT bar
                // (second bar after a wound reset). Partial bar damage
                // without the status is INVISIBLE damage - healthy.
                if (fs.ContainsKey("wounded"))
                    figs[i].Wounded = Json.AsBool(fs["wounded"]);
                if (fs.ContainsKey("vitality"))
                    figs[i].Vitality = Json.AsInt(fs["vitality"]);
            }
            var world = new World(board, figs) { Objectives = new ObjectiveRuntime(board) };
            return (board, world);
        }

        static Cell FirstEntityCell(Dictionary<string, object> ent)
        {
            if (ent["cells"] is List<Cell> lc) return lc[0];
            var cells = Json.AsList(ent["cells"]);
            var first = cells[0];
            if (first is Cell c) return c;
            return Json.AsCell(first);
        }

        static List<List<object>> Cells2L(IEnumerable<Cell> cs) =>
            cs.OrderBy(c => c.X).ThenBy(c => c.Y)
              .Select(c => new List<object> { (long)c.X, (long)c.Y }).ToList();

        static HashSet<Cell> CellSet(List<object> l) =>
            new HashSet<Cell>(Json.AsCells(l));

        /// python dict(sorted(...)) key order helper: ordinal string sort.
        static IEnumerable<KeyValuePair<string, object>> OrdKeys(Dictionary<string, object> d) =>
            d.OrderBy(kv => kv.Key, StringComparer.Ordinal);
    }

}
