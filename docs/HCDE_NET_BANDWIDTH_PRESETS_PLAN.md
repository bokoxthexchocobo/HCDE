# HCDE Network Bandwidth Presets Plan

Status: draft plan for review.

Goal: let HCDE scale its client/server replication budget for the current mod, game mode, and runtime load without disturbing the local-player prediction and authority fixes that are already improving Doom 2 co-op. The system should keep competitive/light games lean, while giving heavy mod stacks such as Brutal Doom or Monsters and Addons enough replication headroom to avoid stale actors, delayed monster/projectile updates, and visible lag.

## Why This Is Feasible

HCDE already has most of the right foundation:

- Live traffic is separated into lanes such as control, command, authority, player snapshot, actor delta, query registry, and presentation echo.
- Lane budgets are already capability-gated through `HCDELiveCapLaneBudgetsV1`.
- `HCDELiveLaneBudgetBytes()` and `HCDELiveLaneBudgetEnd()` already enforce per-lane byte ceilings.
- The actor-delta lane already tracks budget clamps and deferred actor candidates.
- The projectile path already has interest tiers; the shared co-op actor path still uses a simpler round-robin cursor.

The bandwidth preset system should build on those pieces instead of replacing the prediction/reconciliation work.

## Design Principle

Presets should control world replication, not local-player prediction.

Keep these concerns separate:

- Local-player feel: prediction lead, authoritative rebase, pitch handling, use/fire input semantics, and cutscene/player-count handling.
- World replication: monsters, projectiles, pickups, map actors, scripted actors, and their update cadence.

This keeps the Doom 2 movement progress protected. A bad preset should make monsters update less often, not change how the local player walks.

## Presets

Add a server-side preset CVAR:

- `sv_net_bandwidth_preset`
  - `auto` (default)
  - `light`
  - `medium`
  - `heavy`
  - optional future: `custom`

Suggested initial lane budgets:

| Lane | Current | Light | Medium | Heavy |
| --- | ---: | ---: | ---: | ---: |
| Control | 96 | 96 | 128 | 160 |
| Command | 4096 | 4096 | 4096 | 4096 |
| Authority | 384 | 384 | 512 | 768 |
| Player snapshot | 4096 | 4096 | 4096 | 4096 |
| Actor delta | 900 | 900 | 1800 | 3600 |
| Query registry | 512 | 512 | 768 | 1024 |
| Presentation echo | 512 | 512 | 768 | 1024 |

Rationale:

- `light` should match today's behavior as closely as possible.
- `medium` should help monster-heavy co-op without overfeeding bandwidth.
- `heavy` should target Brutal Doom style mods with many active monsters, missiles, gibs, and script-driven actors.
- Command and player snapshot lanes should stay conservative because they directly affect local feel and reliability.
- Actor delta is the main lane to scale because Brutal Doom pressure is mostly actor churn and stale world-state updates.

## Auto Preset Selection

`auto` should choose a starting preset before gameplay and then adapt slowly at runtime.

Startup heuristics:

- Game mode:
  - duel/deathmatch: start `light`.
  - vanilla co-op or low actor count: start `light`.
  - invasion/co-op with many monsters: start `medium`.
- Mod fingerprint:
  - Known heavy mod names or loaded files: start `heavy` for Brutal Doom, Monsters and Addons, or similar compat profiles.
  - Unknown mod stacks with many DECORATE/ZScript classes: start `medium`.
- Map scan:
  - Few monsters/projectiles: `light`.
  - High monster count or many script actors: `medium`.
  - Very high monster count plus known effect-heavy mod: `heavy`.

Runtime promotion signals:

- Sustained actor-delta budget clamps.
- Sustained `HCDERecordLiveLaneDeferred(HLANE_ACTOR_DELTA, ...)` activity.
- Shared actor queue deferred candidates climbing.
- Actor baseline repairs triggered by stale world deltas.
- Snapshot actor update age exceeding a target threshold for nearby monsters/projectiles.

Runtime demotion signals:

- No actor-delta clamps for a long window.
- Low actor churn and low active actor count.
- Stable packet sizes below budget.
- No recent stale-actor repairs.

Promotion/demotion should be slow and hysteresis-based:

- Promote after several seconds of sustained pressure.
- Demote only after a longer quiet window.
- Never switch more than one preset level at a time.
- Log every automatic switch with reason and counters.

## Interest-Aware Actor Delta

The biggest quality improvement is not just raising budgets. HCDE also needs to spend the budget on the most important actors first.

Phase 1 should extend the existing interest-tier idea to the shared co-op actor path:

- Critical:
  - Players and player-owned immediate threats.
  - Projectiles near any player or in the player's view.
  - Monsters attacking, recently damaged, or close to a player.
- High:
  - Active monsters within a useful radius.
  - Moving projectiles not close enough for critical.
  - Doors, lifts, and solid map actors near players.
- Medium:
  - Distant active monsters.
  - Pickups near likely player paths.
- Low:
  - Distant idle monsters.
  - Static pickups after baseline.
- Dormant:
  - Far idle actors with valid baseline and no recent changes.

Budget fill should become priority-first with keep-alive trickles, not pure round-robin.

Important rule: every actor that is skipped due to relevance must still receive an occasional keep-alive if it has a valid baseline, so clients do not permanently lose distant state.

## Mod And Compatibility Handling

Do not edit third-party PK3/WAD files.

For mod-specific behavior:

- Add HCDE-owned compatibility metadata for known heavy mods.
- Allow compat profiles to recommend a bandwidth preset.
- Allow compat profiles to classify known cosmetic actors as client-side or low-priority when legally and technically safe.
- Keep original third-party archives untouched.

This lets HCDE support Brutal Doom and similar mods without repacking them.

## CVARs And Diagnostics

Recommended CVARs:

- `sv_net_bandwidth_preset auto|light|medium|heavy|custom`
- `sv_net_bandwidth_autoscale 1`
- `sv_net_actor_delta_budget_custom <bytes>`
- `sv_net_bandwidth_debug 0|1|2`
- `net_bandwidth_profile_dump`

Recommended debug output:

- current preset
- chosen startup reason
- per-lane TX bytes
- per-lane clamp count
- actor-delta deferred count
- active replicated actors by category
- average and max actor update age
- auto-promotion/demotion events

The debug output should be easy to compare between Doom 2 and Brutal Doom sessions.

## Safety Guardrails

- Default to `auto`, but make `auto` begin as `light` unless evidence says otherwise.
- Never change local prediction CVARs from this system.
- Cap heavy budgets to avoid packet fragmentation and runaway bandwidth.
- Keep command and player snapshot budgets conservative.
- Make preset selection server-authoritative and visible to clients.
- Preserve capability negotiation: clients that do not advertise lane-budget support keep old behavior.
- Record enough telemetry to prove whether a preset helped or hurt.

## Implementation Slices

### Slice 1: Preset Plumbing

- Add preset enum and parser.
- Replace hard-coded lane budget constants with table lookup.
- Keep current values as `light`.
- Add debug print command.
- No behavior change when preset is `light`.

### Slice 2: Auto Startup Selection

- Inspect game mode, loaded files, and basic map actor counts.
- Pick initial preset for `auto`.
- Log the selected preset and reason.

### Slice 3: Runtime Telemetry

- Expose per-lane clamps, actor deferred candidates, active replicated actors, and update-age metrics.
- Add one console dump command and optional periodic trace.

### Slice 4: Adaptive Autoscale

- Promote/demote between light/medium/heavy using hysteresis.
- Start with actor-delta pressure only.
- Do not autoscale command/player-snapshot lanes.

### Slice 5: Interest-Aware Shared Actor Delta

- Extend interest scoring to co-op monsters, pickups, map actors, and scripted actors.
- Fill the actor-delta lane by priority plus keep-alive.
- Preserve baseline repair behavior.

### Slice 6: Compat Metadata

- Add HCDE-owned compat profiles for known heavy mods.
- Let profiles recommend preset and actor priority hints.
- Keep third-party archives untouched.

## Test Plan

Use the same map and player count across these cases:

- Doom 2 vanilla co-op, light/auto.
- Doom 2 with many monsters spawned, medium/auto.
- Brutal Doom v22 test build, heavy/auto.
- Monsters and Addons, heavy/auto.
- Deathmatch or duel, light/auto.

Measure:

- player movement feel
- intermission behavior
- gun/use latency
- actor-delta clamp count
- deferred actor count
- average nearby monster update age
- packet size and bandwidth
- server frame time

Success criteria:

- Doom 2 remains as stable as today's improved baseline.
- `auto` picks light for light sessions and medium/heavy for heavy mod stacks.
- Heavy mods show fewer deferred actor updates and less visible monster/projectile lag.
- Competitive modes remain lean by default.

## Open Questions

- What exact packet-size ceiling should heavy mode respect to avoid fragmentation on typical home networks?
- Should Doom Connector expose the selected preset in the room/game UI?
- Should public servers advertise preset and expected bandwidth in server info?
- Should compat profiles live in engine metadata, Doom Connector engine profiles, or both?
- Do we want per-client adaptation later for mixed LAN/WAN players, or keep one server preset for simplicity?
