// M12d (tactical AI): Imperial CLASS-CARD effects as hooks into the existing
// assessment pipeline. The classification table (tacai.imperial_card_effects/1,
// authored by wiki_data/tools/gen_imperial_card_effects.py from the shipped
// deck asset) maps EVERY printed card of the 12 class decks into exactly one
// bucket:
//
//   (a) PLANER-MODIFIER  - the printed effect lands on the strength-efficiency
//       stage of OrderPlanner.PlanRound or the light target-surcharge slot of
//       OverlordFocus.Rank (M11d/M11h law: MAX, never a sum).
//   (b) STAT-MODIFIER    - the printed effect lands on an EXISTING value seam:
//       the threat-model engagement (accuracy pips / Pierce of the hero
//       exchange, HeroThreatModelV2.Threat(engage)), the deployment economy
//       (ThreatLedger.EffectiveCost), or the store model (attachment
//       lifecycle in Saga.ImperialDeckState, effective Health in the
//       attrition vitality the caller feeds OrderPlanner).
//   (c) ENGINE-DETAIL    - real rules work beyond planner+values (dice
//       manipulation, token/condition operations, spatial legality). NOT
//       half-implemented: the table carries NO hooks for these cards, only a
//       one-line reason - the M12e UI shows them honestly as
//       "aktiv, aber ohne KI-Modell" (ActiveCardInfos projection below).
//
// DETERMINISM: hooks only modify the existing scoring pipeline. No new RNG
// pot, no wall-clock, no hidden information. Every hook VALUE is a printed
// fact or a documented conservative floor (the vocabulary table in the asset
// is the single source of the law; the same values are pinned by the
// imperial_card_effects.json canon):
//   * order_boost 5/4 = one extra action / activation cycle reliably printed
//     for the owning side; 1/8 = activation-timed movement points (mini
//     action economy). Applied as a multiplier on the efficiency stage,
//     MAX-once per group (never stacked sums), and announced in the public
//     why-text with the tag "card_effects".
//   * atk_ev_bonus  = added to the printed attack EV (same yardstick the ORD
//     decks calibrate with, Dice.PrintedEv). Surge prints price at the blue
//     surge floor 1/3 (P(blue shows >=1 surge) = 1/3 over the printed faces),
//     a "may reroll 1 die" print at 1/8, uncapped rerolls at 1/4, fixed
//     printed "+1 {H}/{C} to the attack results" at the exact pip 1.
//   * focus_surcharge +1 = the M10b objective-actor precedent ("die Bedrohung
//     erhoeht sich leicht"): printed target conditions make the conditioned
//     hero the cheaper finish; it flips ties and <1 gaps only.
//   * engage_acc_delta (signed pips) + engage_pierce - applied inside the
//     threat v2 damage race: the owning side's attacks against heroes cost
//     the hero more (+), the owning side's defensive prints make hero attacks
//     worse (-). Pip floor 1 per print row, never inflated.
//   * deploy_cost / free_group - printed threat-cost reductions and free
//     arrivals on the ledger seam; the ledger stays balanced because the
//     Empire really spends the reduced (0-) cost.
//   * attach_lifecycle - persistence lives in the STORE model (Saga.
//     ImperialDeckState), per the Rules Reference "Attachment" law: attached
//     at deployment onto the deploy card, acts on ALL figures of the group,
//     returns to the play area when the last figure of the group is defeated,
//     and can be attached AGAIN at future deployments (not consumed). Trait
//     restrictions are hard: a group without the trait cannot receive it.
//   * stat_health N - printed "+N Health" raises effective Health
//     (EffectiveVitality), the input the caller feeds the attrition weight.
//
// PLAYER SIDE: card effects belong to the side that OWNS the deck. The engine
// distinguishes FACTION only, never AI vs. human hand - the same hooks fire
// for the human Imperial player's cards against the AI heroes (the live
// bridge passes the campaign deck state's acquired cards either way). Where a
// card's player-side execution hangs deeper in the dice path than the model
// can see, the card is bucket (c), not half-implemented.
using System;
using System.Collections.Generic;
using System.Linq;

