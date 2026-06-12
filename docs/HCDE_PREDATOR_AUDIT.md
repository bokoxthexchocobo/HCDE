# HCDE Roadmap #12 — Predator Mode

**Last updated:** 2026-06-15
**Status:** Phase 1 scaffold and Phase 2 snapshot contract landed
default-off; role gameplay, radar, stealth, and kill-reward loop remain pending.

## Naming

The mode is **Predator mode** (not "Predator Economy"). Currency and
loadouts may still exist, but the identity is asymmetric hunt/survival —
one hidden predator vs armed survivors and active monsters — not a
Counter-Strike buy-meta clone.

## What #12 means

Board item: **Predator mode** — a **survival-based** multiplayer mode where
most players are survivors with **one life per round** hunting (or evading)
a server-assigned predator while monsters remain a live threat. Kill rewards
fund survivor upgrades; taking down the predator pays the largest bonus.
Dead survivors **do not respawn** until the round ends.

This document does NOT specify final balance. It records the **target
gameplay**, the **engine surface** the mode needs, and the **authority
rules** that surface must follow.

## Target gameplay (design notes — 2026-06)

Captured from design discussion. These are intent, not implemented behavior.

### Roles

| Role | Intent |
| --- | --- |
| **Predator** | One player selected by the server each round. During **Setup**, picks a **predator archetype** (monster-inspired class) with distinct strengths and weaknesses — e.g. fast runner with low health vs slow bruiser with high health. Granted **invisibility** until **fire or kill** (archetype may tune reveal noise/radar signature). Weapon kit may vary per archetype. |
| **Survivors** | Armed players with **no respawn** this round. During **Setup**, each survivor picks a **survivor class** (human/marine-inspired roles with distinct kits and stat tradeoffs). Earn **currency for monster kills** while alive. Coordinate via radar and **hunt the predator** for the largest payout. Death means spectating until **End**. |
| **Monsters** | Always-on ambient threat. They **move, attack, and respawn** throughout the round (reuse Invasion/spawn director patterns where possible). Monster deaths are expected; replenishment keeps pressure on shrinking survivor numbers. |

### Shared systems

- **Radar (Aliens-style).** Every player sees a live enemy radar — blips
  for monsters, other players, and (with rules TBD) the predator. Exact
  fidelity (always-on vs ping, predator signature, stealth grace after
  reveal) is TBD.
- **Kill rewards.** Server-authoritative currency grants:
  - baseline: monster kills
  - bonus: predator kill (largest reward)
  - predator kills may grant smaller or zero currency depending on balance
- **Reveal rules.** Predator stealth breaks on **weapon discharge** and/or
  **confirmed kills**. Re-stealth after a cooldown is an open tuning knob.

### Predator archetypes (monster-class pick)

Before each round's **Hunt** phase, the assigned predator player **chooses
an archetype** — not a free-roaming invasion monster, but a **player
predator build themed on a strong monster class**. Each archetype trades
stats and playstyle:

| Example archetype | Strengths | Weaknesses |
| --- | --- | --- |
| **Stalker** (fast fiend/revenant-style) | High speed, agile melee, quick reposition | Lower health, louder reveal ping on radar when moving fast |
| **Bruiser** (baron/knight-style) | High health, heavy melee, wins direct fights | Slow movement, larger radar signature even while "invisible" |
| **Skulker** (imp/spectre-style) | Quieter on radar, shorter reveal decay, good ambush | Mid health, lighter damage without setup |
| **More TBD** | Mod/ZScript data-driven | Balance pass required |

**Design intent**

- Pick happens in **Setup only** — server validates choice, replicates
  archetype id in the predator snapshot. No mid-round class swap.
- Archetypes modify **player predator stats** (speed, health, damage,
  height, sound radii, optional melee) — they do **not** replace the
  predator with an AI monster actor.
- The predator remains a **human player** with stealth + role weapons;
  monster class is a **loadout template**, not membership in the ambient
  monster spawn pool.
- Survivors may learn archetype **after reveal** (HUD callout, distinct
  pain/death sounds, or brief silhouette) so counter-play is possible.

**Engine surface (future)**

- `EHCDEPredatorArchetype` enum or string table in `d_net_predator.h`
- Snapshot field: `PredatorArchetypeId` alongside `PredatorPlayerNum`
- ZScript: derived classes under `wadsrc/static/zscript/actors/predator/`
  (e.g. `HCDEPredatorPawn_Stalker`) or a single pawn with data-driven
  stat blocks
- Setup UI: ZScript menu or console `predator_pick <archetype>` validated
  by server during Setup window
- CVAR candidates: `sv_predator_archetype_allowlist`, per-archetype enable
  flags for operators

**Open archetype questions**

1. **Visual model** — third-person only for predator, or first-person with
   themed hands? Full monster sprite risks confusion with real monsters on
   radar unless rules differ.
2. **Weapons per archetype** — shared predator arsenal with stat mods, or
   unique weapon sets per class?
3. **Rotation fairness** — random archetype if no pick before Setup timer
   expires?

### Survivor classes (player pick)

