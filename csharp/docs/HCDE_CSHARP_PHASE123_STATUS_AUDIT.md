# Gameplay phases 1–3: implementation and completion audit

Date: 2026-09-26. Baseline: `55f8fa46` (923 tests), including the preceding
`0a37c839` gameplay, maps/mods and invasion work. This report covers this working
tree's changes and reruns the full managed regression suite against that baseline.

**Result: phases 1–3 remain incomplete.** Here phase 1 means gameplay foundation,
phase 2 means combat/AI, and phase 3 means maps/mods. The older numbered audits
describe smaller protocol/server milestones; their "complete" labels do not
certify these gameplay phases or the full C++ conversion.

## Implemented and audited in this pass

### Snapshot failure handling

Previously collecting a tail drained queued authority events and dead-spawn
indices, and advanced actor rolling hashes, before discovering that encoding
failed. A retry therefore represented different state from the original attempt.

The tail builder now peeks at pending records and commits them only after a
successful encoding preflight. Checksum-bearing builders reserve the checksum
block during preflight and compute/store hashes only after success. Collection
itself no longer mutates actor hashes. Direct co-op writers follow the same
commit-on-success rule. Tests compare retry bytes with an untouched fresh store
for co-op, checksum-bearing co-op and merged invasion tails.

World-delta player/sector counts and shipping-tail counts are rejected before
narrowing to a byte. Previously 256 sectors could wrap their count to zero.
Invalid invasion header versions are rejected by the writer. Canonical input
event payloads over 65,535 bytes are rejected before their length can wrap.

This is serialization failure safety, **not delivery acknowledgement**. Pending
records are still consumed when a packet is built; this does not recover UDP
loss. The single-tail count/packet limits and accumulating actor tombstones still
block large maps and sustained projectile sessions. LiveSession still throws
when its world cannot fit. Nothing is silently truncated to make that disappear.
The checksum-bearing path encodes twice; its performance has not been profiled.

### Player state and buffered weapon selection

Armor and signed aiming pitch now travel from the authoritative player through
the existing pose wire fields into the guest store. Both co-op and invasion
round trips are tested, including positive/negative pitch near the clamp limits.
Health and armor beyond the signed-short protocol range saturate rather than
becoming negative. Simulation values retain their original precision/range;
exact large mod health would require an extended negotiated wire format.

DEM_WEAPSELECT canonical events now enter the same FIFO as movement and attack.
Selection happens on the command's simulation tic, before firing. Default Doom
slots, repeated slot cycling, next/previous, ownership and primary-ammo checks
are implemented. Multiple selections in one tic retain their order. Queue
rejection makes no inventory change; retry admits selection once space opens.
Input bytes are copied into the queued selection list. Death/restore discard
these actions with the existing command queue. Switching cannot reset cooldown.

Malformed framing or selection payload lengths reject the entire command before
movement or attack is admitted. Empty direct-call blocks remain supported.
Other framed event types remain ignored by this sink. Custom slots, alternate
fire, no-ammo-check options, pending-weapon/psprite lowering and raising, and
other gameplay event commands are still unimplemented.

Native references: `src/d_net.cpp` SelectWeapon, `src/g_game.h` WST enum,
`wadsrc/static/zscript/actors/doom/doomplayer.zs`, and player.zs PickWeapon /
PickNextWeapon / PickPrevWeapon. The default slot selection order matches those
tables; the whole native weapon-switch lifecycle does not.

### Weapon noise and map sound flags

Successful weapon firing alerts eligible existing monsters through connected
flat sectors. Closed openings stop propagation using current moving floor and
ceiling heights. A path may cross one ML_SOUNDBLOCK line, but not two. A lower-cost
alternate path revisits a sector so the result is independent of discovery order.
Idle monsters retain the heard player's position for chase without seeing through
walls. Ambush monsters require line of sight to accept the alert; dead/disabled
monsters and dry firing do not create a new target.