namespace TacticalAI.Core
{
    /// One classified card row of the effect table (bucket + hooks + reason).
    public sealed class CardEffectEntry
    {
        public string Id;
        public string Deck;
        public bool Basic;
        public int? Xp;
        public string Bucket;                 // "a" | "b" | "c"
        public readonly List<KeyValuePair<string, Rational>> Hooks =
            new List<KeyValuePair<string, Rational>>();
        public readonly List<string> Partial = new List<string>();
        public string Reason;

        public bool Modelled { get { return Bucket != "c"; } }

        public Dictionary<string, object> AsDict()
        {
            var hooks = new List<object>();
            foreach (var h in Hooks)
                hooks.Add(new Dictionary<string, object>
                {
                    { "kind", h.Key }, { "value", h.Value.ToString() },
                });
            return new Dictionary<string, object>
            {
                { "id", Id },
                { "deck", Deck },
                { "basic", Basic },
                { "xp", Xp.HasValue ? (object)(long)Xp.Value : null },
                { "bucket", Bucket },
                { "modelled", Modelled },
                { "hooks", hooks },
                { "partial", new List<object>(Partial) },
                { "reason", Reason },
            };
        }
    }

    /// The aggregated, owned-card-scoped effect state a pipeline call consumes.
    /// Built by ImperialCardEffects.Aggregate(table, ownedCardIds).
    public sealed class CardEffects
    {
        public Rational OrderBoost = Rational.Zero;     // efficiency multiplier
        public Rational AtkEvBonus = Rational.Zero;     // printed-strength add
        public int EngageAccDelta;                      // signed pips on the hero
        public int EngagePierce;                        // printed Pierce add
        public int DeployCostCut;                       // threat reduction
        public int FreeGroups;                          // cost-0 arrivals
        public int HealthBonus;                         // effective Health add
        public readonly List<string> CardIds = new List<string>();   // sorted ordinals

        /// NULL when nothing fires: the pipelines run bit-identical to their
        /// pre-M12d canon (the same "no unexplained diffs" discipline M11d
        /// pinned for the strain slot).
        public bool IsNeutral
        {
            get
            {
                return OrderBoost == Rational.Zero && AtkEvBonus == Rational.Zero
                    && EngageAccDelta == 0 && EngagePierce == 0 && DeployCostCut == 0
                    && FreeGroups == 0 && HealthBonus == 0;
            }
        }

        /// The OrderPlan strength/efficiency stage condition (cardsFire):
        /// owned hooks that actually MOVE the activation plan. The bridge's
        /// EXPLAIN announcement uses exactly this condition - never more,
        /// never less (announcing a card that changed no number is a lie).
        public bool Fires
        {
            get { return OrderBoost > Rational.Zero || AtkEvBonus > Rational.Zero; }
        }

        public Dictionary<string, object> AsDict()
        {
            return new Dictionary<string, object>
            {
                { "order_boost", OrderBoost.ToString() },
                { "atk_ev_bonus", AtkEvBonus.ToString() },
                { "engage_acc_delta", (long)EngageAccDelta },
                { "engage_pierce", (long)EngagePierce },
                { "deploy_cost_cut", (long)DeployCostCut },
                { "free_groups", (long)FreeGroups },
                { "health_bonus", (long)HealthBonus },
                { "cards", new List<object>(CardIds) },
            };
        }
    }

    /// The engagement delta one pipeline call applies to the hero threat
    /// (M11b threat v2 seam). Direction: the OWNING side's printed effect on
    /// the exchange - positive = the hero's attack exchange worsens (his
    /// threat value as a target shifts accordingly), negative = improves.
    public sealed class EngagementEffects
    {
        public int AccDelta;              // signed accuracy pips on the hero pool
        public int Pierce;                // printed Pierce of the owning side

        public bool IsNeutral { get { return AccDelta == 0 && Pierce == 0; } }

        public Dictionary<string, object> AsDict()
        {
            return new Dictionary<string, object>
            {
                { "acc_delta", (long)AccDelta },
                { "pierce", (long)Pierce },
            };
        }
    }

