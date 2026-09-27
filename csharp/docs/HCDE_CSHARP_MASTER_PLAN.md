# HCDE C# conversion master plan

Updated: 2026-09-27.

## Objective and current status

Convert HCDE's native C++ behavior into a working C# implementation, preserving
gameplay and multiplayer behavior and validating it against representative native
maps and sessions. Fix the reported invasion round-count, timer and enemy-count
synchronization problems as part of that validation.

**The conversion is not complete. Gameplay phases 1–3 remain open.** Passing
managed tests establishes the behavior covered by those tests, not complete
native parity. Earlier documents use phase numbers for smaller protocol/server
milestones; this plan uses phase 1 for gameplay, phase 2 for combat/AI, and phase
3 for maps/mods. Later client, rendering, audio and persistence work is separate.

The detailed evidence and historical findings are in the
[phase 1–3 status audit](HCDE_CSHARP_PHASE123_STATUS_AUDIT.md).
This document is the execution plan; that audit remains the detailed record.

## Verified checkpoint

- Release solution: **3,050 passed; zero failed or skipped**.
- Release build with warnings as errors: zero warnings/errors.
- Tests by project: Playsim 1,991; MapLoader 396; Net.Core 351; Pregame 97;
  Server 61; Protocol 15; Client 12; Gamedata 10; Transport 10; RCON 6; Master 1.
- Historical baseline: 923 tests at `55f8fa46`; 2,127 additional cases since then.
- Earlier checkpoints: `0a37c839` gameplay/maps/invasion foundation,
  `55f8fa46` buffered authoritative input, `d792c54a` AI/sector/snapshot work.
- Native invasion policy compilation was previously verified; a complete native
  engine build and representative in-game comparison have not been established.
- These are source reviews and regression self-audits, not an independent or
  exhaustive native parity audit.

## What has been implemented

| Area | Existing implementation and regression coverage | Status |
| --- | --- | --- |
| Gameplay | XYZ movement, flat-sector collision, gravity/jump, timed states, damage/death, armor, pitch, buffered commands and weapon selection | Implemented subset |
| Combat/AI | Weapon cadence and damage/spread work, pitch-aware traces, projectiles, monster attack profiles, lost-soul/pain-elemental behavior, arch-vile attacks/resurrection, hearing and target-height floating | Implemented subset |
| Map loading | WAD/PK3 foundation, Doom/Hexen/UDMF geometry, spawn filters, fractional planes, overlays, basic DEHACKED, sector damage and sidedef metadata | Implemented subset |
| Map effects | Selected doors, floors, lifts, ceilings, crushers, stair variants, stop/resume behavior, texture changes, lighting effects, sector damage/healing and restricted exits | Implemented subset |
| ACS execution | Variables/arrays, expressions, fixed arithmetic, control flow, waits/delays, nested execution and trigger context, selected specials, actor/sector queries, pitch/yaw, trigonometry, game settings and timer | Word-format subset |
| ACS packed operands | Byte pushes, byte-immediate specials/delays, exact operand lengths, unaligned branches and aligned sorted tables | Implemented and audited |
| ACS metadata/addressing | Local/map array metadata codecs, JUMP decoding, bounded relocation, GotoStack, copied program data and module-relative branch bases | Explicit APIs; automatic map integration unfinished |
| Networking | Input buffering, selected player/invasion replication, snapshot build failure safety, handshake retries/replays and StartGame handling | Implemented subset; scale/loss blockers remain |
| Invasion | Managed round/timer/enemy regression coverage and selected native policy work | Original runtime symptom not yet signed off |
| Persistence | Backward-compatible v6 pose archive with fractional sector heights and actor pitch | Pose archive, not a full savegame |

Recent audits also corrected malformed ACS directory-count overflow, discarded
sidedef offsets, clock observation one tic ahead of native ACS, and alignment
errors exposed by packed-byte instructions. Details and limitations are recorded
with each change in the status audit.

## Remaining work by phase

### Phase 1 — gameplay foundation

- [ ] Match native collision, dropoff, stacking and corner behavior.
- [ ] Implement remaining actor states/actions and player lifecycle/roster rules.
- [ ] Complete remaining input commands and activation rules.
- [ ] Resolve health/overkill and inventory-model differences, including
  non-player armor and full actor inventory behavior.
- [ ] Validate deterministic behavior and documented compatibility choices
  against native traces, not only synthetic managed fixtures.

### Phase 2 — combat and AI

- [ ] Complete state/action tables, boss death actions and weapon-specific effects.
- [ ] Finish arch-vile visual fire/resurrection and remaining soul/spawn semantics.
- [ ] Complete obstacle floating/pathing and advanced sound-alert behavior.
- [ ] Match RNG consumption, autoaim, refire, psprites and BFG behavior.
- [ ] Exercise combat with real maps, actor definitions and multiplayer sessions.

### Phase 3 — maps, mods and scripting