Doom and Hexen binary ambush flags and UDMF `ambush` are retained at spawn.
Binary line sound flags and UDMF `blocksound` feed propagation. Ambush participates
in the simulation checksum. Regression coverage includes all three loading
formats, two sound barriers, cheaper alternate paths, opening/closing doors,
hidden monsters and ambush behavior.

Reference: `src/playsim/p_enemy.cpp` P_RecursiveSound / P_NoiseAlert and
`src/doomdata.h` ML_SOUNDBLOCK. This is an immediate flat-sector alert subset:
native persistent sector sound targets, later-spawned listeners, splash/no-target
options, portal/sloped sound traversal and full native A_Look behavior remain.
The adjacency graph is rebuilt per shot and has not been performance-profiled.

## Combined completion gates, including previous work

### Additional conversion: fractional planes, ceilings, crushers and stairs

Live sector planes now retain fractional heights. UDMF fractional flat-sector
heights are accepted after finite/range validation; collision, player floor
placement, traces, projectiles, sound openings and snapshot publishing consume
the live values. Door/floor/lift argument speeds use division by 8 without integer
truncation. Zero/negative speed requests are rejected rather than coerced to an
unrequested minimum speed. Plane checksums retain fractional bits.

Pose archive version 5 writes sector planes as 16.16 values; readers retain
versions 1–4. A hand-built v4 sector fixture and v5 fractional round trip verify
compatibility. Invalid plane restores are rejected before changing clock or actor
state. This extends plane storage, not the archive's incomplete mover/AI/inventory
coverage. Physics and saves quantize to the existing 16.16 representation.

Converted classic ceiling actions: lower-to-floor, fast/slow repeating crushers,
non-damaging lower-and-crush, stop/resume, and the corresponding supported
repeat/use/walk variants. Extended actions include lower/raise by value,
crush-and-raise, lower-and-crush, crush-stop/remove, crush-raise-and-stay,
distance crushers, lower-to-floor with gap, and raise-to-highest. Slow crushers
retain one-eighth-unit speed after obstruction; damage uses the shared four-tic
cadence. Return legs restore their configured speed. Paused movers retain
direction/speed state, and remove permits a new ceiling action.

Floor and ceiling thinkers can run independently in one sector; duplicate
thinkers on the same plane are rejected. Current opposite planes limit movement
to prevent inverted sectors. Native destination-clamped obstruction behavior is
covered: a ceiling can roll back its final step while completing that leg.
Mover checksums include pause/loop/direction, return speed and slowdown state.

Classic Doom stairs 7/8/100/127 and repeat variants 256–259 now build directed
same-texture chains with native slow/turbo speeds and 8/16-unit steps. Extended
Doom-style stairs 217/270/273 support zero delay/reset. Each chain remains locked
until all steps complete; cycle detection prevents repeated sectors. The first
turbo stair uses stopping crush behavior, while following speed-4 steps use the
native damaging-movement rule. Tests cover fractional progress, final heights,
texture/sidedness boundaries, cycles and whole-chain retrigger locking.

References: `wadsrc/static/xlat/base.txt`, `defines.i`, `src/playsim/p_lnspec.cpp`,
`mapthinkers/a_ceiling.cpp`, `a_floor.cpp` and `dsectoreffect.cpp`.
These cover the implemented flat-sector action variants. Linked sectors,
attached actors, corpse/gib handling, all compatibility flags, texture-changing
ceiling variants, movement audio, synchronized/delayed/reset stairs and remaining
specials are still work to convert. Unsupported ceiling texture-change and stair
delay/reset requests are left unactivated rather than silently dropping those
arguments. Game-specific Hexen defaults beyond the Doom actor/game model are
not certified by loading a Hexen-format map.

### Native Doom attack sequences and projectile review