    /// The table itself: doc cache with the same seam as HeroCards /
    /// ImperialDecks (the harness and the live gate PRIME from file, the app
    /// loader primes from Resources - whoever runs first wins).
    public sealed class CardEffectTable
    {
        public readonly Dictionary<string, CardEffectEntry> ById =
            new Dictionary<string, CardEffectEntry>(StringComparer.Ordinal);
        public readonly Dictionary<string, Dictionary<string, string>> HookVocab =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

        public static CardEffectTable FromDoc(Dictionary<string, object> doc)
        {
            var t = new CardEffectTable();
            if (doc == null) return t;
            var vocab = Json.AsDict(Json.Get(doc, "hooks", new Dictionary<string, object>()));
            foreach (var kv in vocab)
            {
                var d = Json.AsDict(kv.Value);
                var row = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    { "consumed_by", Json.AsStr(Json.Get(d, "consumed_by", (object)"")) },
                    { "unit", Json.AsStr(Json.Get(d, "unit", (object)"")) },
                    { "law", Json.AsStr(Json.Get(d, "law", (object)"")) },
                };
                t.HookVocab[kv.Key] = row;
            }
            foreach (var raw in Json.AsList(Json.Get(doc, "cards", new List<object>())))
            {
                var r = Json.AsDict(raw);
                var e = new CardEffectEntry
                {
                    Id = Json.AsStr(Json.Get(r, "id", (object)"")),
                    Deck = Json.AsStr(Json.Get(r, "deck", (object)"")),
                    Basic = Json.AsBool(Json.Get(r, "basic", (object)false)),
                    Bucket = Json.AsStr(Json.Get(r, "bucket", (object)"c")),
                    Reason = Json.AsStr(Json.Get(r, "reason", (object)"")),
                };
                var xp = Json.Get(r, "xp", null);
                if (xp != null) e.Xp = (int)Json.AsLong(xp);
                foreach (var hraw in Json.AsList(Json.Get(r, "hooks", new List<object>())))
                {
                    var h = Json.AsDict(hraw);
                    e.Hooks.Add(new KeyValuePair<string, Rational>(
                        Json.AsStr(h["kind"]), Rational.Parse(Json.AsStr(h["value"]))));
                }
                foreach (var praw in Json.AsList(Json.Get(r, "partial", new List<object>())))
                    e.Partial.Add(Json.AsStr(praw));
                if (e.Id.Length > 0) t.ById[e.Id] = e;
            }
            return t;
        }

