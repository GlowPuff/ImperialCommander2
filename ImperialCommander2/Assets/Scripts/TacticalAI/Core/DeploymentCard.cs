// Port of sim/cards.py — DeploymentCard data model + fairness whitelist.
// The Tactical AI may ONLY use values printed on the card. Hidden buff data
// (bonuseffects.json-style) has no home in this class.
using System;
using System.Collections.Generic;

namespace TacticalAI.Core
{
    public sealed class DeploymentCard
    {
        public string Id;
        public string Name;
        public string MiniSize;
        public int Speed;
        public int Health;
        public string AttackType;           // "Ranged" | "Melee"
        public string[] Attacks;            // printed attack dice colours
        public HashSet<string> Keywords = new HashSet<string>();
        public bool IsEliteCard;
        public string Subname = "";

        // printed surge/defence lines (optional catalog fields; empty = none)
        public string[] SurgeLines = Array.Empty<string>();
        public string[] Defense = Array.Empty<string>();
        public int Cost = 1;                // printed deck-build point cost

        public bool IsMassive => Keywords.Contains("Massive");
        // M13t (EMPIRE6 playtest, Thomas 30.10.): printed Stativ/Tripod rule
        // - leaving your space and attacking are mutually exclusive within
        // one activation. The app layer injects this keyword from the
        // card's PRINTED ability (En "Tripod" / De "Stativ"); it is public
        // card text the human reads on the physical card, fairness-safe.
        public bool IsTripod => Keywords.Contains("Tripod");
        // Noldorion playtest 2.11 (PR #194): the printed Assault rule -
        // "can perform multiple attack actions each activation" (En print;
        // De "Sturm", Ru "Штурм", Fr "Assaut"). Same printed-fact class as
        // Tripod: the app layer injects the keyword from the card's own
        // ability line (DG-belt: across ALL shipped language assets exactly
        // DG007/DG008/DG046/DG071 print Assault).
        public bool HasAssault => Keywords.Contains("Assault");
        public bool IsMobile => Keywords.Contains("Mobile");
        public bool HasReach => Keywords.Contains("Reach");
        public bool IsLargeBase => Geometry.IsLarge(MiniSize);
        // M4 (Thomas 29.10.): doctrine pattern assigned by the APP layer at
        // catalog build (MissionPosture x printed traits, bridge-side). Core
        // data class default keeps every pre-M4 caller aggressive - the sim
        // harness sets Pattern per roster itself, vectors stay untouched.
        public string Pattern = "aggressive";

        public int[] Footprint(int orientation = 0) => Geometry.FootprintDims(MiniSize, orientation);

        // Fields allowed on a Tactical-AI card record; anything else is a
        // red flag and is TOLERATED but never consulted.
        public static readonly HashSet<string> CardFieldWhitelist = new HashSet<string>
        {
            "id","name","subname","miniSize","speed","health",
            "attackType","attacks","keywords","isElite","defense","surges",
            "traits","faction","characterType","tier","cost","size","priority",
        };

        public static DeploymentCard FromRecord(Dictionary<string, object> rec)
        {
            var ct = Json.Get(rec, "characterType", "Imperial");
            if (Json.AsStr(ct) != "Imperial")
                throw new InvalidOperationException("card " + Json.Get(rec, "id") + ": characterType must be Imperial");
            var c = new DeploymentCard
            {
                Id = Json.AsStr(rec["id"]),
                Name = Json.AsStr(rec["name"]),
                MiniSize = Json.AsStr(rec["miniSize"]),
                Speed = Json.AsInt(rec["speed"]),
                Health = Json.AsInt(rec["health"]),
                AttackType = Json.AsStr(rec["attackType"]),
                Attacks = StrArray(Json.Get(rec, "attacks", new List<object>())),
                Subname = Json.AsStr(Json.Get(rec, "subname", "")),
                Cost = Json.AsInt(Json.Get(rec, "cost", 1L)),
                IsEliteCard = Json.AsBool(Json.Get(rec, "isElite", false)),
            };
            foreach (var k in Json.AsList(Json.Get(rec, "keywords", new List<object>())))
                c.Keywords.Add(Json.AsStr(k));
            c.SurgeLines = StrArray(Json.Get(rec, "surges", new List<object>()));
            c.Defense = StrArray(Json.Get(rec, "defense", new List<object>()));
            return c;
        }

        static string[] StrArray(object v)
        {
            var list = Json.AsList(v);
            var outl = new string[list.Count];
            for (int i = 0; i < list.Count; i++) outl[i] = Json.AsStr(list[i]);
            return outl;
        }

        /// Load a tacai unit catalog (top-level list or {source, cards}).
        public static Dictionary<string, DeploymentCard> LoadCatalog(object root)
        {
            List<object> records;
            if (root is Dictionary<string, object> d && d.ContainsKey("cards"))
                records = Json.AsList(d["cards"]);
            else
                records = Json.AsList(root);
            var cards = new Dictionary<string, DeploymentCard>();
            foreach (var rec in records)
            {
                var c = FromRecord(Json.AsDict(rec));
                if (cards.ContainsKey(c.Id))
                    throw new InvalidOperationException("duplicate card id " + c.Id);
                cards[c.Id] = c;
            }
            if (cards.Count == 0) throw new InvalidOperationException("empty catalog");
            return cards;
        }
    }

    /// One figure on the board. Position fields are mutable (the world moves).
    public sealed class Figure
    {
        public string Id;
        public string GroupId;
        public DeploymentCard Card;
        public string Team;          // "imperial" | "rebel" | "npc"
        public Cell Anchor;
        public int Orientation;
        public int Vitality;         // remaining vitality per figure (wheel)
        public int Index;            // activation/initiative order
        public bool Alive = true;
        public string Pattern = "aggressive";
        // M9-F6 hero three-state model: the isWounded STATUS (public app
        // flag). Hero damage itself is physical table state the app never
        // sees - a hero becomes wounded when he loses a FULL bar and
        // fights on a fresh second one (runner.Figure.take_damage mirror).
        public bool Wounded;

        public List<Cell> Cells() => Geometry.FootprintCells(Anchor, Card.MiniSize, Orientation);
    }
}