The continuation adds timed attack profiles for 15 map actor types: zombie,
shotgun guy, chaingunner, Wolfenstein SS, demon/spectre, imp, baron/hell knight,
cacodemon, revenant, mancubus, arachnotron, cyberdemon and spider mastermind.
Profiles implement their attack-action delays, melee damage dice, bullet counts,
projectile classes, volleys and repeat intervals. Pain cancels a pending volley.
Previously several of these monsters had no brain and barons/cacodemons fired
imp balls. Demon melee cannot fall through to a ranged attack if its target flees.

Projectile definitions now carry native speed, radius, height and direct-damage
bases; impact damage rolls 1–8 times the base. Monster spawn heights and initial
normalized source-to-target direction follow P_SpawnMissileXYZ. Revenant tracers
retain their target and steer on four-tic boundaries with bounded yaw and vertical
adjustment. Cyberdemons and spider masterminds ignore radius damage while remaining
vulnerable to direct hits. New properties participate in the simulation checksum.
Managed projectile identities stay unique within the existing ushort range.

The review also fixes BFG spray yaw: it follows the projectile angle even if its
owner turns. Its forty-ray fan uses 90/40 spacing and fifteen 1–8 damage dice per
hit, replacing the uniform damage approximation. The original geometry regression
for plasma now checks native damage bounds instead of the previous fixed 20.

Thirty-one new regressions cover native attack delays, repeat/volley intervals,
mancubus spread, projectile defaults and identities, tracer timing/dead targets,
boss impact damage without splash, pain interruption, demon range escape, hitscan
pellet counts, elevated aim, deterministic replay and BFG owner-turn behavior.
Sources: Doom actor definitions under `wadsrc/static/zscript/actors/doom`,
`src/playsim/p_mobj.cpp` and `src/playsim/p_actionfunctions.cpp`.

This is not complete native AI/state execution. Acquisition/chase, attack-choice
probabilities, per-action RNG streams, refire friendly-fire/visibility rules,
floating movement, native animation states/pain durations and difficulty modifiers
remain open. The following continuations add lost-soul, pain-elemental and
arch-vile behavior. Tracer state-entry cadence, spawn collision/initial displacement,
projectile lifetime/species rules, BFG explosion delay and vertical autoaim still
require conversion. These limitations prevent claiming native behavioral parity.

### Lost souls, pain elementals and invasion descendants

Lost souls now start a charge after the native ten-tic attack windup, preserve
20-unit horizontal speed without ground friction, aim vertically, and inflict
3 times a 1–8 roll on impact. Movement checks in two-unit or smaller increments
prevent crossing actors/walls during a charge, including vertically overlapping
targets. Impact, pain and death end the charge. Source review corrected plane
contact: floor contact clears downward velocity while preserving the charge;
ceiling contact reverses vertical velocity. Charge state is checksummed.

Pain elementals spawn a charging lost soul after their fifteen-tic windup and
three souls at offsets 90/180/270 after 32 death tics. Spawn checks use the native
prestep distance and subdivided path, temporarily ignoring the parent's solidity
and restoring it in a finally block. Low ceilings and blocked paths reject the
child. IDs are monotonic; successful children enter the actor and thinker lists.
Death timer/target state is checksummed and the death burst fires once per death.

The managed invasion director registers descendants only for members of the
current wave. It waits for a pending pain-elemental death burst before ending an
empty wave, then counts successful child spawns. A completely blocked burst still
allows completion after the death action runs. This closes a new managed wave
transition race; it does not certify native invasion runtime parity. Default
wave generation still spawns bots, so the regression installs a pain-elemental
brain on a tracked wave actor to exercise this integration.

Fourteen added regressions cover charging, impact bounds/single hits, walls,
vertical hits, pain/death, floor/ceiling response, spawn delay and path rejection,
death burst timing/uniqueness, deterministic replay, child counts and blocked-burst
victory. The audit used `lostsoul.zs`, `painelemental.zs`, native `AActor::Slam`,
`P_XYMovement`/`P_ZMovement` and actor collision handling.