        /// Structural integrity the canon pins: every deck card exactly once,
        /// buckets legal, (c) carries no hooks, (a)/(b) carry at least one
        /// vocabulary hook, printed Attachment restrictions always carry the
        /// attach_lifecycle hook (hard RR rule).
        public List<string> ValidateAgainst(ImperialClassDeckTable decks)
        {
            var errs = new List<string>();
            if (decks == null) { errs.Add("no deck table"); return errs; }
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var deck in decks.Decks)
                foreach (var c in deck.Cards)
                {
                    if (!ById.TryGetValue(c.Id, out var e))
                    { errs.Add("missing card row: " + c.Id); continue; }
                    seen.Add(c.Id);
                    if (e.Bucket != "a" && e.Bucket != "b" && e.Bucket != "c")
                        errs.Add(c.Id + ": illegal bucket " + e.Bucket);
                    if (e.Bucket == "c" && e.Hooks.Count > 0)
                        errs.Add(c.Id + ": bucket c must carry no hooks");
                    if (e.Bucket != "c" && e.Hooks.Count == 0)
                        errs.Add(c.Id + ": bucket " + e.Bucket + " needs a hook");
                    foreach (var h in e.Hooks)
                        if (!HookVocab.ContainsKey(h.Key))
                            errs.Add(c.Id + ": hook not in vocabulary: " + h.Key);
                    bool attach = e.Hooks.Any(h => h.Key == "attach_lifecycle");
                    bool printedAttach = (c.Restriction ?? "").IndexOf("ttachment",
                        StringComparison.OrdinalIgnoreCase) >= 0;
                    if (printedAttach && !attach)
                        errs.Add(c.Id + ": printed Attachment without attach_lifecycle");
                    if (string.IsNullOrEmpty(e.Reason))
                        errs.Add(c.Id + ": empty reason");
                }
            foreach (var id in ById.Keys)
                if (!seen.Contains(id)) errs.Add("row for unknown card: " + id);
            return errs;
        }
    }

    public static class ImperialCardEffects
    {
        static Dictionary<string, object> _docCache;
        static CardEffectTable _table;

        public static void PrimeDoc(Dictionary<string, object> doc)
        {
            _docCache = doc;
            _table = null;
        }

        public static Dictionary<string, object> DocOrNull() { return _docCache; }

        public static CardEffectTable Load()
        {
            if (_table == null) _table = CardEffectTable.FromDoc(_docCache);
            return _table;
        }

        /// Aggregate the effects of the OWNED cards. Ownership is checked
        /// against the deck table (basics of the chosen deck are always
        /// owned; anything else must be in the acquired list). Deterministic:
        /// MAX/sum over a stable ordinal walk, never a hidden tie-break.
        public static CardEffects Aggregate(ImperialClassDeckTable decks,
                                            string deckId, IEnumerable<string> ownedIds)
        {
            var eff = new CardEffects();
            if (decks == null || string.IsNullOrEmpty(deckId)) return eff;
            var table = Load();
            var deck = decks.Find(deckId);
            if (deck == null) return eff;
            var owned = ownedIds == null
                ? new HashSet<string>(StringComparer.Ordinal)
                : new HashSet<string>(ownedIds, StringComparer.Ordinal);
            foreach (var card in deck.Cards.OrderBy(c => c.Id, StringComparer.Ordinal))
            {
                bool isOwned = card.Basic || owned.Contains(card.Id);
                if (!isOwned) continue;
                if (!table.ById.TryGetValue(card.Id, out var e)) continue;
                eff.CardIds.Add(card.Id);
                foreach (var h in e.Hooks)
                {
                    switch (h.Key)
                    {
                        case "order_boost":
                            if (h.Value > eff.OrderBoost) eff.OrderBoost = h.Value;
                            break;
                        case "atk_ev_bonus":
                            if (h.Value > eff.AtkEvBonus) eff.AtkEvBonus = h.Value;
                            break;
                        case "engage_acc_delta":
                            eff.EngageAccDelta += (int)h.Value.Num;
                            break;
                        case "engage_pierce":
                            eff.EngagePierce += (int)h.Value.Num;
                            break;
                        case "deploy_cost":
                            if ((int)h.Value.Num > eff.DeployCostCut)
                                eff.DeployCostCut = (int)h.Value.Num;
                            break;
                        case "free_group":
                            eff.FreeGroups += 1;
                            break;
                        case "stat_health":
                            eff.HealthBonus += (int)h.Value.Num;
                            break;
                        // focus_surcharge is per-card (condition-bearing):
                        // carried in CardIds, consumed by OverlordFocus via
                        // CardSurcharge() below. stat_speed/stat_note are
                        // audit-only (model value 0).
                    }
                }
            }
            return eff;
        }

        /// The light +1 target surcharge of the owned focus-condition cards
        /// (table-driven, NOT hard-coded per card): every owned card carrying
        /// the focus_surcharge hook and a condition the caller can see lights
        /// up. The caller (Rank) passes the candidate; the condition itself
        /// stays printed-text knowledge of the table, so the model applies
        /// the printed floor exactly once regardless of how many condition
        /// cards are owned (MAX law, M11h pin).
        public static int CardSurcharge(CardEffects eff)
        {
            if (eff == null || eff.CardIds.Count == 0) return 0;
            var table = Load();
            foreach (var id in eff.CardIds)
                if (table.ById.TryGetValue(id, out var e)
                    && e.Hooks.Any(h => h.Key == "focus_surcharge"))
                    return 1;
            return 0;
        }

        /// The engagement delta of the owned engage-hook cards.
        public static EngagementEffects EngagementOf(CardEffects eff)
        {
            if (eff == null || (eff.EngageAccDelta == 0 && eff.EngagePierce == 0))
                return null;
            return new EngagementEffects
            {
                AccDelta = eff.EngageAccDelta,
                Pierce = eff.EngagePierce,
            };
        }

        /// Effective-vitality seam (stat_health): the attrition weight the
        /// OrderPlanner consumes is the vitality fraction of EFFECTIVE Health,
        /// health / (health + bonus), clamped into [0, 1] the same way the
        /// caller already clamps its raw damage counter. Pure, no state.
        public static Rational EffectiveVitality(Rational rawVitality, int healthBonus)
        {
            if (healthBonus <= 0) return rawVitality;
            if (rawVitality <= Rational.Zero) return Rational.Zero;
            if (rawVitality >= Rational.One) return Rational.One;
            // printed Health H observed as vitality v = d/H with damage d:
            // effective = d/(H+N) = v*H/(H+N). Without H the caller must
            // pass the raw fraction; the model assumes the printed base the
            // caller already normalised against - the documented seam keeps
            // the RAW fraction and deflates it by H/(H+N) with H=1
            // placeholder ONLY when the caller cannot know H. The bridge
            // always knows H (deployment card print), so this branch is the
            // documented degraded path and pinned as such.
            return rawVitality / (Rational.One + new Rational(healthBonus));
        }

        /// M12e UI seam (API only, no UI here): one row per OWNED card with
        /// its bucket, hooks and the (c)-reason - "aktive Karten +
        /// Klassifikation". Attachment legality (trait restrictions are hard)
        /// rides along so the UI never offers an illegal attach.
        public static List<Dictionary<string, object>> ActiveCardInfos(
            ImperialClassDeckTable decks, string deckId, IEnumerable<string> ownedIds,
            string attachGroupTraits = null)
        {
            var rows = new List<Dictionary<string, object>>();
            if (decks == null || string.IsNullOrEmpty(deckId)) return rows;
            var table = Load();
            var deck = decks.Find(deckId);
            if (deck == null) return rows;
            var owned = ownedIds == null
                ? new HashSet<string>(StringComparer.Ordinal)
                : new HashSet<string>(ownedIds, StringComparer.Ordinal);
            foreach (var card in deck.Cards.OrderBy(c => c.Id, StringComparer.Ordinal))
            {
                if (!(card.Basic || owned.Contains(card.Id))) continue;
                if (!table.ById.TryGetValue(card.Id, out var e)) continue;
                var row = e.AsDict();
                row["name"] = card.Name;
                bool attach = e.Hooks.Any(h => h.Key == "attach_lifecycle");
                row["attachable"] = !attach || AttachLegal(card.Restriction, attachGroupTraits);
                rows.Add(row);
            }
            return rows;
        }

        /// Hard trait legality of one Attachment print (RR: groups without
        /// the trait cannot receive the attachment; "Attachment" without a
        /// trait list is unrestricted). Token scan over the printed
        /// restriction line - same ordinal word-vocabulary law as the deck
        /// families (ImperialDecks.FamilyCounts).
        public static bool AttachLegal(string printedRestriction, string groupTraits)
        {
            var r = (printedRestriction ?? "").ToUpperInvariant();
            if (r.IndexOf("ATTACHMENT", StringComparison.Ordinal) < 0) return true;
            // "Attachment" alone: legal for any group
            var traitPart = r.Substring(r.IndexOf(":", StringComparison.Ordinal) + 1);
            if (traitPart == r || traitPart.Trim().Length == 0) return true;
            var g = (groupTraits ?? "").ToUpperInvariant();
            foreach (var token in new[] { "TROOPER", "DROID", "HEAVY WEAPON", "VEHICLE",
                                          "GUARDIAN", "LEADER", "VILLAIN", "NON-CREATURE",
                                          "OFFICER" })
            {
                if (traitPart.IndexOf(token, StringComparison.Ordinal) >= 0)
                {
                    if (token == "NON-CREATURE")
                    {
                        // hard rule the other way: CREATURE groups are illegal
                        if (g.IndexOf("CREATURE", StringComparison.Ordinal) >= 0
                            && g.IndexOf("NON-CREATURE", StringComparison.Ordinal) < 0)
                            return false;
                    }
                    else if (g.IndexOf(token, StringComparison.Ordinal) >= 0)
                        return true;
                }
            }
            // trait list without any matching token in the group: illegal
            return false;
        }
    }
}