Every **survivor** (non-predator player) also chooses a class during
**Setup**. Survivor classes are **human team roles** — not monster
templates — tuned for hunting, holding ground, or farming monsters under
survival pressure:

| Example class | Strengths | Weaknesses |
| --- | --- | --- |
| **Assault** | Balanced weapons, solid DPS vs monsters and predator | No standout team utility |
| **Scout** | Faster movement, wider radar range or faster radar refresh | Lower health, weaker direct duel vs predator |
| **Heavy** | High health, damage resistance, anchors chokepoints | Slow, loud on radar, poorer monster-farming tempo |
| **Technician** | Bonus currency from monster kills, deployable gadgets TBD | Light weapons, relies on positioning |
| **More TBD** | Mod/ZScript data-driven | Balance pass required |

**Design intent**

- Pick in **Setup only** — server validates, replicates per-player
  `SurvivorClassId` in the predator snapshot (or a dedicated per-player
  mode slice). No mid-round respec.
- Classes define **starting loadout**, **movement/health modifiers**, and
  optional **team utility** (radar buffs, traps, healing — TBD). Kill-earned
  currency still applies; classes may modify earn rate or spend discounts.
- **Duplicate classes allowed** by default (multiple Scouts, etc.) unless
  operators enable `sv_predator_unique_classes` or similar.
- Late joiners who missed Setup **cannot pick a class** that round — they
  spectate until End, then pick on the next Setup.

**Engine surface (future)**

- `EHCDEPredatorSurvivorClass` enum or string table in `d_net_predator.h`
- Snapshot: `PlayerSurvivorClass[MAXPLAYERS]` + validity flags (mirror
  currency fields in `FHCDEPredatorSnapshotV1`)
- ZScript: `HCDEPredatorSurvivorPawn` base or extend existing `PlayerPawn`
  with class data blocks under `wadsrc/static/zscript/actors/predator/`
- Setup command: `predator_class <class>` or shared `predator_pick` with
  role gating (predator vs survivor)
- UI: class select during Setup window alongside predator archetype pick

**Open survivor-class questions**

1. **Team composition caps** — max one Technician? min one Scout for radar
   fantasy? Operator CVARs vs hardcoded limits.
2. **Currency interaction** — flat earn rates per class, or class only
   affects starting gear and stats?
3. **Visual identity** — distinct player skins per class for predator
   counter-play after reveal.

### Class selection flow (Setup)

```text
Setup opens
  -> server assigns predator player
  -> predator picks archetype (monster-themed)
  -> each survivor picks class (human-themed)
  -> optional short armory / starting-currency spend
  -> Hunt begins (locks all picks)
```

### Survival rules

- **Survivors: one life per round.** Player deaths are permanent for the
  current round. No respawn, no mid-round re-entry. Eliminated survivors
  become spectators until the round ends.
- **Predator: one life per round** (same survival stakes). If the predator
  dies, survivors win the round (predator-kill bonus applies). Predator
  death handling is authoritative on the server.
- **Monsters: respawn allowed.** Monster population is replenished during
  Hunt so threat stays high as survivor count drops. This is intentional —
  the round gets harder for the living, not easier.
- **Late join: spectate only.** Players who connect after a round has
  started are **forced to spectator** until **End**, then eligible for the
  next round's Setup. No drop-in as survivor or predator mid-Hunt.

### Win / loss conditions (draft)

| Outcome | Condition |
| --- | --- |
| Survivors win | Predator killed, or round timer expires with at least one survivor alive |
| Predator wins | All survivors eliminated (by predator and/or monsters) |
| Draw / timeout edge cases | TBD (e.g. predator alive, one survivor alive at timer — likely survivor win) |

### Round flow (revised from early scaffold)

1. **Waiting** — enough players to start.
2. **Setup** — server assigns predator (`pr_predator`); predator **picks
   archetype**; each survivor **picks class**; starting loadout/currency
   applied from class tables.
3. **Hunt** — monsters spawn, roam, and **respawn**; survivors farm monsters
   and search for the predator; predator picks off survivors (**no survivor
   respawn**). Radar active for all. Late joiners spectate.
4. **End** — round scoring, currency tallies, predator rotation; spectators
   and late joiners become eligible for the next Setup.

The early scaffold's dedicated **Buy** phase may shrink to a short
**armory** window, merge into setup, or be replaced entirely by
**earn-only** spending between rounds — see open questions.

### Open design questions

1. **Single predator vs two teams.** Default assumption: **one predator
   player** per round for clarity. A two-team variant (predator squad vs
   survivors) is possible for high player counts but blurs the fantasy.
2. **Radar vs invisibility.** If radar always shows the predator, stealth
   must be implemented as **delayed / imprecise blips**, **noise events**
   on gunfire, or **sector-limited** detection — not literal client-side
   invisibility alone.
3. **Buy phase fate.** Current CVAR `sv_predator_buy_seconds` and buy
   opcodes are scaffold-only. Vision favors **kill-earned currency** over
   a CS-style buy timer; armory UI may be between-round only.