Remaining limitations include native float/chase/retarget logic, animation states,
compatibility soul limits, friendliness/species and damage-type rules, runtime
actor replacements/DEHACKED defaults for spawned souls, and advanced geometry.
Rejected soul spawns are omitted rather than producing the native killed spawn
actor/effects. The v5 pose archive still does not restore these AI timers or
dynamic actor membership. These are implementation gaps, not passed audit gates.

### Arch-vile attack and resurrection

The arch-vile now acquires a target through the managed brain, starts its fire
stage after ten attack tics and attacks at tic 66, recovering through tic 94.
The attack checks sight again, applies 20 direct damage and a 70-radius blast
behind the target, then sets vertical velocity to 1000 divided by target mass.
Native Doom mass defaults are initialized for map monsters and spawned souls.
Mass and resurrection metadata participate in the simulation checksum.

The resurrection scan checks nearby terminal corpses with native raise states,
clearance against floors/ceilings/walls and occupied actor space. It restores the
spawn health, clears prior retaliation/charge/death state, shares the arch-vile's
target, and holds the revived brain for the type's raise duration. The arch-vile
heals for 30 tics. Review corrected the zombie raise duration to 20 tics and made
pain interrupt healing/raising instead of waiting behind those timers.
Map actor resurrection health preserves the applied spawn health; managed wave
bots retain their own 30-health default. A resurrected wave member keeps its ID
and returns to the live enemy count without incrementing total spawns.

Eighteen regressions cover attack timing/damage/thrust, loss of sight, four raise
durations, four non-raiseable types, actor obstruction, ceiling clearance, healing
interruption, target mass, actual resurrection of a tracked wave bot, and replay.
References: `archvile.zs`, Doom actor Raise blocks, and
`P_CheckForResurrection` in `src/playsim/p_enemy.cpp`.

This completes attack dispatch coverage for the 18 cataloged Doom monster map
types, not full native monster/state parity. The fire stage is currently simulation
state, not a replicated visible fire actor. Native death/raise animation tables,
exact blockmap/chase resurrection selection, friendliness/no-target/damage flags,
modified mass/raise actions, corpse restoration after crushing, ghost compatibility,
blast/autoaim edge semantics and full save restoration remain open. The managed
terminal-corpse state is still reached on its generic death timeline. A source
review and synthetic regressions cannot certify the native gameplay experience.

### Floating movement and persistent hearing

Cacodemons, pain elementals and lost souls now have a separate floating flag,
initialized from the native actor defaults rather than inferred from no-gravity.
After ordinary movement, a live enabled floater adjusts its base height toward
the target's center when horizontal distance is strictly less than three times
the vertical difference. Default float speed is four units per tic. The adjustment
is a position change, preserves vertical velocity, respects current sector planes,
and is bypassed by the dedicated lost-soul charge path. Float state/speed are
included in the checksum. Eight regressions cover all three species, rising,
descending, strict range boundaries, ceiling clipping, no-gravity separation and
dead/disabled actors. Sources: `P_ZMovement` and Actor's `FloatSpeed 4` default.

Weapon noise now records a persistent `LastHeardTargetId` on reached actors,
including those unable to wake at that moment. An idle brain can later acquire a
living remembered source; ambush monsters still require sight. A newer sound is
remembered while the current live target is retained. Five regressions cover
disabled listeners, dead sources, ambush behavior, newly spawned monsters, and
switching to the latest heard source after the previous target dies.

Source review distinguished native per-actor LastHeard from the optional sector
SoundTarget compatibility mode (`NoiseMarkSector`/`A_Look` in `p_enemy.cpp`). The
implementation follows the per-actor default: newly spawned actors do not inherit
old sector noise. Last-heard identity is checksummed. The sector compatibility
mode, splash/range-limited alerts, friendliness/no-target rules, full native look
state cadence, obstacle-directed float adjustment, 3D geometry/actor stacking and
save persistence remain open. This supersedes the earlier immediate-alert-only
limitation without claiming those additional behaviors are converted.

