# Gameplay foundation implementation and combined audit

Date: 2026-09-26. Scope: the gameplay-foundation phase of the newly discussed
eight-phase completion plan, plus all local combat, invasion, native timer and
server/network changes from the preceding work. This is separate from the old
`HCDE_CSHARP_PHASE1_AUDIT.md`, which covers tools and protocol migration.

The subsequent [combat/AI audit](HCDE_CSHARP_COMBAT_AI_AUDIT.md) supersedes the
validation total, records version 3 pose archives and resolves the two CA2014
warnings listed below. This document preserves the foundation pass's results.

## Assessment

The managed flat-sector foundation is implemented and regression-tested. This
is **not full native physics or engine parity**, and is not a release approval.
The earlier invasion fixes remain validated in managed loopback sessions and
through native policy compilation; the user's native multiplayer symptom still
needs a current-checkout native build and runtime reproduction.

This was a self-audit of the combined working tree, not an independent review.
The work remains local and uncommitted.

## Implemented foundation

- Fixed-point stored XYZ position and momentum; bounded horizontal movement,
  ground friction, gravity, jumping, floor landing and ceiling clipping.
- Swept horizontal cylinder collision, wall sliding, solid actor obstruction
  with vertical separation, flat-sector lookup, 24-unit steps and headroom checks.
  Moving floors carry grounded actors. Geometry is scanned directly rather than
  trusting malformed BLOCKMAP entries. This favors correctness over large-map speed.
- Timed actor frames, entry actions, immediate zero-tic chains, action redirects,
  hold/removal states, and bounded detection of immediate state cycles.
- Shared damage handling with armor, invulnerability/bypass flags, pain, source
  tracking and a single death transition. Corpses stop blocking or taking damage.
  Generic pain/death frames are infrastructure, not native monster animations.
- Horizontal attacks now also check actor Z and the opening at shot height;
  shootability is independent of solidity.
- Destroyed thinkers are unlinked; fresh destroyed actors do not receive startup
  callbacks; changing thinker stats cannot tick an actor twice in the same pass.
  Dynamically assigned actor IDs are never reused after removal.
- Version 2 pose archives retain Z, velocity, state timer and grounded status.
  Version 1 archives remain readable. Restore does not rerun state entry/death
  actions, and pending player commands are cleared. This remains a pose archive.
- Player snapshots carry Z, momentum and actual grounded status through the wire;
  actor snapshots carry Z. BT_JUMP reaches the authoritative simulation.

## Findings fixed during audit

| Finding | Resolution / evidence |
| --- | --- |
| Endpoint-only actor collision could miss intervening obstacles | Subdivided swept collision; fast wall slide and small-radius actor tests |
| Existing doors raised floors | Doors now move ceilings toward neighboring ceiling height minus four, close to floor height, and reopen if a solid actor blocks closing |
| Existing special-crossing test room was only 12 units tall for a 56-unit player | Corrected room fixture to 128 units; separate ceiling-door regression retains the original timing check |
| Destroyed thinkers accumulated; actor IDs could be reused | Unlink cleanup and monotonic allocation, tested with state-triggered removal |
| State actions can change thinker lists | Snapshot each stat pass and tick each thinker at most once; fresh-child behavior remains tested |
| Save counts were trusted before allocation; physics would be lost on restore | Bounded record counts and overflow-safe lengths; v2 replay reproduces the next simulation checksum |
| Snapshot publisher reported every player at Z=0 and on the floor | End-to-end network jump verifies player and actor Z, vertical velocity and grounded status |
| Removed actors remained live in the publication store | Retained non-live, zero-health records correct subsequent full snapshots; regression verifies a later spawn has a distinct ID |
| Co-op retained the small tail buffer and per-recipient event draining | Both co-op and invasion build/cache one full tail per tic, with checksum generation after rolling-hash updates; two recipients with 40 actors receive matching events and checksums |
| Input record count could disagree with the header and partially mutate state | Validate the entire admitted player list before acknowledgements or commands are changed |

## Audit of preceding combat/invasion work

