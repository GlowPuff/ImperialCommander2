# Tactical AI — the Fair Imperial AI (wiki draft text)

_Draft for the Imperial Commander 2 wiki (https://github.com/GlowPuff/ImperialCommander2/wiki).
Suggested placement: a new wiki page "Tactical AI", linked from Home → Game Modes.
All UI labels below are the shipped English strings, verified against the app's language files._

---

## Tactical AI

**Tactical AI** is an optional game mode that replaces the random Imperial activation
routine with a tactical, rule-faithful Imperial brain. It is designed to feel like playing
against a competent human Imperial player — while remaining completely **fair** and
**explainable**.

The mode is fully optional: with the switch off (the default), the app behaves exactly as
before, down to the random activation order.

### The fairness rules

The Tactical AI is only ever allowed to know what a human Imperial player sitting at the
table could know:

- **Public board state only** — map geometry, deployment zones, ranges printed on the
  Imperial cards, doors, objectives and terminals.
- **No line-of-sight peeking** — the AI never computes the rebels' actual sight lines; it
  reasons about likely firing lanes instead.
- **No hidden information** — it never reads hidden event data, never rolls or looks at
  dice, and in Tactical mode the hidden bonus-effect rolls of the legacy mode **never
  fire**: the printed stat card is the only source of enemy power.
- **Hero damage stays manual** — you log damage on the hero trackers as usual; the AI
  uses those tracked deltas as input (and as a threat signal), never hidden rolls.
- **Every decision is explainable** — enable `EXPLAIN` in the settings and the app tells
  you *why* a group activated where it did and why a target was chosen.

### Turning it on

All controls live in the normal **Options window → "UI" tab**, at the bottom:

| Setting | Values | What it does |
|---|---|---|
| **TACTICAL AI** | off (default) / on | Master switch. Off = the app is untouched legacy. |
| **MODE** | `legacy` → `reference` → `overlord` | `legacy` = the old random routine. `reference` = planned activation order + tactical targeting. `overlord` = additionally applies the strict doctrine rules (hold/withdraw/focus disciplines). |
| **DIFFICULTY** | Recruit → Stormtrooper → Veteran → Commandant → Thrawn | The troops' competence curve (S1–S5). Higher levels coordinate focus fire, use cover and follow their doctrines more strictly. |
| **EXPLAIN** | `none` → `post_hoc` → `full` | Explanation texts under the AI's decisions (`full` also prints the "why" lines on the instruction popups). |
| **INFO TEXT** | on (default) / off | Adds the general reference-rules note (attack cap, door law) under the actions in the activation popup. |

In addition, every mission carries a teal **mission badge** (on the campaign mission list
card and on the mission setup panel) with the cycle **AI: AUTO → AI: REF → AI: ORD →
AI: OFF → AI: AUTO**. `AUTO` (the default) simply follows the global **MODE** setting, so
turning the master switch on is all you need; the per-mission overrides let you pin
individual missions to one mode (for example `OFF` for a legacy comparison run).

### What you will see change

- **Activation order is planned, not rolled.** Each round the Imperial side activates in a
  deliberate order (who goes first, who follows), announced with a plain-language line at
  round start.
- **Activation popups come straight from the figure.** Each Imperial activation shows its
  printed actions — one movement and one attack (the reference-rules action slots) — as a
  numbered "if–then" decision tree in the group's **doctrine**, e.g. _"Doctrine: Guard
  (hold the zone, never be lured out)"_. No drawn order cards, no random instruction
  option, no random hero naming.
- **Doctrines are situation-driven.** On defend-type missions, immobile and melee figures
  guard the objective and big bases block the lanes; on hunt-type missions nobody guards —
  everyone sweeps. Each figure's printed traits decide its role, never hidden data.
- **One focus target per round, announced openly.** The AI announces which hero the
  Imperials concentrate on and why (wounded first, then highest firepower) — information a
  human Imperial player would also have.
- **Emplacement figures (Stativ/Tripod, e.g. E-Web Troopers)** follow their printed rule:
  movement and attack are mutually exclusive in one activation. Their popup leads with the
  attack and warns that moving forfeits the shot.
- **Villain special abilities** printed on villain cards (force powers, IG-88's hacking
  and similar) appear as an additional optional action block on the activation popup
  whenever the printed conditions could apply — you decide at the board whether the
  condition is met.
- **In campaign play**, tactical campaigns add the **Imperial class deck**: the IMPERIUM
  panel shows the Empire's class deck and its XP purchases between missions, and an
  agenda market lets the Empire spend influence on agenda cards — the campaign-side
  counterweight to hero XP. These elements only exist while Tactical AI is active.

### What does NOT change

- Vanilla play is untouched when the mode is off — this is the default.
- All printed mission rules, events, XP and credits work as before.
- **Custom and embedded cards** (fan-made groups not in the shipped card set) keep the
  legacy popup and activation behaviour; the rest of the mission still runs tactically.

### Tips and known limits

- Don't switch the master toggle mid-mission — planning happens per round, but a clean
  mode change is between missions.
- Don't deploy the same enemy card twice as two separate groups: planner state is keyed
  per card id, and a second instance on the board is planned separately (known
  limitation). Normal squad sizes on one card (e.g. 2+3 stormtroopers) are fine.
- The AI has no line-of-sight model; wall-shooting is suppressed via probability zones,
  not by real sight-line checks.
- Hero damage must still be entered in the hero trackers by hand.
- In `overlord` mode the app prints a note if the tactical world feed is not active yet;
  planning then honestly follows the `reference` rules.