| Phase | Existing verified foundation | Work still preventing completion |
| --- | --- | --- |
| 1 — gameplay | XYZ movement, gravity/jump, flat-sector collision, timed states, damage/death, armor, buffered commands; current regressions pass | Native collision/dropoff/stacking/corner semantics; advanced geometry; complete actor state/action execution; player lifecycle/roster integration and remaining input commands |
| 2 — combat/AI | Managed weapon cadence, pitch-aware traces, pellets, projectile definitions, 18 monster attack profiles, lost-soul charges, pain-elemental spawns, arch-vile attacks/resurrection, target-height floating, default slots and persistent per-actor hearing | Complete native state/action tables and boss death actions, arch-vile visual fire/remaining resurrection semantics, remaining soul/spawn semantics, obstacle float/path logic, sound compatibility/advanced alert rules, exact RNG/autoaim/refire/psprites/BFG and vertical pellet spread |
| 3 — maps/mods | Checked WAD/PK3 loading, Doom/Hexen/UDMF geometry and spawn flags, fractional planes, ordered overlays, basic DEHACKED, selected doors/floors/lifts/ceilings/crushers/stairs/teleport/exit specials | Remaining line/sector specials and advanced mover variants, lights/switch textures, full activation-side/monster/projectile rules, slopes/portals/3D floors, MAPINFO progression, resource namespaces/client consumption and complete DEHACKED/BEX/actor-definition execution |
| Shared release blocker | Failed snapshot builds preserve source state | Paging or a negotiated replacement snapshot protocol, atomic receive/apply, bounded tombstones, loss/reorder/late-join coverage; current 255-record / packet-size limits remain |
| Cross-phase validation | Full managed suite and native invasion policy compile | Running this native checkout with representative IWAD/maps/mods and comparing behavior; native class/resource negotiation and original invasion round/timer/enemy symptom still need real runtime confirmation |
| Later persistence phase | Backward-compatible v5 pose archive with fractional sector planes | Full actor membership/inventory/AI/projectiles/movers/ACS/invasion saves; pose archives are not complete savegames |

Earlier detailed reviews remain relevant: [foundation](HCDE_CSHARP_GAMEPLAY_FOUNDATION_AUDIT.md),
[combat/AI](HCDE_CSHARP_COMBAT_AI_AUDIT.md), [maps/mods](HCDE_CSHARP_MAPS_MODS_AUDIT.md),
[phase 2/3](HCDE_CSHARP_PHASE23_COMPLETION_AUDIT.md),
[movers/damage](HCDE_CSHARP_MOVERS_DAMAGE_AUDIT.md), and
[command buffering](HCDE_CSHARP_INPUT_BUFFER_AUDIT.md). New default weapon-selection
and immediate sound-alert coverage supersedes only those specific open items.

## Validation

- Release solution: **1,065 passed, zero failed/skipped**, 142 cases above baseline.
  Protocol 15; Gamedata 10; Transport 10; Master 1; RCON 6; MapLoader 153;
  Playsim 276; Client 12; Net.Core 351; Pregame 83; Server 48.
- `dotnet build csharp/HCDE.sln -c Release --no-restore --verbosity quiet -warnaserror`:
  zero warnings/errors. Tests/build run from the repository root using SDK
  10.0.201 targeting net8.0; the nested SDK pin is unchanged.
- Existing invasion multi-client, stale-tic, timer/wave/enemy, input retry,
  physics, combat, map/mod and save regressions remain passing.
- `tests/invasion_stage9/invasion_policy_tests.cpp` compiles with MSVC C++17,
  `/W4 /WX`; compile-time policy assertions pass. This is not a full engine build.
- `git diff --check`: no whitespace errors. No dependencies or native sources
  changed in this pass. Prior committed work is retained.

This is a source review and regression self-audit, not an independent review or
an exhaustive native parity audit. Passing this suite does not close the gates
above. Full completion requested by the user has not been achieved.