- [ ] Connect BEHAVIOR loading to actual ACS program registration and startup.
- [ ] Extract validated chunk regions, bind metadata and preserve module addresses.
- [ ] Support compact opcode execution, functions/imports, cross-region/module
  execution, remaining instructions and native scheduling semantics.
- [ ] Complete line/sector specials, mover variants, switch textures, activation
  side and monster/projectile rules, lighting compatibility and native RNG.
- [ ] Implement slopes, portals, 3D floors and their collision/query interactions.
- [ ] Complete MAPINFO progression, custom skill return values, resource namespaces
  and client consumption, DEHACKED/BEX and actor-definition execution.
- [ ] Validate ACS math against the selected native math backend where exact
  rounding and deterministic results matter.

### Shared multiplayer and invasion blockers

- [ ] Replace single-packet/count limits with paging or a negotiated protocol;
  preserve all records rather than silently truncating large worlds.
- [ ] Add atomic receive/apply, bounded tombstones and loss/reorder/late-join coverage.
- [ ] Validate native class/resource negotiation and sustained projectile sessions.
- [ ] Reproduce the original invasion issue with recorded native and managed runs:
  check round transitions, timer starts/stops, spawn/death bookkeeping and
  authoritative/client enemy counts on the same ticks.
- [ ] Test join/rejoin, delayed or duplicate packets, empty waves, last-enemy deaths,
  restart and map transitions with multiple clients.
- [ ] Investigate the previously intermittent five-second
  `Pump_LiveSessionReceivesGuestClientInput` timeout if it recurs; passing reruns
  have not established its root cause.

### Work beyond gameplay phases 1–3

- [ ] Complete usable client integration, rendering, audio and input/UI paths.
- [ ] Replace pose-only archives with complete actor membership, inventory, AI,
  projectile, mover, ACS and invasion saves.
- [ ] Complete remaining native integrations/platform support and packaging.
- [ ] Run install/start/play/reconnect/save/restore acceptance on supported platforms.

## Ordered execution plan

1. **Integrate a bounded ACS map path.** Start with a supported word-format map
   fixture: load BEHAVIOR, bind program metadata, start the intended script and
   observe a real map effect. Reject unsupported formats/features explicitly.
   Then extend compact execution and module/function support with fixtures.
   Gate: a loaded map executes without manually constructed AcsProgram objects.
2. **Establish native comparison fixtures in parallel with conversion work.**
   Identify available IWAD/maps and a reproducible native build; record exact
   versions, commands and traces. Obtain missing assets from the user when needed.
   Gate: repeatable native and managed runs with comparable tick-level evidence.
3. **Close networking capacity and delivery gaps.** Specify limits/protocol,
   implement atomic transfer and test adverse delivery plus large worlds.
   Gate: no silent state loss, count wrap, partial apply or unbounded tombstones.
4. **Close the original invasion report.** Use the comparison fixtures and network
   harness to reproduce/fix divergence and demonstrate synchronized rounds,
   timers and enemy counts through transitions and reconnects.
   Gate: reproducible multi-client acceptance evidence, not just policy compilation.
5. **Finish phase 1 parity gaps.** Prioritize lifecycle/collision issues uncovered
   by real maps; audit every behavior against a named native source path.
6. **Finish phase 2 parity gaps.** Complete combat/AI tables and effects, then
   compare deterministic combat scenarios and sustained multiplayer sessions.
7. **Finish remaining phase 3 coverage.** Complete specials, advanced geometry,
   mod/resource rules and progression; maintain a supported/unsupported matrix.
8. **Run the phase 1–3 acceptance audit.** Resolve remaining findings and only
   mark phases complete when every applicable gate below has evidence.
9. **Proceed to full-product completion.** Finish client, media, persistence and
   packaging; phase 1–3 sign-off alone does not mean the entire C++ port is done.

These are work packages, not a fixed estimate of sessions or a promise that a
test-count milestone completes a phase. New native discrepancies belong in the
relevant package and audit before closure.

## Completion and audit rules

- Each change names the native source behavior and its supported scope.
- Add meaningful regression cases for behavior, boundaries, malformed data and
  integration. Check existing callers, checksums and persistence/replication impact.
- Run focused tests during development, then the full suite and required build.
- Record limitations and actual failures; do not relabel a subset as complete.
- Phase sign-off requires loaded-map/runtime evidence, resolved material findings,
  passing multiplayer/invasion acceptance and an explicit remaining-work review.
- Keep this plan and the status audit current; commit at reviewable checkpoints.

Run from the repository root (the nested SDK pin is unchanged):

```powershell
dotnet test csharp/HCDE.sln -c Release --no-restore --verbosity quiet
dotnet build csharp/HCDE.sln -c Release --no-restore --verbosity quiet -warnaserror
git diff --check
```

The verified environment uses SDK 10.0.201 targeting net8.0. On a fresh machine,
restore dependencies before using `--no-restore`. A passing managed suite does
not substitute for native engine, map/mod or multi-client runtime acceptance.
