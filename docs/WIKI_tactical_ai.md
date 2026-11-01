# Tactical AI — the Fair Imperial AI (wiki draft text)

_Draft for the Imperial Commander 2 wiki (https://github.com/GlowPuff/ImperialCommander2/wiki).
Suggested placement: a new wiki page "Tactical AI", linked from Home → Game Modes.
All UI labels, level behaviours and pop-up texts below were verified against the shipped
code and the English language files — nothing in this text is invented._

---

## Tactical AI

**Tactical AI** is an optional mode that turns the Imperial side from a random activation
spinner into a thinking opponent. Instead of rolling for "who activates next" and drawing a
random instruction, the app plans each Imperial round like a careful human player would —
and it shows its work.

Three design promises come with it:

- **Fair.** The AI only knows what a human Imperial player can see at the table: the board,
  the printed cards, the announced objectives and the hero damage you log yourself. It never
  reads hidden event data, never touches the dice, and the legacy hidden bonus-effect rolls
  **never fire** in Tactical mode — the printed stat card is the only source of enemy power.
- **Deterministic.** Same seed, same decisions. No hidden difficulty buffs, no rubber-banding.
- **Explainable.** Every order card can tell you *why* it did what it did.

With the master switch off (the default), the app is untouched legacy — bit for bit.

### Where to find it

Options window → **UI** tab, bottom section. There are exactly **four rows**:

| Row | Values | What it does |
|---|---|---|
| **TACTICAL AI** | OFF (default) / ON | Master switch. OFF = the app behaves exactly like before, including the random activation order. |
| **DIFFICULTY** | Recruit → Stormtrooper → Veteran → Commandant → Thrawn | How well the Imperials *choose* between their legal actions. It never changes stats, ranges, action counts or dice — only decisions. See "Difficulty" below. |
| **EXPLAIN** | NONE → AFTER THE FACT → FULL | The AI's reasoning texts. `AFTER THE FACT` explains decisions after they happen; `FULL` also prints the "why" lines directly on the activation pop-ups. |
| **INFO TEXT** | ON (default) / OFF | Adds the short rules reminder under the action texts on every activation pop-up (attack cap, door-opening rule). Turn it off once you know the drill. |

On top of that, every mission in a campaign list — and the mission setup panel — carries a
teal **level badge**. Clicking it cycles:

**AUTO → OFF → Recruit → Stormtrooper → Veteran → Commandant → Thrawn → AUTO**

- `AUTO` (default): the mission follows your global DIFFICULTY setting. Turning the master
  switch on is all you need — no per-mission work required.
- A named level on the badge pins **this mission** to that difficulty, whatever the global
  setting says (handy for tuning one specific mission).
- `OFF` forces this single mission back to the legacy random behaviour — e.g. to play a
  comparison round against the old AI.

### Difficulty — what each level actually changes

Difficulty in Tactical AI is **play strength, never hidden power**. Every level plays with
the exact same legal options, the same printed stats and the same threat budget the mission
gives the Imperials. What changes is *how well the Imperials pick* among those options.
In board terms: same army, different general.

**Recruit (S1)** — the old "automaton" behaviour. Recruits focus on whoever happens to be
closest or first in their own bookkeeping; they don't read the hero damage trackers, so they
rarely finish off a wounded hero, and they concentrate their fire only by accident. They
spend their whole threat budget immediately (activating everything they can, as fast as
possible), move to the first legal position they find, and react to what the rebels did two
rounds ago. Honest and forgiving — good for your first runs or for younger players.

**Stormtrooper (S2)** — still doesn't read damage trackers (no deliberate focus fire), but
now usually picks one of the two *sensible* targets, sometimes fumbles (about one fumble in
three activations). Movement improves to greedy steps toward contact, and the threat budget
is mostly held back for later activations instead of spent in one burst. The Imperial
masses get reliably into the fight, but they still won't set up crossfires or use cover
deliberately.

**Veteran (S3)** — the turning point: Veterans **read the hero damage trackers**. That
unlocks deliberate focus fire: wounded heroes get finished first, the highest-firepower hero
draws the shots, and the round's focus target is announced openly at round start. Veterans
react to rebel events with a one-round delay and take reasonable approach paths (shortest
route, but without dodging known fire lanes). At this level the Imperials start to feel like
a competent squad — expect real pressure if you split your party.

**Commandant (S4)** — the tactics turn professional. The Imperials now plan **two rounds
ahead** (they will step into a position that cuts off a hero who is about to run, instead
of chasing yesterday's position), fumbles are rare, and movement uses the full planner —
cover, firing lanes, avoiding known hero fire. From Commandant up the AI also plays the
**printed special abilities** of villain cards (the third action block on the pop-up) when
the printed conditions fit — force powers, IG-88's hacking, and similar are now part of the
game.

**Thrawn (S5)** — everything above, plus the complete risk judgement: the Imperials weigh
each option against a full threat calculation (including your campaign-side reputation
pressure) and essentially always take the best legal move on the board. Expect interceptions
before you reach them, lanes kept clean for fire, wounded heroes hunted to the death, and
no free steps. This is the "play it like the books say — perfectly" end of the ladder.

> Implementation note: internally S1–S3 run the app's "reference" decision brain and S4–S5
> the "overlord" brain. That is not a setting — the DIFFICULTY ladder (and the mission
> badges) are the only knob you need.

### The activation pop-ups — how to read them, and how to play them well

With Tactical AI on, the old random "instruction card" is replaced by a deterministic
**activation order** (planned each round, announced with a plain-language round line) and
**activation pop-ups that come straight from the figure** — no order decks, no drawn cards.
Each pop-up reads top to bottom:

1. **Focus line** (when a target is chosen): which hero the Imperials are concentrating on
   and *why* — e.g. "he is wounded — finish him first" or "highest firepower among the
   remaining heroes". This information is deliberately public; a human Imperial player has
   it too.
2. **Doctrine line**: the role this figure plays in *this mission*, named in plain words —
   e.g. `Doctrine: Guard (hold the zone, never be lured out)`. Doctrines are
   situation-driven: on defend-type missions, immobile and melee figures guard the
   objective, big bases block the lanes; on hunt-type missions nobody guards — everyone
   sweeps. Each figure's printed traits decide its role, never hidden data.
3. **The actions — an if–then decision tree.** The reference rules give every Imperial
   figure two action slots per activation, at most one of which is an attack. Each action
   block is printed as conditions, not as a hardcoded order:

   > *Action 1 — Focus target within attack range and clear line of sight? Then: attack the
   > focus target. No hero reachable? Then: close in on the focus target until the figure
   > is within attack range.*

   **You (the human Overlord) evaluate the conditions on the real board.** The tree shows
   the AI's strongest play with its public information; you check the actual positions,
   ranges and sight lines and execute accordingly. How to play it *best for the Empire*:
   - **Work the tree top-down.** The first condition that is true on your board IS the
     order. The lines below it only apply when the ones above are false.
   - **One attack per activation** (the info text reminds you): unless the figure's own
     card prints multiple attacks, the attack slot goes to exactly one hero — give it to
     the announced focus target, not to whoever happens to be closest.
   - **Movement serves the fire plan.** For "Advance" figures that means closing until the
     focus target is inside range; for "Guard" and "Blocker" figures it means *not* moving —
     holding the objective zone or the chokepoint is worth more than one more step.
   - **Doors cost an action** (interact). Opening one means skipping attack or movement
     that activation — decide who opens, or route around.
   - **Emplacement figures (Stativ/Tripod, e.g. E-Web Troopers)** follow their printed
     rule: movement and attack are mutually exclusive in one activation. Their pop-up
     leads with the attack and reminds you: moving forfeits the shot. Best for the Empire:
     let them hold a lane and pour fire into anything that crosses it — never chase.
4. **Optional third block — printed villain abilities** (from Commandant up). When a
   villain card prints a special action, it appears as its own block carrying the printed
   condition text ("within 4 spaces and line of sight…"). You decide at the board whether
   the condition is met; the block proposes, it never overrides the rules.
5. **Explanation line** (with EXPLAIN at FULL): the why behind this activation — target
   ranking, doctrine, risk posture — right on the pop-up.

### What does NOT change

- Vanilla play is untouched while the master switch is off — this is the default.
- All printed mission rules, events, XP, credits and the campaign manager work as before.
- **Custom and embedded cards** (fan-made groups not in the shipped card set) keep the
  legacy pop-up and the legacy activation draw; the rest of the mission still runs
  tactically.
- In Tactical mode, the legacy random **hidden bonus-effect rolls never fire** (fairness
  rule). Printed stat cards only.

### Campaign extras (Tactical mode only)

Tactical campaigns add the Imperial counterweights from the printed campaign game:

- **Imperial class deck** — the **IMPERIUM panel** shows the Empire's class deck and its
  between-mission XP purchases; acquired cards strengthen the Imperial side (activation
  efficiency and printed strength), in the same open way hero XP works for you.
- **Agenda market** — the Empire spends influence on agenda cards between missions.

### Tips and known limits

- Don't switch the master toggle **mid-mission** — planning is per round, but a clean mode
  change belongs between missions.
- **Don't deploy the same enemy card twice as two separate groups**: planner state is keyed
  per card id, and a second copy of the same card on the board is planned separately (known
  limitation). Normal squad sizes on one card (e.g. 2+3 stormtroopers) are fine.
- The AI has **no line-of-sight model**: it never peeks through walls. Wall-shooting is
  suppressed via probability zones, not by real sight-line checks — that's also why *you*
  still enforce sight lines at the board.
- **Hero damage stays manual**: enter it in the hero trackers; the AI reads those values
  (and focuses accordingly from Veteran up).
- On missions where the tactical world feed is not active yet, the higher levels fall back
  to the reference brain — the app says so in the log instead of faking it.