Reviewed the changed combat tracer, damage path, director, CLI options, phase-to-wire
mapping, query publication, shared-socket command dispatch, input sequencing,
snapshot freshness handling, world-state publication, per-tic tail caching,
35 Hz scheduler and native countdown/count policy.

The retained tests cover wall/flag occlusion, melee range/direction, corpse
handling, current-wave membership, countdown/intermission boundaries, finite-wave
victory, independent client command sequences, duplicate suppression, stale
simulation tics in fresh packets, accepted counter corrections and late-join
countdown state. A real UDP pregame-to-live managed session kills an invasion
enemy with three commands and checks the replicated health/count/victory together.

Native reference points reviewed include `src/d_net.cpp`, `src/d_event.h`,
`src/doomdef.h`, `src/playsim/p_mobj.cpp` and the MapSpot definition in
`wadsrc/static/mapinfo/common.txt`. Native constants and selected behavior informed
the port; this is not an instruction-for-instruction translation.

## Remaining findings and phase boundaries

| Priority | Remaining work | Completion area |
| --- | --- | --- |
| High | Flat-sector solver does not cover slopes, portals, 3D floors, actor stacking, full crushing/drop-off rules or native corner/slide compatibility; doubles are used for geometric calculations, so deterministic same-runtime tests do not establish cross-platform/native parity | Advanced geometry and parity work; foundation remains a subset |
| High | State tables are generic and not bound to all native classes, DEHACKED frames or script actions; monster AI, projectiles, weapon timing and damage types remain incomplete | Combat/AI and scripting phases |
| High | Full snapshots still have the protocol's 255-record and transport-size limits; overflow produces an explicit exception. Retained dead records also consume that budget. No packet paging, event replay after loss, or complete late-join baseline | Multiplayer phase; blocks large-map release |
| High | The simulation accepts one pending command per player per tick, so multiple admitted commands in one socket drain can overwrite one another; reliable command buffering and loss/reorder recovery need end-to-end work | Multiplayer phase |
| High | Save/rewind does not restore dynamic actor membership, inventory, director, running ACS or mover state; restoring a pose cannot undo those side effects | Persistence phase |
| High | Native classic invasion spawners, budgets, spawn placement, participant wipes/retries and AI remain absent from the managed subset | Combat, maps and multiplayer phases |
| High | Current native `d_net.cpp` has not been built or exercised in a native host/client match. Generated VS projects point to another checkout; no local native runtime/IWAD fixture was available | Native validation before declaring the reported desync resolved |
| Medium | Line-special numbers/activation and moving-plane behavior remain the existing simplified subset, despite correcting ceiling doors; full Doom/Hexen/UDMF translation and crush rules are pending | Maps/mods and scripting phases |
| Medium | Full geometry scans and state-table dispatch have not been profiled on large maps; world snapshot class IDs and actor animations still need native-compatible spawn/presentation integration | Performance, client and parity phases |

## Validation

- Before this foundation work: 776 managed tests passed.
- Final combined managed suite: **802 passed, zero failures/skips** across eleven
  test projects. Command from repository root:
  `dotnet test csharp/HCDE.sln --verbosity quiet`.
- New foundation tests cover momentum, jump/rejump, gravity, ceiling impact,
  no-gravity actors, sweeps, steps/headroom/drop-offs, moving floors, state timing,
  action redirects/cycles, thinker removal/stat changes, armor/death, shot height,
  save bounds, airborne restore/replay and matching command-stream checksums.
- Native policy: MSVC C++17 `/W4 /WX /c` compiles
  `tests/invasion_stage9/invasion_policy_tests.cpp` successfully. This checks pure
  countdown and pending-wave rules, not the whole native translation unit.
- `git diff --check` reports no whitespace errors.
- SDK 10.0.201, net8.0 test runtime. The `csharp/global.json` pin is unchanged;
  commands were run from the repository root. Existing CA2014 warnings remain in
  `DemEventStreamConverter.cs` and `SnapshotChecksumPlaysimInputs.cs` on rebuild.

Next release-level verification must include representative real maps and native
host/client multi-wave play with late joins, a wipe/retry and packet loss. Managed
unit and loopback results alone cannot certify the entire conversion.