4. **Invasion overlap.** Predator mode and Invasion both drive monster
   presence. Assume **mutually exclusive** gametypes at the server level
   until a combined soak proves safe.
5. **Spectator UX.** Late join and dead-player spectate need a clear camera
   mode and HUD (radar may be spectator-only or disabled — TBD).
6. **Archetype vs ambient monsters.** Predator archetypes must be
   distinguishable from respawning map monsters on radar and audio, or
   survivors cannot tell hunter from horde.
7. **Class picks are Setup-only.** Survivor and predator selections lock
   at Hunt start; late joiners spectate without a class pick that round.

## Reference shape (engine)

Predator mode follows the same integration pattern as Invasion — explicit
states, server-owned director, snapshot replication:

```text
Waiting -> Setup -> Hunt -> End -> (rotate predator) -> Waiting
```

Early scaffold used `Buy` instead of `Setup`; renaming is documentation
only until the state enum is updated in code.

## Existing in-tree surface to reuse

- **State machine pattern.** `EInvasionState`, `FInvasionWaveDirector` in
  `src/d_net_invasion.cpp`. Predator mode uses `EHCDEPredatorState` and
  `FHCDEPredatorRoundDirector` in `src/d_net_predator.cpp`.
- **Replicated mode state.** `FHCDEPredatorSnapshotV1` with capability
  gating (`HCDELiveCapPredatorSnapshotV1`), mirroring Invasion.
- **Authority events.** Currency, role assignment, reveal events, and round
  transitions are server-owned. Cosmetic-only effects may use `DEM_*` lanes.
- **Named RNG.** `pr_predator` for deterministic predator selection across
  saves/demos.
- **Monster pressure.** Invasion spawn directory, AI director hints (#13),
  and existing playsim monster tick — predator rounds need monsters without
  duplicating Invasion's wave victory conditions.

## Hard rules (server authority)

1. **Currency lives on the server.** Clients see replicated balances only.
2. **Purchases / loadout changes are commands.** Server validates phase,
   funds, and item legality before granting gear.
3. **Predator role is server-assigned** via `pr_predator`. No self-promote.
4. **Stealth and radar are server-informed.** Clients render what snapshots
   and events allow; predator visibility state must not be client-writable.
5. **Round timer is server-side.** Clients display replicated countdowns.
6. **No survivor respawn mid-round.** Death transitions the player to
   spectator; only the server may start a new life on the next round.
7. **Late join is spectate-only** during Hunt. Join-in-progress as a
   combatant is rejected until End → Waiting/Setup.
8. **Monster respawn is server-driven** during Hunt; survivor deaths do not
   pause or reduce spawn pressure unless tuned by CVAR.
9. **Cheat scoping.** While `sv_predator_enable` is on, cheat opcodes are
   rejected unless `sv_predator_allow_cheats=1`.

## Phased plan

- **Phase 0 (this doc).** Boundary + target gameplay captured.
- **Phase 1 (landed).** Skeleton in `src/d_net_predator.cpp`: state enum,
  director stub, CVARs, `predator_status` CCMD.
- **Phase 2 (landed, partial).** `FHCDEPredatorSnapshotV1` contract +
  build/apply helpers. Wire encoding still pending.
- **Phase 3 (partial).** Buy net-event path + cheat scoping landed; revisit
  whether buy becomes armory/earn-only per design notes above.
- **Phase 4.** Role gameplay:
  - predator selection + `HCDEPredatorPawn` wiring
  - Setup-phase **predator archetype** + **survivor class** picks
  - invisibility / reveal on fire and kill (per-archetype tuning)
  - per-class loadouts (predator archetypes + survivor classes)
  - Aliens-style radar replication (signatures per role/class)
  - monster spawn/roam loop during Hunt
  - kill-reward currency (monsters + predator bonus; class earn modifiers)
- **Phase 5.** Soak on dedicated server: late-join spectate, no-respawn
  death flow, monster respawn under load, demos, RCON `predator_status`,
  stealth/radar desync checks.

## What this is NOT

- Not a rewrite of deathmatch scoring. Predator mode sits beside DM the way
  Invasion sits beside coop.
- Not a client-authoritative stealth mode. Reveal and radar must replicate.
- Not a cheat-elevation path.

## Source map

| Concern | File |
| --- | --- |
| Mode core | `src/d_net_predator.cpp`, `src/d_net_predator.h` |
| ZScript pawn surface | `wadsrc/static/zscript/actors/predator/predator_player.zs` |
| Snapshot wire format | `src/d_net_snapshot_part1.cpp` / `part2.cpp` (follow-up) |
| Validation harness | `tests/predator_validation/` |

## CVARs (scaffold)

| CVAR | Role today | Notes under new vision |
| --- | --- | --- |
| `sv_predator_enable` | Master switch | Unchanged |
| `sv_predator_round_seconds` | Hunt timer | Unchanged |
| `sv_predator_buy_seconds` | Buy-phase length | May become setup/armory or deprecated |
| `sv_predator_starting_currency` | Round-start money | May pair with kill-earned grants |
| `sv_predator_allow_cheats` | Cheat whitelist | Unchanged |
