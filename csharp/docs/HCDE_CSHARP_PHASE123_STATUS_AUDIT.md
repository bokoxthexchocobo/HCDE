# Gameplay phases 1–3: implementation and completion audit

Updated: 2026-09-27. Cumulative baseline: `55f8fa46` (923 tests), including the
preceding `0a37c839` gameplay, maps/mods and invasion work. The first 1,065 tests
and conversion work were committed as `d792c54a`; subsequent changes are included
below. Validation reruns the full managed regression suite.

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

### Player weapon damage, spread and range

Pistol/chaingun damage now rolls 5 times an integer from 1–3; fist/chainsaw damage
rolls 2 times an integer from 1–10. These replace the fixed 10-damage placeholders.
Damage is rolled before spread, following the native action order. Fist and saw
yaw spread use their distinct defaults. Gun/punch spread uses the native /256
scale, while the saw uses /255. The super shotgun now rolls independent vertical
spread for each of its twenty pellets using `7.097 / 256`, in addition to its
horizontal spread. Player hitscan range is now 8192 (PLAYERMISSILERANGE), correcting
the previous use of the 2048-unit monster range.

Eleven new regressions cover damage bounds/variation across seeds for four weapons,
vertical pellet hits, thin-target pellet misses, dry-fire RNG preservation,
deterministic replay and the player-range boundary. Geometry/melee tests that had
assumed fixed damage now check native damage bounds. Both local and UDP invasion
tests fire until the actual kill, bounded by six minimum-damage hits against the
30-health fixture, and still verify the exact wave transition, ammo use, counters
and replicated health. Five initial regression failures were fixed test assumptions
about fixed damage, not waived checks or changes to enemy health.

Sources reviewed: `weaponpistol.zs`, `weaponshotgun.zs`, `weaponssg.zs`,
`weaponfist.zs`, `weaponchainsaw.zs`, Actor's melee constants and
`constants.zs`/`p_local.h`. This does not replace the managed RNG with native
per-action random streams. Native psprite startup/refire timing and first-shot
accuracy, autoaim, berserk/saw-specific effects and optional vertical-spread cvars
remain unconverted. No claim of complete player-weapon parity is made.

### Immediate sector lighting actions

Runtime sector light levels are now separate from immutable map definitions,
included in simulation checksums and read by the snapshot publisher. Converted
extended actions are 110/111/112 (raise/lower/change) and 233/234 (minimum/maximum
neighbor). Doom translations cover 12/13/35/79/80/81/104/138/139/157/169/170/171/
173/192/194 with their walk/use and repeat rules. Tag zero uses untagged sectors;
an unmatched tag succeeds and consumes a one-shot line, as native immediate light
actions do. Minimum includes the current sector; maximum searches neighbors and
can lower brightness. An isolated maximum resolves to -1.

The source audit corrected an initial 0–255 clamp: this checkout's SetLightLevel
clamps to signed short limits. Negative and overbright levels therefore survive
the action and snapshot encoding. The snapshot tests explicitly enable sector
metadata, matching DedicatedServerHost's default; raw tail builders omit that
metadata unless requested. Twenty playsim cases and two snapshot cases cover
activation/repeat rules, tags, neighbor selection, signed bounds, checksums and
wire values. References: `xlat/base.txt`, `p_lnspec.cpp`, `a_lights.cpp` and
`sector_t::ClampLight` in `r_defs.h`.

Remaining sector lighting thinkers, exact native lighting RNG, automatic
map compatibility selection, remaining ACS dispatch, switch textures and
full save persistence remain open. In particular, v5 pose archives do not capture
dynamic light levels or lighting thinkers.

### Animated sector lighting actions

Extended map-line actions 113/114/116/117 now implement fade, glow, configurable
strobe and stop. Reviewed against DGlow2, DStrobe and their EV entry points in
`src/playsim/mapthinkers/a_lights.cpp`: the first fade tick writes its start value;
the endpoint is reached before the next tick writes it again and destroys the thinker. Glows sort
their bounds and reverse without an extra endpoint pause. Strobes inspect the
current light, start after one tick and retain separate bright/dark durations.
Effects can overlap in creation order; immediate fades still apply. Stop removes
only matching tagged effects and preserves their current brightness.

Eighteen new playsim cases exercise those timelines, descending integer rounding,
nonpositive durations, signed bounds, equal targets, missing tags, overlapping effects,
stop/restart and pending-effect checksums. One server test verifies successive
authoritative fade levels through the snapshot encoder/decoder. Wider arithmetic
prevents interpolation overflow for extreme extended arguments; native signed
integer overflow behavior is not emulated. Lighting advances before actor thinkers;
cross-thinker ordering still needs a native runtime comparison. These tests use
synthetic maps and source-derived expectations, not captured native execution.

### Doom-style triggered strobes

Extended action 232 and Doom translations 17/156/172/193 now derive the upper
level from the current sector and the lower level from its current neighbors.
When the minimum equals the upper level, the lower level becomes zero. The
native walk/use and one-shot/repeat mappings are retained. Startup delays are
1–8 ticks; extended arguments control bright/dark durations, while Doom
translations use 5/35. Unmatched tags neither create an effect nor
consume a random draw. Stop also removes these strobes.

Eleven regressions cover mappings, cadence, runtime neighbor values, signed
levels, equal/higher-neighbor fallback, missing tags, stop/restart,
deterministic replay and combat RNG isolation. Source references are DStrobe's
Doom constructor and EV_StartLightStrobing in `a_lights.cpp`, action 232 in
`p_lnspec.cpp`, and `wadsrc/static/xlat/base.txt`.

The dedicated managed strobe stream participates in the checksum and does not
consume combat RNG. Its LCG sequence is not native FCRandom/StrobeFlash sequence
parity. As with lighting thinkers and current light levels, this stream is not
stored in the v5 pose archive. Full lighting save/restore remains incomplete.

### Flicker and correction to overlapping-effect behavior

Extended action 115 now implements DFlicker's immediate upper-level assignment,
independent signed clamping and countdown behavior. Its initial countdown is 1
or 65; transition occurs one tick later. Subsequent dark and bright intervals
are 2–9 and 2–33 ticks respectively. Bounds are not sorted. Ten new regressions
cover these ranges, retriggers, overlapping glow, stop-all behavior, deterministic
replay and isolation from both combat and strobe RNG. Flicker has a dedicated
checksummed managed RNG stream; exact native FCRandom sequence parity and saving
that stream remain open.

The deeper source audit found an error in the preceding lighting passes: checking
`sector.lightingdata` was incorrectly interpreted as these thinkers acquiring a
lock. DLighting has no such constructor, DSectorEffect::Construct only assigns
its sector pointer, and CreateThinker only constructs/links the thinker. The
assignment is in FraggleScript's DLightLevel, not these map-line effects. Removed
the incorrect managed single-effect guard and corrected three earlier tests.
An immediate fade can be overwritten by an older fade's final write; overlapping
effects run in creation order; Stop removes every matching effect. Earlier audit
claims about busy-sector rejection and fade completion locks are superseded.
The separate FraggleScript lighting lock is still unimplemented.

### Automatic sector glow and strobes

Map startup now creates glow/strobe thinkers for Doom sector specials 2/3/4/8/
12/13 and extended specials 66/67/68/72/76/77. Doom's low five special bits select
the effect without confusing higher generalized sector flags with its number.
Extended maps use extended numbers. Neighbor minima are captured on creation.
Ordinary strobes start after 1–8 ticks; synchronized strobes start on tick one.
Bright intervals are five ticks, with fast/slow dark intervals of 15/35 ticks.
DGlow moves eight light units per tick, undoing the step and reversing direction
at either bound, so endpoints are not reached in the usual non-degenerate case.

Eighteen regressions cover automatic startup, format mappings, generalized Doom
bits, fast/slow and synchronized timing, glow reversal, narrow light ranges and
stopping initialized effects. Source review used `src/maploader/specials.cpp`,
`src/playsim/mapthinkers/a_lights.cpp`, `p_lnspec.h` and
`wadsrc/static/xlat/doom.txt`. These are synthetic source-derived tests.
Only the lighting component of strobe/hurt special 4/68 is implemented here;
this pass does not implement its sector damage. Additional game-specific lighting variants,
other game-specific translations and
namespace-specific UDMF handling remain open, along with RNG/save limitations
already listed. Immutable map special values are retained.

### Automatic random flashes and fire flicker

Doom specials 1/17 and extended specials 65/81 now create DLightFlash-style and
DFireFlicker-style effects at startup. Light flashes capture current and minimum
neighbor brightness, with bright countdowns of 1 or 65 ticks and dark countdowns
of 1–8 ticks. Unlike action 115's DFlicker, they transition on the decrement that
reaches zero. Fire flicker updates every fourth tick, chooses a reduction of
0/16/32/48 and compares that reduction against the current brightness before
choosing the captured maximum minus reduction or minimum-neighbor-plus-16.
The latter minimum is signed-short-clamped and may exceed the initial maximum;
the native behavior is preserved rather than normalizing those bounds.

Thirteen regression cases cover both format mappings and generalized Doom bits,
countdown ranges, four-tick cadence, external brightness changes, high minimum
clamping, stop behavior, combat RNG isolation and deterministic replay. Reviewed
against DLightFlash/DFireFlicker in `a_lights.cpp`, startup dispatch in
`src/maploader/specials.cpp` and Doom sector translations. Both effects have
separate checksummed managed RNG streams; they still do not reproduce native
FCRandom sequences or persist through v5 pose saves. The full managed suite,
including existing invasion tests, passes; this is not native runtime validation.

### Standalone phased sector lighting

Doom special 21 and extended special 1 now initialize DPhased-style lighting.
The initial light's low six bits choose the starting phase; the runtime cycle
uses a fixed base brightness of 48, twelve steps per ramp and a 64-tick period.
Integer interpolation produces a peak of 237 for two ticks, not 255. Startup
does not immediately overwrite the map brightness; the first simulation tick
does. Immediate light changes do not reset the phase, and tagged Stop removes
the effect while preserving its current brightness.

Twelve tests cover three full cycles, Doom generalized bits, extended mapping,
signed/overbright initial values, stop/override behavior and checksums for phases
with equal current brightness. Expected cycles were derived from DPhased::Tick
and MapLoader::SpawnLights in native source. Connected LightSequenceStart and
alternating sequence sectors are covered by the subsequent pass below; this section covers standalone
phased sectors. Native runtime comparison, UDMF namespace handling and complete
lighting saves remain open.

### Connected phased light sequences

Doom sector 22 and extended sector 2 now start chains through alternating
23/24 or 3/4 neighbors. Traversal follows the first matching linedef neighbor,
excludes the previous sector and terminates on revisiting a sector. Iterative
traversal avoids native recursion depth limits. Zero brightness inherits the
preceding base; nonzero brightness replaces it. Initial phases distribute the
64-tick cycle across the chain. A local initialization-special array consumes
members so later starts cannot reuse them; immutable map definitions are retained.

Nine tests cover both map formats, inherited brightness, phase spacing, branch
order, cycles, competing starts, isolated zero bases, stop-all behavior and
extended flags. Native InitSectorSpecial strips flags in sector order; the
implementation preserves that timing, so an uninitialized flagged neighbor does
not match P_NextSpecialSector's exact special comparison. Source references are
DPhased::PhaseHelper, P_NextSpecialSector and MapLoader::InitSectorSpecial.
Two initial test expectations used the wrong peak tick and were corrected after
checking the native cycle. No tests were disabled. Runtime special consumption
outside lighting initialization is not yet modeled, and full save persistence,
native runtime comparison and game/UDMF namespace variants remain open.

### Light maximum compatibility and negative search values

The EV_LightTurnOn source audit found that negative ChangeToValue arguments must
remain the initial neighbor-search brightness. The earlier implementation
incorrectly replaced every negative value with -1. Corrected this while retaining
-1 for MaxNeighbor. Added the opt-in CompatSurface.SharedLightMaximum flag,
matching COMPATF_LIGHT's carry-forward behavior across tagged sectors: each
resolved value becomes the next sector's search baseline, and once nonnegative
it is applied directly to subsequent sectors. This is not a global maximum scan.
Raise, lower and minimum operations are unaffected.

Eleven new regressions cover default versus compatibility behavior, negative
baselines, negative carried values and unrelated operations. The source reference
is EV_LightTurnOn in `src/playsim/mapthinkers/a_lights.cpp`. The flag is available
through AuthoritySimulation.Start's existing compatibility argument, defaults off,
and is not automatically selected from native MAPINFO/CVars. Full compatibility
configuration loading remains open. Earlier general lighting test success did
not cover these signed-search cases; this audit fixes that coverage gap.

### Direct ACS lighting calls

The word-format ACS interpreter now reads LSPEC1DIRECT through LSPEC5DIRECT for
converted lighting specials 110–117 and 232–234. Map lines and ACS use a shared
lighting dispatcher. Arguments retain their signed values, omitted arguments are
zero, and all required operands must be present before an action can execute.
No player activator is needed for these tag-based actions. Existing one-argument
internal-action fallback remains unchanged; unsupported multi-argument actions
stop the fiber rather than silently continuing into later mutations.

Thirteen tests cover immediate changes, fade/stop, glow, five-argument strobes,
all five truncated operand counts, zero defaults, instruction alignment and
unsupported-action termination. Native reference: LSPEC1DIRECT–LSPEC5DIRECT in
`src/playsim/p_acs.cpp`. Compact/enhanced ACS, remaining stack-based special calls, native
special-argument compatibility masks, complete actor/line activation context
and non-lighting multi-argument dispatch remain unconverted. This does not make
the existing ACS subset a complete native interpreter.

### Stack-based ACS lighting calls

Word-format LSPEC1–LSPEC5 now pass one to five stack arguments to the same
lighting dispatcher as direct calls and map lines. Argument order follows native
STACK(n) through STACK(1); only the required top operands are consumed, leaving
earlier stack values intact. Missing instruction operands or insufficient stack
values stop the fiber before any action is applied. Unsupported stack specials
also stop rather than being misinterpreted as the legacy internal action API.

Twenty regressions compare all eleven converted lighting specials against direct
calls over 100 future ticks and test every underflow count, argument consumption,
arithmetic-produced values, truncated special operands and unsupported actions.
Reviewed against PCD_LSPEC1–PCD_LSPEC5 in `src/playsim/p_acs.cpp`. Native malformed
stack handling logs and attempts recovery in some cases; the managed subset
deliberately terminates instead. Native compatibility argument masks, compact
bytecode, result-returning/extended special opcodes, activation context and
non-lighting stack special dispatch remain open. Existing invasion regressions
pass, but this does not certify ACS-driven invasion maps against native runtime.

### Legacy invasion ACS queries and opcode correction

Corrected the managed Lspec6/Lspec6Direct enum values from 128/129 to native
129/130. These legacy opcodes are zero-operand invasion queries, not six-argument
special calls. The VM now pushes the managed director's wave for 129 and its
classic state for 130: waiting/disabled 0, countdown 5, wave 6, intermission 7,
victory 8. Disabling invasion does not erase the retained wave number. Existing
bytecode walkers use the corrected enum IDs as well. Opcode 128 is no longer
misidentified as the wave query; its actual non-invasion behavior is unimplemented.

Six new regressions use literal native opcode numbers and cover countdown,
wave completion, intermission, next wave, victory, disabling, zero-operand stack
alignment and rejection of the prior wrong wave ID. References: PCD_LSPEC6 and
PCD_LSPEC6DIRECT in `p_acs.cpp`, Net_GetInvasionWave and
Net_GetClassicInvasionState in `d_net.cpp`. Queries do not advance timers or
recount enemies. ACS still runs before the director in a simulation tick and
therefore reads the state committed by the previous director update.
Remaining native invasion metadata/control functions,
failure-state support and native runtime/map comparison remain open. Passing
these tests does not establish that the original invasion symptom is fully fixed.

### Core invasion function queries

Native word-format CALLFUNC opcode 351 now supports zero-argument functions
19700–19707: native state, state tics, wave, maximum waves, wave budget, spawned,
cleared and active monsters. Queries read committed director fields without
advancing its timer or recounting actors. The snapshot publisher and ACS now use
the same NativeState property; the managed atomic-spawn wave maps to native
cleanup (4), whereas the separate classic query still returns active state 6.
Budget remains Spawned, matching the existing managed snapshot approximation.

Seven tests cover all eight values across countdown, kills, intermission, next
wave and victory, native versus classic IDs, disabled state, invalid argument
counts, unknown functions and truncated instructions. Unsupported CALLFUNC
variants terminate the fiber. An initial byte conversion compile error was fixed;
the subsequent full suite passes. Native references: PCD_CALLFUNC and ACSF
19700–19707 in `p_acs.cpp`, and EInvasionState in `d_net.h`.

The subsequent opcode audit below corrects the older CallFunc alias; the VM
recognizes native 351, now shared with the enum and bytecode walker.
Compact ACS and nonzero-argument function calls remain unsupported. Boss waves,
spawn-plan metadata/control functions, staged native spawning, failure behavior,
native timer authority/cutscene policy and native-map runtime comparisons remain
open. Tests establish consistency with current managed state, not full native
invasion parity or resolution of every reported synchronization symptom.

### Native CALLFUNC and adjacent opcode decoding

Corrected enum IDs for CALLFUNC, SAVESTRING and the six map/world/global string
range operations to native 351–358. Removed redundant walker aliases and changed
the VM to use the corrected CallFunc enum. This prevents native opcode 353
(PRINTMAPCHRANGE) from being decoded as a two-operand function call. Word-format
CALLFUNC has two word operands; compact CALLFUNC has three operand bytes: one
argument-count byte and a two-byte function index. The compact walker previously
skipped only two, losing alignment. A trailing extended-opcode prefix now returns
a truncation rejection instead of reading past the buffer.

Fifteen literal-wire tests cover the eight enum values, adjacent word/compact
instructions, all incomplete compact CALLFUNC operands and truncated extended
opcode prefixes. Native references are the PCD enum, NEXTSHORT and PCD_CALLFUNC
in `src/playsim/p_acs.cpp`. Existing tests and the full solution pass. This audits
only this opcode block; other legacy aliases remain inconsistent, and recognizing
compact bytecode in the loader does not implement compact execution in AcsVm.

### Function-pointer, script-array and translation opcode IDs

Corrected enum IDs for PUSHFUNCTION through ORSCRIPTARRAY to native 359–375,
added named script-array shift opcodes 376/377, and corrected TRANSLATIONRANGE4/5
to 383/384. The walker now accepts the latter native IDs in both word and compact
formats. Twenty-one literal-ID cases and two complete block-alignment tests
cover the corrected values and immediate operand widths. The native reference
is the PCD enum and NEXTBYTE/NEXTWORD instruction handlers in `p_acs.cpp`.

An existing compact translation fixture encoded 364 (ASSIGNSCRIPTARRAY) instead
of intended 362 (TRANSLATIONRANGE3). Corrected the fixture to native 362 after it
failed with the enum correction; its expected instruction count remains unchanged.
All tests pass. These changes correct decoding/naming, not execution of function
pointers, script arrays or translations. Other historical aliases and the
remaining special-call handling still require audit; legacy non-native walker
aliases have not all been removed.

### Extended special-call decoding and lighting execution

Fixed LSPEC5EX/LSPEC5EXRESULT (381/382) decoding: both read a four-byte special
number in word and compact formats. Previously the walker treated them as having
no operands. Two old fixtures were corrected to include the required operand and
the expected operand count was updated. Four new decoding cases check alignment
using a multi-byte special number and reject every truncated operand length.

The word-format VM now executes these two opcodes for converted lighting actions.
Both consume five stack arguments; the result form pushes the action's boolean
result while retaining any earlier stack values. Four runtime regressions check
void/result stack behavior and underflow rejection. Native reference:
PCD_LSPEC5EX/PCD_LSPEC5EXRESULT and NEXTWORD in `p_acs.cpp`. Unsupported actions
still terminate the fiber. Compact execution, native compatibility masks and
other result-returning special opcodes remain unconverted. Full solution tests
and warning-as-error build pass; native runtime parity is not certified.

### Standard result-returning special call

Corrected LSPEC5RESULT from the erroneous managed ID 343 to native 263. Its
special number is a word operand in word-format ACS and a single byte in compact
ACS, unlike LSPEC5EXRESULT's four bytes in both formats. Corrected the walker,
removed a now-unreachable legacy word-walker alias arm, and updated the existing
fixture to supply the required operand. Native 343 remains decoded separately
as SETMUGSHOTSTATE; it is not executed as a lighting call.

The word-format VM now executes 263 for converted lighting actions, consuming
five arguments and pushing success while preserving the lower stack. Six added
cases cover the literal opcode ID, both operand widths, truncated decoding,
result-stack behavior, underflow and missing special operands. Source reference:
PCD_LSPEC5RESULT and NEXTBYTE in `p_acs.cpp`. Other legacy aliases, including
AddGlobalVar's former collision with 263 (corrected below), require care; callers must not
interpret that alias as implemented global-variable execution. Compact execution
and non-lighting result calls remain unconverted. The full managed suite passes.

### Global-variable opcode collisions and compact index widths

Corrected Add/Sub/Mul/Div/Mod/Inc/DecGlobalVar from erroneous 263–269 to native
183–189, removing the collision with LSPEC5RESULT and unrelated sector/player
queries. Corrected SetLineMonsterBlocking from 184 to native 104 so subtraction
is no longer mistaken for an operand-free line operation. Word decoding now
consumes each global-variable index; compact AssignGlobalVar/PushGlobalVar now
consume one byte instead of four, consistent with native NEXTBYTE.

Twenty-one new cases verify ten native enum values, word/compact block alignment
and all nine missing compact indices. Source references are the native PCD enum
and global-variable handlers in `p_acs.cpp`. The full suite and warning-as-error
build pass. These were decoding fixes; the subsequent pass adds scalar execution, while
cross-map lifetime remains unimplemented. Other opcode-table collisions, including
the old player-key aliases around 104, still require audit; this does not certify
the entire ACS decoder or VM.

### Scalar ACS global-variable execution

The word-format VM now implements native opcodes 181–189: assign, push, add,
subtract, multiply, divide, remainder, increment and decrement for 64 signed
scalar globals. Values are shared across fibers in the same VM and included in
the simulation checksum. Invalid indices, missing operands/stack values and zero
divisors terminate the fiber before altering a global. Increment/decrement take
no stack operand. Arithmetic wraps to 32 bits; signed division truncates toward
zero. Widening division/remainder avoids CLR exceptions for INT_MIN/-1, defining
that overflow result as wrapped INT_MIN and zero remainder respectively.

Seventeen new cases exercise shared values, signed arithmetic, overflow, the
highest valid index, invalid indices, divide/modulo by zero, underflow, isolation
of new simulations and checksums. Sources: NUM_GLOBALVARS and global-variable
handlers in `p_acs.h`/`p_acs.cpp`. Native undefined signed-overflow behavior is not
claimed as a portable parity guarantee. Values currently live within one AcsVm;
cross-map game-session lifetime and save persistence remain unimplemented, as do
global arrays, bitwise operations and compact execution. The checksum addition
does not imply complete ACS fiber/stack/program-state hashing or save support.

### ACS expressions and zero-condition branches

Added multiply/divide/remainder, all six signed comparisons, logical AND/OR/NOT,
bitwise AND/OR/XOR, shifts, unary minus and IFNOTGOTO to the word-format VM.
Boolean operations return 0/1. Binary operations now require both stack operands
before consuming them; zero divisors stop the fiber without executing later
world mutations. Unary and conditional branches likewise stop on underflow.
IFNOTGOTO consumes its condition and branches only for zero.

Thirty-nine new cases cover operand order, negative arithmetic, true/false
comparisons, nonzero logical values, shifts, unary operations, branching and
malformed expressions. Reviewed against the expression handlers in `p_acs.cpp`.
Arithmetic wrapping and INT_MIN/-1 use the managed policy described above;
out-of-range shift counts follow C#'s low-five-bit masking, not a claim about
undefined native C++ shifts. Jump offsets still address the supplied AcsProgram
code buffer; native BEHAVIOR/module-relative address integration is incomplete.
These tests use explicit word programs, not compiled native map fixtures.

### ACS delay timing correction

The scheduler now resumes a delayed script on the tick its counter reaches zero;
previously it waited one additional tick. DELAYDIRECT with nonpositive duration
now continues within the current instruction budget instead of yielding. Added
stack-based DELAY with the same timing and stack-underflow handling. Programs
may explicitly enable LegacyHexenDelay to add the native old-Hexen extra tick;
the default remains normal timing. This option requires caller selection because
native game/ACS-format metadata integration remains incomplete.

Fifteen new cases cover both delay forms, positive/zero/negative durations,
legacy timing, retained stack values, underflow and instruction-budget bounds
for zero-delay loops. Corrected the older test that expected Delay(1) to resume
on tick three rather than tick two. References: SCRIPT_Delayed, PCD_DELAY and
PCD_DELAYDIRECT in `p_acs.cpp`. Extreme legacy durations saturate at INT_MAX to
avoid overflow; undefined native signed overflow is not emulated. Full managed
tests, including invasion, pass. This removes a concrete script timing mismatch
but does not certify native invasion map behavior or complete ACS scheduling.

### Script-local scalar variables

Word-format scripts now execute native local assign/push/add/subtract/multiply/
divide/remainder/increment/decrement opcodes. Each fiber owns 20 zero-initialized
locals (native default LOCAL_SIZE), preserved through delays and discarded when
that invocation ends. Scalar arithmetic and validation reuse the global-variable
path; locals never alias global storage. Added names for the previously unnamed
native local arithmetic opcodes.

Twelve tests cover all arithmetic operations, slot 19, concurrent delayed fibers,
fresh invocation state, stack-free increment/decrement, invalid indices and zero
divisors. Reviewed against LOCAL_SIZE in `p_acs.h` and script-variable handlers
in `p_acs.cpp`; existing global and invasion regressions remain passing. Enhanced
script VarCount metadata, script arguments, local arrays, full suspension/resume,
save persistence remains open. At that point the
simulation checksum included globals but not locals; the pass below closes that gap.

### ACS execution-state checksums and owned bytecode

The simulation now incorporates AcsVm.Checksum, covering scalar globals,
registered program IDs/code/options, and ordered fibers with their program/code
identity, program counter, wait, stack and local values. Registration order is
normalized by program ID; execution order is retained because it affects shared
state. Registered bytecode is cloned and hashed once, preventing caller mutations
from changing active code or invalidating cached code hashes. Replacing a program
does not replace the bytecode already held by an active fiber.

Seven tests verify deterministic replay, registration versus execution order,
different stack/local values with otherwise equal programs/globals, wait-counter
changes and input-buffer ownership. Direct-versus-stack lighting equivalence
tests now compare observable lighting and running status, since different program
bytes intentionally produce different full simulation checksums. All tests pass.
This is a managed divergence diagnostic, not a native checksum format or a
collision-free proof. Checksums do not persist or replicate the script state;
save/restore, session lifetime and native ACS parity remain incomplete.

### Map-scoped ACS scalar variables

Added assign/read and arithmetic for the native map-variable opcodes, backed by
128 signed slots shared by scripts in the current VM. Map slots remain separate
from 20 per-fiber locals and 64 scalar globals, and participate in ACS/simulation
checksums. Bounds and arithmetic failure handling reuse the scalar implementation;
new simulations initialize map values to zero.

Thirteen new cases cover the final valid slot, arithmetic, sharing between
scripts, scope separation, increment/decrement, new-map isolation, invalid
indices, zero divisors and checksum differences with identical program registries.
Native references: NUM_MAPVARS in `p_acs.h` and map-variable handlers in
`p_acs.cpp`. This models one behavior module's scalar storage, not native module
imports/variable aliases or map-variable initializer chunks. Map arrays, hub
restoration, save persistence and compact execution remain unconverted. All
managed tests, including invasion regressions, and the warning-as-error build pass.

### ACS case branches

Converted native CASEGOTO (84) and CASEGOTOSORTED (256) for the managed
word-code executor. Matching branches consume the selector; unmatched cases
retain it for later cases or the default path. Sorted tables use signed comparisons
and binary search, matching the handlers in `src/playsim/p_acs.cpp`. Empty tables
continue with the selector intact. Table bounds are checked before searching.

Taken branches now share destination validation with GOTO, IFGOTO and IFNOTGOTO:
negative, unaligned and out-of-buffer destinations terminate the script. Untaken
branches do not validate unused destinations. Twenty-five regression cases cover
case chains, sorted hits/misses, selector consumption, missing selectors, malformed
table lengths and invalid ordinary/conditional branch destinations. Full managed
tests, including invasion regressions, pass.

This is a source comparison and regression audit of these handlers. Destinations
remain offsets within the supplied AcsProgram.Code buffer; native behavior-module
offset relocation and compact bytecode execution are still missing. Alignment and
bounds checks do not prove that a target is an instruction boundary. Sorted-table
ordering is assumed, as in the native handler. These changes do not complete ACS
or establish native runtime parity.

### ACS stack operations and restart

Converted DUP, SWAP, NEGATEBINARY and RESTART in the word-code executor, using
the native handlers in `src/playsim/p_acs.cpp` as the source reference. The audit
found incorrect enum values: DUP is 216 (previously 196), SWAP is 217 (previously
197), and NEGATEBINARY is 330 (previously 332). Corrected these and an existing
decoder fixture that incorrectly identified 332/333/334 as negate/get/set pitch;
the actual sequence is 330/331/332. Other legacy enum aliases remain unaudited.

DUP preserves the original top value, SWAP exchanges only the top two values,
and bitwise negation complements all 32 bits. Insufficient operands terminate
the script before following mutations. RESTART returns to offset zero in the
current script buffer while retaining locals and the evaluation stack. This
does not enqueue a replacement fiber or reset shared state.

Seventeen execution cases cover stack ordering, bit patterns, underflow, rejection
of the former enum numbers, finite restarts retaining locals/stack, delayed
restarts and scheduler fairness for an endless restart. Six new decoder/enum
cases use native wire numbers and verify zero operand widths in both bytecode
formats. All 1,471 managed tests pass, including invasion regressions.

The executor still assumes a single script beginning at offset zero, rather than
native behavior-module entry-point lookup. Endless scripts yield at the existing
64-instruction budget; native runaway-script termination and stack-size limits
are not implemented. Compact bytecode is decoded but not executed by this VM.
This pass does not establish native scheduler or real-map runtime parity.

### ACS fixed-point arithmetic

Converted FIXEDMUL (136) and FIXEDDIV (137) in the word-code executor after
reviewing the handlers in `src/playsim/p_acs.cpp` and MulScale/DivScale in
`src/common/utility/m_fixed.h`. Multiplication uses a signed 64-bit product,
arithmetic right shift by 16, and a wrapping 32-bit result. Division uses the
native magnitude guard before scaled integer division, saturating to signed
minimum/maximum on overflow or zero divisors. Ordinary integer DIVIDE retains
its existing zero-divisor termination behavior. Magnitudes are calculated after
widening to avoid a CLR exception on INT_MIN; no native compiler differential
test for that edge case was run.

The enum audit found an omitted SINGLEPLAYER entry at 135, leaving fixed-point
and adjacent gravity/air-control opcode names shifted by one. Added the entry
and corrected 136 through 141. Decoder widths now follow those native numbers:
the direct gravity/air-control variants consume one four-byte argument in both
formats. This corrects decoding only; SINGLEPLAYER and gravity/air-control
execution are still unsupported by this VM. Existing enum-based decoder fixtures
now emit the corrected numbers.

Thirty-one execution cases verify full 32-bit results, fractional/sign behavior,
rounding, wrapping multiplication, saturating division boundaries, zero divisors,
stack underflow and adjacent unsupported opcodes. Nine enum/decoder cases check
literal native numbers and mixed instruction widths. All 1,511 managed tests,
including invasion regressions, pass. This source/regression audit does not
complete ACS, native behavior-module integration, or real-map validation.

### ACS scalar bitwise updates

Converted AND, exclusive-OR and OR assignment for script-local, map and global
scalar variables, using the corresponding handlers in `src/playsim/p_acs.cpp`.
Each operation consumes one stack operand and an immediate variable index,
updates only the selected slot, and preserves the lower stack. Added the missing
ORGLOBALVAR enum name at native opcode 308. Invalid indices, truncated index
operands and stack underflow terminate the script before an update.

Thirty-four execution cases check all nine operations, full signed bit patterns,
last valid slots, out-of-range/negative indices, underflow, truncated instructions,
scope isolation and persistence of map/global updates between scripts in one VM.
Eleven enum/decoder cases check literal native numbers, compact versus word
index widths, and truncated indices. All 1,556 managed tests pass, including
invasion regressions. Existing ACS checksums already include these scalar slots.

This source audit also identified an enum defect (corrected in the following pass): the legacy world-array
names at 306 through 314 overlapped native scalar/array bitwise opcodes; native
world-array operations actually occupy 226 through 234. Runtime dispatch in this pass follows
native scalar wire numbers, not those array aliases. World-variable and array
execution, scalar shifts, cross-map/hub lifetime, compact execution and full
behavior-module integration remain incomplete. No native runtime parity claim
is made for this pass.

### ACS array opcode and decoder conflict audit

Corrected all nine world-array opcode names to native 226–234 and all nine
global-array names to 235–243. Resolving those collisions also required correcting
direct inventory, music and print names to their native 144/146/148 and 153–158
values, actor property operations to 245/246, ACTIVATORTID to 248, and PUSHBYTE
to 167. The reference is the opcode enumeration and handlers in
`src/playsim/p_acs.cpp`. These changes close the specific world-array enum defect
reported above; unrelated legacy opcode aliases remain outside this pass.

The decoder now consumes exactly one variable index for every world/global array
operation and retains index widths for the former 306–314 aliases, which are
native bitwise/shift instructions. Restored the previously omitted word-format
DIVGLOBALARRAY case. CHECKINVENTORYDIRECT consumes one four-byte string argument,
not two. Compact PUSHBYTE consumes one byte while PLAYERNUMBER consumes none.
Corrected two older compact fixtures that encoded PUSHBYTE and global arrays
using the erroneous numbers; enum-based fixtures now emit the corrected IDs.

Thirty-six new cases verify 31 enum values, all 18 array operand widths and
truncations in both formats, adjacent instructions and former-alias widths, and
compact PUSHBYTE/PLAYERNUMBER boundaries. The full 1,592-test managed suite,
including invasion regressions, passes. The warning-as-error build passes.
This is decoding and source-audit work, not array runtime conversion: the managed
VM still lacks array execution/storage, complete native behavior-module binding,
compact execution and real-map parity validation.

### ACS scalar shift assignments

Converted left/right shift assignment for script-local, map and global scalar
slots. Each handler consumes one stack operand and an immediate index, preserves
the lower stack and updates only its selected scope. The native reference is
PCD_LSSCRIPTVAR through PCD_RSGLOBALVAR in `src/playsim/p_acs.cpp`.

Corrected all 14 variable/array shift enum names from erroneous 393–406 to native
312–325. Added decoding for native 325, which had lost its one-index width when
the conflicting actor-property enum was corrected. Legacy decoder aliases at
393–406 remain accepted for now, with comments corrected to identify them as
legacy rather than native wire numbers. Older tests for those aliases remain;
the new tests independently check the actual native range.

Thirty-four execution cases cover signed right shifts, left-shift bit patterns,
last valid slots, invalid/truncated indices, underflow, scope isolation and shared
values surviving script termination. Sixteen enum/decoder cases verify all 14
native IDs and index/truncation widths in both formats. All 1,642 managed tests,
including invasion regressions, pass. Scalar values already participate in ACS
checksums; no new persistence or replication format was introduced.

The managed implementation uses C# shift-count masking to five bits, including
negative counts and counts at least 32. Those boundary tests establish the managed
contract, not portable native C++ semantics for undefined shifts or overflowing
signed left shifts. World-variable/array execution, compact execution, complete
native module integration and real-map parity remain unfinished. This pass
supersedes the earlier statement that all scalar shifts were unconverted.

### ACS world scalar variables

Converted world-variable assign/read, add/subtract/multiply/divide/remainder,
increment/decrement, bitwise AND/XOR/OR and left/right shift operations. Added
256 signed slots, matching NUM_WORLDVARS in `src/playsim/p_acs.h`, shared among
scripts in the current VM and separate from local, map and global storage.
World values now participate in ACS checksums. Added missing arithmetic enum
names at native 33/36/39/42/45/48; existing decoder ranges already accept their
immediate indices. Native handlers and P_ClearACSVars were reviewed in p_acs.cpp.

Twenty-nine execution cases use literal native opcodes to check arithmetic and
bitwise/shift results, slot 255, signed/wrapping boundaries, stack preservation,
invalid/truncated indices, underflow, zero divisors, four-scope isolation,
inter-script sharing and checksum differences with identical program registries.
All 1,671 managed tests, including invasion regressions, pass. Arithmetic edge
behavior follows the existing managed scalar contract: wrapping integer results,
widened division/remainder intermediates and masked shift counts. This does not
certify portable native C++ behavior for undefined arithmetic cases.

This closes world scalar instruction execution within one VM, superseding earlier
statements that those instructions were unsupported. It does not complete world
scope lifetime: separate simulations initialize separate zeroed storage, with no
hub transition ownership, cross-map handoff, save/restore or native string-root
integration. World/global arrays, compact execution and real-map parity remain
unfinished. Full phases 1–3 completion is still not achieved.

### ACS world/global sparse array execution

Converted the 18 basic world/global array instructions at native 226–243:
read, assign, add, subtract, multiply, divide, remainder, increment and decrement
for both scopes. Storage follows FWorldGlobalArray in `src/playsim/p_acs.h`:
signed 32-bit element keys with missing values reading as zero. World array IDs
are bounded to 256 and global IDs to 64. Read replaces the element index on the
stack; arithmetic/assignment consumes index and value; increment/decrement consumes
only the index. Native stack order and handlers were reviewed in p_acs.cpp.

Array values are included in ACS checksums, sorted by array ID then signed element
key, independently of insertion order. Zero writes remove entries and missing
reads do not insert them. This canonicalizes observable values for managed hashes;
it does not reproduce native dictionary allocation or native save serialization.
Arithmetic uses the existing managed wrapping/widened-division contract. Invalid
array IDs, missing operands and zero divisors terminate without changing an element.

Thirty-nine new cases cover both scopes, signed/sparse keys, the final valid array
IDs, arithmetic and boundary values, stack consumption, invalid IDs, underflow,
zero-divisor preservation, scalar/array separation, inter-script sharing and
checksum ordering/zero canonicalization. All 1,710 managed tests, including
invasion regressions, pass. This supersedes earlier statements that world/global
arrays had no runtime storage or execution support.

Array bitwise and shift operations, local/map arrays, string-array operations,
cross-map/hub ownership, complete save/restore and compact execution remain
unconverted. Sparse storage currently has no entry quota. This source/regression
audit does not establish native runtime parity or complete phases 1–3.

### ACS world/global array bitwise and shift updates

Converted AND, XOR, OR, left-shift and right-shift updates for both world/global
arrays, using the native handlers in `src/playsim/p_acs.cpp` as the reference.
Each instruction reads one array ID and consumes an element index plus operand,
preserving the lower stack. Added the missing ORWORLDARRAY and ORGLOBALARRAY enum
names at native 310/311. Existing decoder widths already support these IDs.
The shared sparse-array executor now covers all 28 basic arithmetic/bitwise/shift
instructions for these two scopes, retaining signed keys and deterministic hashes.

Thirty-six new cases cover all ten operations, high/sign bits, slot 255/63,
negative element keys, inter-script persistence, lower-stack preservation,
missing operands, invalid IDs and truncated array IDs. Shift-boundary tests
explicitly verify C#'s masked-count behavior, not portable native C++ behavior
for undefined shifts. All 1,746 managed tests, including invasion regressions,
pass. Existing zero canonicalization and checksum ordering remain unchanged.

This supersedes the previous world/global array bitwise/shift execution gap.
Local/map arrays, string-array operations, native module binding, compact
execution, cross-map/hub ownership and complete save/restore remain unfinished.
This source/regression audit is not a real-map native parity certification and
does not complete gameplay phases 1–3.

### ACS map-array declaration metadata

Added MapBehaviorMapArrayCodec to decode an already-delimited enhanced ACS chunk
region. It preserves ARAY map-variable references and unsigned lengths plus AINI
signed initializer values in source order. The first ARAY chunk wins, as in native
FindChunk; duplicate variable bindings remain available for later native-order
binding. Unknown chunks are skipped after checking their bounds. Decoding does
not allocate storage based on declared array length, and returned lists own their
data. Native ARAY/AINI loading in `src/playsim/p_acs.cpp` was reviewed.

Fifteen regression cases cover declaration/initializer ordering, repeated bindings,
first-chunk selection, zero/large lengths, signed values, source-buffer ownership,
unknown chunks, malformed payload sizes, invalid map-variable slots, truncation
and chunk-length overflow. All 1,761 managed tests, including invasion regressions,
pass. Bad input returns no partial metadata.

This codec is groundwork, not end-to-end map-array execution. It requires the caller
to delimit the enhanced chunk region; it does not locate old-format enhanced
trailers or bind arrays to the VM. MINI/import bindings, initializer application
and clipping, native string-array tags, runtime allocation limits and local-array
metadata remain unfinished. Raw initializers are intentionally not resolved to
declarations here because native binding depends on module map-variable state.

### ACS map-array binding and execution

Connected decoded ARAY/AINI metadata to an explicit TryLoadMapArrays VM API for
one behavior module, and converted all 14 map-array read/write, arithmetic,
increment/decrement, bitwise and shift instructions. Added ORMAPARRAY at native
309. Shared arithmetic evaluation with world/global arrays so signed/wrapping
and masked-shift behavior remains consistent with the existing managed contract.

Declarations bind map-variable slots to arrays in declaration order; duplicate
slots point at the last declaration. Initializers resolve through the resulting
map-variable state, apply in source order and clip to declared lengths. Existing
map-variable values are retained unless overwritten by declarations, allowing
explicit aliases. Storage owns copied values. Runtime instructions resolve the
binding on each access; invalid array bindings/elements read as zero and ignore
writes, matching FBehavior::GetArrayVal/SetArrayVal. Invalid variable slots and
malformed stack operands terminate scripts. Zero divisors leave values unchanged.

Loading rejects active scripts and validates before committing arrays/bindings.
The managed allocation limits are 4,096 arrays and 1,048,576 total elements per
load; these are supported-runtime limits, not native format restrictions. Array
shape and contents participate in ACS checksums. Twenty-seven regression cases
cover native chunk decoding through execution, all operations, aliasing, duplicate
bindings, clipped/repeated initialization, source ownership, bounds, underflow,
zero divisors, rejected-load atomicity and checksums. All 1,788 managed tests,
including invasion regressions, pass.

This supersedes the prior gap in metadata-to-VM binding and map-array instruction
execution. Callers must still locate/decode chunks and explicitly load metadata;
automatic map startup, MINI parsing, imports/multiple behavior modules, native
string tags, local arrays, compact execution and complete persistence remain
unfinished. No native real-map parity certification or phase completion is claimed.

### ACS MINI initialization and module reset correction

Added checked MINI map-variable initializer decoding and application. Initializers
retain source order and signed values, reject ranges beyond the 128 map slots,
and own their decoded data. An empty range at slot 128 is accepted. Module loading
now clears map variables, applies MINI chunks in order, binds ARAY declarations,
then applies AINI chunks, regardless of the relative placement of chunk types.

This source audit found and corrected a mismatch in the preceding implementation:
it retained prior map-variable values during loading. The native loader clears
MapVarStore first. That earlier description is superseded; aliases must now come
from MINI or subsequent script assignments. The existing alias regression was
updated accordingly. World/global state is retained, and failed or active-script
loads still leave current map state untouched.

Eight decoder and seven VM regression cases verify signed/overlapping initializers,
source ownership, range boundaries, malformed lengths, native load order, array
aliases, reset scope and failed-load atomicity. All 1,803 managed tests pass,
including invasion regressions. This closes MINI decoding/application through the
explicit module-load API. Automatic map startup, imports, local arrays, native
string tags, compact execution, persistence and native runtime validation remain
unfinished; phases 1–3 are not complete.

### ACS script-local arrays and layout metadata

Converted all 14 script-local array instructions at native 364–377. AcsProgram
now accepts LocalVariableCount (default 20) and LocalArraySizes. Each fiber owns
one zeroed local-storage block, with arrays placed after scalar locals and exposed
through checked offsets, matching ACSLocalArrays and ParseLocalArrayChunk in the
native sources. Out-of-range array/element reads return zero and writes are ignored;
missing operands and zero divisors terminate. Layouts are copied at registration,
and replacing a program does not alter active fibers. Program/fiber hashes include
layout metadata as well as code, and local values remain hashed.

Registration validates layout before replacement, with managed limits of 4,096
arrays and 1,048,576 total local slots per fiber. These are supported-runtime limits,
not native format guarantees. Arithmetic uses the existing managed wrapping and
masked-shift contract, with the same native portability caveats as scalar shifts.

Added MapBehaviorLocalLayoutCodec for already-delimited enhanced chunks. SVCT
uses signed script IDs and unsigned local counts; only its first chunk is used.
SARY sizes apply after SVCT regardless of chunk placement. Repeated SARY chunks
append storage after the prior total but replace visible descriptors, following
the native loader. Invalid chunk sizes, unsupported negative sizes and total
storage overflow reject without partial output. The codec retains raw script IDs;
named-script resolution and filtering against the script directory remain external.

Thirty-one execution/integration cases and twelve decoder cases cover operations,
offset sharing, bounds, underflow, delay/concurrent-fiber isolation, definition
ownership, active replacement, layout limits/checksums, signed IDs, chunk order,
repeated arrays and truncation. One integration case executes a layout decoded
from native SVCT/SARY bytes. All 1,846 managed tests, including invasion regressions,
pass. This supersedes the script-local array execution/metadata gaps above.

Callers still explicitly attach decoded layouts to programs. Automatic behavior
module loading, function-local FARY/call frames, script arguments, named-script
binding, compact execution, string-array operations and complete persistence
remain incomplete. Real-map/native runtime parity and full phases 1–3 completion
have not been established.

### ACS script startup arguments

Added AcsProgram.ArgumentCount and an Enqueue overload accepting a read-only span
of signed arguments. Startup copies the smaller of supplied/declared counts into
the first scalar local slots, leaving missing arguments and other local/array
storage zeroed. Extra values are ignored, matching the DLevelScript constructor
in `src/playsim/p_acs.cpp`. The original no-argument enqueue API remains available.
Argument declarations must fit scalar storage so supplied values cannot spill
into the array region. Invalid declarations reject before replacing registration.

Argument count participates in the program/fiber definition hash; copied values
participate through local-storage hashing. Fourteen regressions cover partial and
extra arguments, zero declarations, the original API, input ownership, concurrent
delayed instances, array isolation, invalid definitions, unknown scripts,
checksums and replacement of queued programs. All 1,860 managed tests pass,
including invasion regressions.

This converts startup arguments at the explicit VM API. Script-directory ArgCount
binding, map-line argument forwarding, native activator/line context, duplicate
execution/resume policy and deferred cross-map execution are not converted here.
Automatic behavior loading, function calls, compact execution, persistence and
native runtime validation remain outstanding; phases 1–3 remain incomplete.

### ACS map-line arguments and active-script guard

Extended map-line special 80 now forwards Arg2/Arg3/Arg4 to the declared script
arguments for map target zero. Added TryExecute, which rejects another active
fiber with the same script number before enqueueing, matching the running-script
check for normal ACS_Execute. Enqueue remains an explicit API permitting concurrent
instances. Native references are LS_ACS_Execute in p_lnspec.cpp and P_GetScriptGoing
in p_acs.cpp. Nonzero map targets now fail instead of silently starting locally;
numeric map lookup and deferred cross-map execution are not yet implemented.

Eleven regression cases cover ordered/signed arguments, use/cross activation,
one-shot/repeat rules, missing scripts, nonzero map requests, active/delayed
duplicates, preservation of initial arguments, restart after completion and zero
filling beyond the three line arguments. Failed activation leaves the line intact.
All 1,871 managed tests, including invasion regressions, pass. Initial test fixture
compile errors were corrected by constructing immutable line arguments rather
than trying to modify them.

This closes argument forwarding for the extended map-line path only. The legacy
internal Execute API is unchanged. Native suspended-script resume, ACS_ExecuteAlways,
activator/line context, numeric map resolution (including a nonzero number naming
the current map), automatic module loading, function calls and persistence remain
unfinished. These tests do not certify native runtime parity or phases 1–3 completion.

### ACS opcode suspension and normal execute resume

Corrected PCD_SUSPEND (2), which previously terminated the fiber. It now leaves
the fiber suspended after that instruction; scheduler ticks skip it. TryExecute
resumes a suspended matching script without replacing its arguments or local
state, following P_GetScriptGoing in p_acs.cpp. Already-running scripts still
reject normal duplicate execution. Definition replacement does not alter a
suspended fiber's retained code/layout. Suspension state participates in checksums.

RunningCount continues to count all nonterminated fibers, including suspended
ones, so map-storage reload stays blocked. SuspendedCount reports the suspended
subset. Seven regressions cover local/array/argument preservation, repeated
suspensions, definition replacement, checksum/state stability, storage-reload
rejection, one-shot map-line resume, post-resume delay timing and other scripts
continuing to run. All 1,878 managed tests, including invasion regressions, pass.

This closes opcode-triggered suspend/resume for normal extended map-line execution.
External ACS_Suspend/ACS_Terminate specials and native ACS_ExecuteAlways tracking
remain unconverted. Direct Enqueue can still create concurrent instances; normal
TryExecute selects the first live matching fiber. The managed evaluation stack
is retained across yields; native valid scripts are expected to have an empty
evaluation stack when returning from RunScript, so malformed nonempty-stack yield
parity is not established. Persistence and native runtime validation remain open.

### ACS suspend/terminate map-line controls

Converted extended map-line specials 81/82 for target map zero, backed by VM
SuspendScript/TerminateScript operations. Current-map requests succeed even if
no script matches, following LS_ACS_Suspend/Terminate in p_lnspec.cpp. Control
targets the first live matching fiber, consistent with the current managed normal
execute model. Suspended fibers retain their state; terminated fibers stop being
eligible to run and are removed by the next scheduler cleanup.

Source review of SetScriptState and P_GetScriptGoing showed that external suspend
replaces the delayed state, and resume enters Running. Resume now clears the prior
delay instead of waiting out the old counter. Ten regressions cover missing-script
success, activation/repeat rules, unsupported map targets, delayed suspend/resume,
termination of delayed/suspended scripts, fresh execution after cleanup and
suspension before the first tick. All 1,888 managed tests, including invasion
regressions, pass.

Nonzero map targets reject without mutation; numeric map lookup/deferred requests
remain unsupported. This intentionally differs from the native control specials'
unconditional success for unresolved map numbers. Native ExecuteAlways tracking,
client-side script controllers and exact same-tick termination/restart ordering
remain unverified. Script-invoked control specials, automatic module loading,
function calls, persistence and real-map/native runtime validation remain open.

### Script-invoked ACS execute/suspend/terminate specials

Connected specials 80–82 to both direct and stack LSPEC execution, including
result-returning forms. Map lines and scripts now share ExecuteScriptControl,
so argument order, current-map checks, duplicate rejection and control results
stay consistent. Missing direct operands reject before dispatch. Unsupported
specials retain the existing handling outside this converted set.

Twenty-two cases cover all five direct/stack arities, child-script argument
forwarding, standard/extended result values, unsupported map targets, duplicate
self-execution, self-suspend/resume, self-termination, controlling a later queued
script and truncated control operands. All 1,910 managed tests, including invasion
regressions, pass. Native LSPEC stack ordering and LS_ACS control handlers were
reviewed in p_acs.cpp/p_lnspec.cpp.

The scheduler takes a snapshot per tick: a newly started child runs on the next
VM tick, while a suspended/terminated target already in that snapshot is skipped.
Tests establish this managed scheduling contract; exact native same-tick ordering
is not certified. Activator/back-side context, legacy Hexen argument masks,
ExecuteAlways tracking, remote map handling, automatic module loading, function
calls and persistence remain unfinished. This supersedes the earlier gap in
script-invoked control-special dispatch, not full native runtime parity.

### ACS_ExecuteAlways concurrent instances

Converted current-map special 226 for extended map lines and direct/stack ACS
special calls, including result-returning forms. Each call creates an independent
instance with its own arguments, locals, arrays and execution state. Native
P_GetScriptGoing, DLevelScript construction and SetScriptState in p_acs.cpp
exclude ACS_ALWAYS instances from RunningScripts. Managed normal execute/resume,
suspend and terminate now likewise ignore always instances. Self-suspend and
self-termination opcodes still act on the executing instance. The control mode
participates in the VM checksum because it changes future control behavior.

Fourteen regressions cover concurrent arguments across delay, coexistence with
normal execution, normal duplicate rejection, selective suspend/terminate,
resume preserving old arguments, self-suspended always instances, map-array
reload exclusion, missing programs, checksum distinction, map-line activation
and repeat rules, argument forwarding, rejected remote targets and direct/result
script dispatch. All 1,924 managed tests pass, including prior invasion coverage.
Source review used LS_ACS_ExecuteAlways in p_lnspec.cpp and the tracking code above.

This closes the earlier ExecuteAlways tracking gap for these managed entry points.
The legacy public Enqueue API still permits multiple normal instances and numbered
control selects their first live match; it is not a native RunningScripts registry.
Exact native same-tick scheduling, named scripts, activator/back-side context,
client-side controllers, remote/deferred map execution, automatic module loading,
function calls, persistence and real-map/runtime parity remain incomplete. This
pass does not certify the original invasion round/timer/enemy symptom as fixed.

### Numbered ACS ScriptWait

Converted wordcode ScriptWait (81) and ScriptWaitDirect (82), following the
default native PCD_SCRIPTWAIT handlers and SCRIPT_ScriptWaitPre/ScriptWait states
in p_acs.cpp. An absent target must first be observed running; an existing target
must finish before execution resumes. Suspended normal instances still count,
while ExecuteAlways instances do not. Wait state and target enter the checksum.
External suspend followed by normal execute resumes after the wait instruction,
discarding the previous wait state as native state replacement does. Waiting
instances remain live and prevent map-array reloads.

Eleven regression cases cover stack/direct forms, delayed and absent targets,
always-instance exclusion, suspended targets, termination, both wait states on
resume, self-wait, truncated operands/stack underflow, and checksum target state
with identical remaining locals and stacks. The full 1,935-test suite passes.

The managed scheduler retains its per-tick snapshot/order. Native PutLast/PutFirst
reordering and exact same-tick completion/restart behavior are not reproduced;
a target that starts and finishes between waiter observations can be missed.
Named waits, the COMPATF2_SCRIPTWAIT legacy direct-wait behavior, client-side
controllers and full module loading remain unconverted. This is a source and
regression self-audit, without real-map/native runtime equivalence validation.

### Legacy direct ScriptWait compatibility and scheduler review

Added the opt-in CompatSurface.LegacyScriptWaitDirect switch, corresponding to
the native COMPATF2_SCRIPTWAIT branch of PCD_SCRIPTWAITDIRECT in p_acs.cpp.
Only the direct opcode skips the pre-start wait. It still yields and checks for
a running numbered target on its next scheduled evaluation. Stack ScriptWait
retains the default start-then-finish behavior. Always instances remain excluded.
The flag is supplied through AuthoritySimulation.Start; native compatibility
database/MAPINFO selection is not automatically loaded.

Ten regressions cover the direct/stack and enabled/disabled matrix, delayed and
suspended targets, a target started between ticks, always-instance exclusion,
wait-state checksum differences and truncated operands. All 1,945 managed tests
pass, including the existing invasion regression suite. This closes the prior
legacy direct-wait opcode behavior gap when the switch is explicitly configured.

The scheduler audit also reviewed DACSThinker::Tick and DLevelScript::Link,
PutLast and PutFirst. Native code traverses mutable links, starts scripts at the
front by default and reorders waiters. The managed VM still snapshots its list
in insertion order; changing this requires coordinated execution/control timing
work rather than just moving a waiter in the list. Native ordering, Hexen startup
ordering, named waits and real-map validation remain open. No claim is made that
the original invasion timer/round/enemy symptom is resolved by this change.

### ACS TagWait for managed sector movers

Converted wordcode TagWait (61) and TagWaitDirect (62). Source review of the
native opcode handlers and SCRIPT_TagWait in p_acs.cpp shows that either floor
or ceiling ownership blocks a tagged wait, including paused movers. The managed
VM now checks sector tags against its live motion list, yields even when no
matching mover exists, and resumes when that list no longer contains a matching
sector. Unrelated tags do not block it. External suspend/resume discards the old
wait state, and the saved tag/presence are included in the checksum.

Twelve cases cover stack/direct waits, floor and ceiling completion, absent tags,
both planes moving together, unrelated motion, paused ceiling removal, map-array
reload exclusion, resume, malformed operands and saved-tag checksum differences.
The paused-ceiling test initially supplied removal mode in the wrong argument;
correcting the fixture to use Arg1 matched the existing special contract. All
1,957 managed tests then passed, including previous invasion regressions.

This converts waits for the current managed motion representation, not all native
floor/ceiling thinkers. Completed stair records retained until group cleanup,
multi-tag sectors, unconverted movers and native thinker ordering still need
parity work. Managed simulation advances movers before ACS; exact native tick
ordering and representative real-map validation remain unverified. PolyWait and
named script waits remain unconverted. Phases 1–3 are still incomplete.

### Completed stair steps release floor ownership

The follow-up TagWait audit found that completed stair records were serving two
different roles: active floor ownership and the chain's stair retrigger lock.
Native DFloor completion in mapthinkers/a_floor.cpp clears floordata before
retaining/updating stairlock. Ordinary floor starts check PlaneMoving, whereas
stair starts also check stairlock. The managed retained Completed records now
block only stair reconstruction. TagWait and both managed ordinary floor start
paths ignore them, while still respecting any new active mover on that sector.

Six regressions cover stack/direct TagWait release before the chain finishes,
ordinary map and internal floor activation after step completion, a replacement
floor mover continuing to block TagWait, rejection while a step is still moving,
and retention/eventual release of the stair chain lock. All 1,963 managed tests
pass, including existing mover, ACS and invasion regressions.

This closes the completed-stair ownership gap recorded in the preceding audit.
Stair traversal/reset variants, compatibility-specific lock behavior, advanced
geometry and exact native thinker scheduling still need conversion or validation.
The checks are source review and managed regressions, not native runtime parity
or confirmation of the original invasion round/timer/enemy symptom.

### Stair traversal honors declared two-sided lines

Source review of EV_BuildStairs in mapthinkers/a_floor.cpp found that native
traversal requires ML_TWOSIDED before checking front/back sectors and texture.
Managed traversal previously checked only valid side references. It now also
requires LevelLine.TwoSidedFlag (4). UDMF LevelBuilder now preserves the parsed
twosided property in Flags; binary formats already copy linedef flags.

Three new regression cases cover a back-side reference without the flag and
both values through UDMF text parsing, level construction and stair activation.
Existing synthetic stair fixtures now explicitly declare two-sided connectivity.
The new UDMF fixture initially omitted player spawn eligibility flags; adding
those corrected the fixture. All 1,966 managed tests pass, including previous
stair ownership, TagWait and invasion coverage.

This closes the declared-connectivity gap in the converted stair traversal.
Other geometry consumers still use their existing side-reference rules; this
does not certify malformed-map handling across the engine. Stair sync/reset,
special-sector traversal, compatibility options, slopes and real-map/native
runtime parity remain unfinished. No native files or dependencies changed.

### Synchronized Doom stair map specials

Added extended map specials 271/272 for upward/downward synchronized Doom stairs
with reset zero. Source review of LS_Stairs_BuildUpDoomSync/DownDoomSync and
EV_BuildStairs confirms that later steps use base speed multiplied by their
signed travel distance divided by the signed step size. The seed retains base
speed. This lets unobstructed steps with uneven initial heights finish together.
The unused fifth argument is ignored, matching the native handlers.

Stair construction now stages its motion records before committing. A sync chain
requiring a negative computed speed rejects without creating any movers: the
managed mover lacks the native explicit direction semantics needed for that
case. Pending motions participate in busy checks so branches and overlapping
tagged seeds retain the existing exclusion behavior. Ordinary stair regression
coverage remains passing after this refactor.

Ten new cases cover upward/downward chains with level and uneven initial floors,
repeat/busy activation, unsupported resets, atomic reversed-distance rejection
and a step already at its target. All 1,976 managed tests pass. This is partial
sync conversion: reset, reversed-distance semantics, ceiling-limited/obstructed
timing, special-sector traversal, compatibility options and native runtime
comparison remain unfinished. It does not certify invasion synchronization or
complete phases 1–3.

### Stair reset countdown and return

Converted nonnegative reset arguments for map stair specials 217/270/273 with
delay zero and synchronized specials 271/272. Native DFloor::Tick decrements
the reset countdown from activation, before motion, and reverses toward the
original floor when it reaches zero. Managed stairs now follow that state
sequence, including reversal before construction finishes. Built steps awaiting
reset retain floor ownership, blocking ordinary floor starts and TagWait. They
release ownership after the return finishes; chain locks retain their existing
group cleanup behavior. Countdown and waiting state are checksum inputs.

Nine new cases cover all five specials, upward/downward early reversal,
countdown checksum differences before positions diverge, and TagWait remaining
blocked through the built/waiting/return states. The existing negative-reset
rejection tests remain. All 1,985 managed tests pass, including invasion coverage.

This supersedes the preceding audit's unsupported nonnegative reset restriction.
Negative reset values still reject atomically. Per-step delay, native exact
destination-crossing timing, obstructed/ceiling-limited movement, reversed sync
distances, advanced geometry and real-map/native runtime comparisons remain
unfinished. This source/regression audit does not establish complete stair or
invasion parity, and phases 1–3 remain incomplete.

### Per-step stair delays and reset interaction

Converted nonnegative delay arguments for Doom stair specials 217/270/273.
EV_BuildStairs initializes the step interval by truncating step height divided by
base speed; DFloor::Tick schedules a pause after that many active movement ticks.
The managed mover now tracks that interval, its countdown and pause ticks, and
includes them in the checksum. A sub-tick interval truncates to zero and does
not schedule pauses. Negative delays and intervals beyond the managed integer
range reject before creating motion records.

Reset countdowns run before pause handling. If a reset expires during a pause,
the transition tick still returns without moving; subsequent return ticks skip
the old construction pause machinery, matching the native state branch. Seven
new regressions cover all three specials, upward/downward reset during pause,
sub-tick intervals and negative-delay rejection. All 1,992 managed tests pass,
including earlier stair/ACS and invasion cases.

This closes the prior per-step-delay restriction for these map specials. Exact
native destination-crossing timing, blocked and ceiling-limited motion, native
compatibility variants, special-sector traversal and runtime comparisons remain
open. The source/regression audit is not full native parity validation; phases
1–3 and real-map confirmation of invasion synchronization remain incomplete.

### Blocked stair arrival and mover completion

Review of sector_t::MoveFloor in mapthinkers/dsectoreffect.cpp confirmed inclusive
destination comparisons for unobstructed flat planes. It also exposed a mismatch:
native destination-clamped upward moves report pastdest even when obstruction
rolls the floor back. Managed stairs previously kept retrying that endpoint.
Stairs now retain the arrival result separately from the applied floor position,
allowing completion or reset-wait state after rollback. Intermediate obstruction
still retries. Later crushing steps retain exact arrivals but roll back blocked
overshoots, matching the native stopMoving condition.

Nine cases cover blocked seed endpoints with/without crushing, exact versus
overshooting arrival, intermediate blockage and release, reset after a rolled-back
build endpoint, and later crushing steps. The latter fixture was corrected to
establish actor sector membership through geometry and a simulation tick rather
than assigning the read-only sector property. All 2,001 managed tests pass.

The behavior change is restricted to stairs; other floor/lift endpoint handling
still needs its own parity review. Native actor clipping, attached sectors,
downward obstructions, slopes, compatibility variants and real-map comparisons
remain outside this validation. This closes the audited upward stair endpoint
case, not full mover or invasion parity. Phases 1–3 remain incomplete.

### Ordinary floor and returning lift endpoint obstruction

Extended the upward endpoint correction to ordinary floor and lift motions.
Native MoveFloor reports pastdest on a destination-clamped attempt even after
obstruction rollback; DPlat::Tick distinguishes this from an intermediate
crushed result. Managed floors now release ownership at such endpoints, and
returning down-wait-up-stay lifts finish instead of reversing. Intermediate
noncrushing floor obstruction still retries, and intermediate returning lift
obstruction still reverses. Crushing floors retain exact blocked arrivals but
roll back blocked overshoots before finishing.

Six new regressions cover exact/overshooting ordinary floor endpoints, intermediate
obstruction and recovery, returning lift completion, and crushing exact versus
fractional overshooting targets. Existing intermediate lift reversal and stair
regressions remain passing. All 2,007 managed tests pass, including invasion cases.
Source review used MoveFloor, DFloor::Tick and DPlat::Tick in native mapthinkers.

This closes the preceding audit's upward ordinary floor/lift endpoint review gap.
Downward obstruction, actor clipping/stacking, attached planes, unconverted mover
types, compatibility behavior and real-map/runtime comparisons remain unfinished.
It is a source and regression self-audit, not full native or invasion parity.

### Door closing endpoint obstruction

Reviewed MoveCeiling in dsectoreffect.cpp and the closing branch of DDoor::Tick
in a_doors.cpp. A blocked destination-clamped close reports pastdest after
rollback, and native doorRaise releases its ceiling thinker. The managed door
previously reopened for every closing obstruction. It now finishes at blocked
endpoints while retaining the rolled-back ceiling height. Obstruction before
the destination still reopens and retains ownership.

Three regressions cover exact and overshooting blocked close endpoints,
non-damaging rollback, subsequent reactivation, and intermediate obstruction
retaining the door mover. All 2,010 managed tests pass, including previous
door, floor, stair, ACS and invasion coverage.

This addresses the represented raise/wait/close door's endpoint behavior.
Unconverted door types, opening obstruction through attached geometry, native
actor clipping and sound/light effects still need work. Real-map/native runtime
parity and original invasion synchronization remain unverified; phases 1–3 are
not complete. This remains a source/regression self-audit.

### Extended Door_Close map special

Converted extended map special 10 with light tag zero. LS_Door_Close and
DDoor::Tick in the native sources distinguish close-only doors from ordinary
raise/wait/close doors: intermediate obstruction retains the closing direction,
while destination-clamped obstruction completes after rollback. The managed
motion now records CloseOnly and hashes it. Closing can start without neighboring
sectors, targets the sector floor, respects ceiling ownership and uses speed/8.

Six regressions cover intermediate obstruction and recovery, exact/overshooting
blocked endpoints, no-neighbor closure, repeat/use rules, zero speed and unsupported
lighting rejection. All 2,016 managed tests pass, including previous door/mover,
ACS and invasion coverage. Nonzero light tags reject without consuming the line.

This adds the current flat-sector map activation path. Door-linked lighting,
close-wait-open, native manual reactivation rules, script dispatch for this special,
sound effects, attached geometry and runtime parity remain incomplete. Native
behavior for zero/invalid speed is not claimed. Phases 1–3 and real-map invasion
validation remain open.

### Extended Door_CloseWaitOpen and ACS wait integration

Converted extended map special 249 for positive supported delays and light tag
zero. Native LS_Door_CloseWaitOpen converts eighth-second units using delay*35/8;
DDoor stores the original ceiling as the reopening destination. Managed doors
now close, retain ceiling ownership for the converted wait, then reopen to that
original height. Intermediate closing obstruction reverses immediately; blocked
arrival rolls back and enters the wait. Reopening completion releases ownership.
The new motion mode participates in the simulation checksum.

Nine regressions cover delay conversion/truncation, original-height restoration,
both obstruction paths, invalid delay/lighting rejection, reactivation and ACS
TagWait blocking throughout closing, waiting and reopening. All 2,025 managed
tests pass, including existing invasion regressions. Source review used the
native special handler and DDoor constructor/Tick state transitions.

Zero/negative delays and values exceeding the checked multiplication range reject
without changing the map; native behavior for those inputs is not reproduced.
Door-linked lighting, script dispatch, manual reactivation, sound/attached-plane
behavior and real-map/native runtime comparisons remain incomplete. This closes
the earlier close-wait-open map-special gap for the stated subset, not phases
1–3 or the original invasion synchronization validation.

### ACS dispatch for converted closing door specials

Connected door specials 10/249 to direct and stack ACS LSPEC dispatch, including
result-returning forms. Map lines and scripts share ExecuteDoorSpecial so speed,
delay, unsupported lighting and busy-ceiling results use the same checks. Script
calls with tag zero reject because the current VM does not retain activation-line
context; map-line manual targeting still uses its supplied line. Truncated direct
operands terminate before dispatch. Unsupported options return false rather than
falling through to a differently numbered legacy internal action.

Eleven regressions cover direct/stack calls for both specials followed by TagWait,
standard/extended result forms, missing tags, tag zero, invalid speed/lighting
and truncated bytecode. All 2,036 managed tests pass, including prior map-line,
mover, ACS and invasion regressions. The implementation reuses the native-derived
door handlers and previously reviewed LSPEC argument/result ordering.

This closes script dispatch for these two converted door specials. Open/raise
script dispatch, activation-line/actor context, lighting, manual reactivation,
native scheduler parity and real-map comparisons remain unfinished. Neither full
phases 1–3 completion nor original invasion synchronization is certified.

### ACS Door_Open and Door_Raise dispatch

Connected specials 11/12 to the shared door helper used by map lines and ACS
direct/stack/result calls. Source review of LS_Door_Open/Raise confirms speed/8,
raise delay in tics and the differing light-tag argument positions. Both entry
paths now reject nonzero lighting tags instead of map activation silently ignoring
them; negative raise delays also reject without starting a mover or consuming a
line. Current tag-zero script context restrictions remain.

Seven new cases cover direct/stack opening and raising, argument forwarding,
positive-delay timing, TagWait completion, and consistent map/script rejection
of unsupported lighting or negative delay. All 2,043 managed tests pass, including
previous door/mover, ACS and invasion coverage.

This closes the previous open/raise script-dispatch gap for the represented
subset. The existing zero-delay raise behavior remains a managed compatibility
choice and is not certified against native countdown underflow behavior. Native
manual reactivation, door lighting, actor/line context, special geometry, sound
and runtime comparisons remain unfinished. Phases 1–3 are incomplete.

### ACS dispatch for converted Doom stair specials

Connected specials 217 and 270–273 to ACS direct/stack/result dispatch and shared
ExecuteStairSpecial with map activation. Native handler review confirmed the
different delay/reset argument positions for ordinary and synchronized stairs.
The shared path preserves those positions, direction, crushing and sync options.
Tag-zero script calls reject because activation-line context is not retained;
map-line manual targeting remains available. Failed/truncated calls do not
partially create movers.

Sixteen cases cover direct/stack calls for all five specials with reset and
TagWait, result forms, absent/manual tags, zero speed and truncated operands.
The initial delayed-stair test expectation missed the reset-during-pause tick;
correcting that expectation matched the previously audited native transition.
All 2,059 managed tests pass, including prior map stair, door, ACS and invasion
coverage. Existing map-line activation and repeat handling are retained.

This connects the converted stair subset to scripts, not complete ACS or native
stair parity. Activation context, reversed-distance sync cases, compatibility
variants, special-sector traversal, advanced geometry and runtime comparisons
remain open. Phases 1–3 and original invasion symptom validation remain incomplete.

### ACS dispatch for selected floors and lifts

Connected floor specials 20/21/23/25 and lift 62 to direct/stack/result ACS calls,
sharing ExecuteFloorSpecial with map-line activation. Native handler review
confirmed speed/8, value/neighbor targets, lift delay in tics and its eight-unit
lip. The shared helper rejects unconverted texture/type-change and crushing
options instead of silently ignoring them, and rejects negative lift delays.
Tag-zero script requests still need unconverted activation-line context.

Fourteen cases cover direct/stack value moves and lifts, neighbor targets,
TagWait through completion/return, failure results for unsupported arguments and
truncated direct operands. All 2,073 managed tests pass, including prior mover,
ACS and invasion regressions. Existing map activation/repeat handling remains.

This connects the represented floor/lift subset to ACS. Texture/type transfer,
the rejected crushing variants, full activation context, native zero-delay lift
behavior, advanced geometry and real-map/native comparisons remain unfinished.
Other native floor/platform specials are still unconverted. Phases 1–3 and the
original invasion synchronization validation remain incomplete.

### Crushing arguments for raising floors

Converted the crushing argument for specials 23/25 through their shared map/ACS
helper. Native LS_Floor_RaiseByValue/RaiseToNearest use CRUSH(a), enabling positive
damage and disabling nonpositive values, and request Hexen crushing semantics.
The managed mover now receives that damage plus StopOnCrush: intermediate
obstruction damages on the existing four-tic cadence but rolls the floor back
until space clears. Existing destination-arrival handling remains in force.

Eight new cases cover both specials through map and direct ACS activation,
requested damage amounts, rollback/recovery, and zero/negative damage disabling.
The earlier unsupported-argument cases now test texture/type changes rather than
the newly supported crush arguments. All 2,081 managed tests pass, including
previous mover, ACS and invasion regressions.

This closes the previous rejection of those two crushing arguments. Native actor
clipping, corpses/items, stacked actors, exact damage effects, texture/type
transfer, advanced geometry and real-map/native runtime comparisons remain open.
It does not certify complete mover or invasion parity; phases 1–3 remain incomplete.

### ACS ceiling value moves and stop controls

Connected specials 40/41/44 to shared map/ACS dispatch. Native handler review
confirmed speed/8, height/change/crush argument positions and explicit stop mode
1 (pause) versus 2 (remove). Tag-zero script requests still reject without
activation-line context. Other map ceiling specials keep their existing paths.

Ten new cases cover direct/stack raise/lower moves with TagWait, explicit pause
retaining the wait, removal releasing it, result values, missing/manual tags and
unsupported texture/type-change rejection. All 2,091 managed tests pass,
including prior ceiling, mover, ACS and invasion regressions.

Native default stop mode depends on gameinfo.gametype (Hexen removes; others
pause). The managed default still pauses; map format alone cannot establish game
type. That compatibility gap remains open, along with native stop target/result
edge cases, unconverted ceiling/crusher script dispatch, texture/type transfer,
activation context, advanced geometry and native runtime comparison. This is a
source/regression self-audit; phases 1–3 and invasion runtime validation remain
incomplete.

### Ceiling stop targeting correction and unconditional stop

Native EV_CeilingCrushStop in a_ceiling.cpp filters DCeiling thinkers by their
creation tag and nonzero direction. Corrected managed special 44 accordingly:
paused movers no longer report success or get removed, and matching uses the
motion tag rather than current sector tags. Tag zero is valid for this registry
lookup and no longer requires script activation-line context.

Added Ceiling_Stop (276) to map/ACS dispatch, following EV_StopCeiling: target
sectors, remove their ceiling ownership including paused ceiling movers and doors,
preserve floor movers, and succeed even when no target exists. Existing tests
that incorrectly used CrushStop to remove paused movers now use Ceiling_Stop;
their TagWait release assertions remain. Tag-zero script Ceiling_Stop still
rejects without activation-line context.

Four new cases cover paused-mover exclusion, creation-tag matching including
manual tag zero, unconditional removal of doors without stopping floors, and
missing-target success/one-shot consumption. All 2,095 managed tests pass,
including the corrected ACS/TagWait cases and prior invasion regressions.

This corrects the earlier pause/remove audit's incomplete native targeting model.
Game-type-dependent default mode, native resume edge cases, unconverted ceiling
thinkers, activation context and real-map/native runtime validation remain open.
Phases 1–3 and original invasion synchronization validation remain incomplete.

### Ceiling creation tags and native resume ordering

Follow-up review of EV_DoCeiling/CreateCeiling/ActivateInStasisCeiling found that
manual ceiling activation replaces tag zero with sectorIndex | 0x1000000.
Managed ceiling movers now store that derived creation tag. Tagged looping
crusher activation resumes paused ceiling movers by creation tag before scanning
sector targets, preserving the original direction, speed, target and mode.
It can therefore resume a mover even when no sector has that tag.

Manual activation also resumes by its derived tag before attempting creation.
As in the native manual branch, resuming an existing mover can return false
because CreateCeiling then refuses a second mover. Other tagged non-looping
requests do not resume paused movers. The earlier tag-zero stop test was corrected
to use the native derived tag rather than assuming a stored zero.

Four new cases cover manual resume/result behavior, mismatched creation tags,
tagged resume preserving movement, and resume without a matching sector tag.
All 2,099 managed tests pass, including prior mover/ACS/invasion coverage.
This corrects the preceding audit's incomplete manual-tag model. Invalid argument
edge cases, game-specific stop defaults, full activation context and native
runtime comparisons remain open; phases 1–3 are still incomplete.

### ACS crusher activation and lifecycle

ACS line-special dispatch now shares map activation for Ceiling_CrushAndRaise
(42), Ceiling_LowerAndCrush (43), and Ceiling_CrushRaiseAndStay (45). Source
review against native p_lnspec.cpp preserves their existing managed speed,
return-speed, crush-mode and looping settings. This closes the dispatch gap for
these three converted movers, including result-returning script calls.

Eight new regression cases cover direct and stack activation, lower-only and
return-and-stay completion, looping crusher pause/resume with original settings,
TagWait blocking until removal, and success results from both result opcodes.
All 2,107 managed tests pass. This does not establish full native crusher parity:
activation-line context, clipping/damage details, invalid-argument edge cases,
game-specific stop defaults and remaining ceiling variants still need work.
Phases 1–3 and real-runtime invasion synchronization validation remain incomplete.

### ACS distance crushers and ceiling geometry targets

Shared map/ACS dispatch now includes Ceiling_CrushAndRaiseSilentDist (104),
Ceiling_CrushAndRaiseDist (168), Ceiling_LowerToFloor (254), and
Ceiling_RaiseToHighest (262). Review against src/playsim/p_lnspec.cpp confirms
argument placement: distance crushers use argument 1 for clearance and argument
2 for equal down/up speeds; LowerToFloor uses argument 4 for clearance and
argument 3 for crush damage. Unsupported ceiling texture/type changes still
return false without starting a mover. The existing map behavior is retained.

Twelve added regression cases exercise direct/stack calls, requested crusher
clearance and equal return speed, continuing loops, floor clearance and TagWait
release, highest neighboring ceiling targets, and rejected change flags through
both result opcodes. All 2,119 managed tests pass. The initial test compile caught
incorrect fixture property names; these were corrected before the full run.

This closes these four ACS dispatch gaps only. Silent-versus-audible native
sound behavior is not implemented by this mover layer. Native clipping/damage,
activation context, invalid/no-op target edge cases, remaining ceiling variants
and real-map comparison remain open. Phases 1–3 remain incomplete.

### Separate-speed crushers and crush-and-stay slowdown audit

Map and ACS activation now implement Ceiling_LowerAndCrushDist (97),
Ceiling_CrushRaiseAndStayA (195), Ceiling_CrushAndRaiseA (196),
Ceiling_CrushAndRaiseSilentA (197), and Ceiling_CrushRaiseAndStaySilA (255).
Review of src/playsim/p_lnspec.cpp and mapthinkers/a_ceiling.cpp establishes
independent down/up speeds for the A variants and their zero floor clearance,
unlike the eight-unit clearance of specials 42/43/45. Special 97 uses its
explicit distance and finishes after lowering; 195/255 return and finish;
196/197 continue looping.

The native thinker only applies slowdown after obstruction to looping and
lower-only crusher types. Managed special 45 incorrectly enabled slowdown for
mode 3; it now maintains its speed, as do new 195/255 movers. This supersedes
the earlier crusher activation audit's implicit claim that all of special 45's
existing crush-mode behavior was preserved correctly.

Twenty-one new regression cases cover direct ACS, stack ACS and map activation,
distinct down/up speeds, clearance, completion versus looping, TagWait release,
and obstructed mode-3 slowdown differences with an actor in the target sector.
All 2,140 tests pass. Game-specific default crush modes, silent/audible sound
sequences, complete native clipping/damage, activation context and real-map
validation remain open. These additions do not complete phases 1–3 or certify
the original invasion synchronization symptom in a running game.

### Eight-times-distance ceiling movers

Map and ACS dispatch now support Ceiling_RaiseByValueTimes8 (198) and
Ceiling_LowerByValueTimes8 (199). Native p_lnspec.cpp scales argument 2 by eight
but leaves the speed in eighth-units per tic. Both handlers explicitly pass
non-crushing damage (-1), even though the lower variant's comment mentions a
crush argument. The managed handlers therefore ignore argument 4, preserving
the executable native behavior. Texture/type change requests remain unsupported.

Fourteen regressions cover direct/stack ACS and map activation, scaled travel,
one-shot map consumption, TagWait timing, negative and overflowing distances,
unsupported changes, and blocked lowering that causes no damage then completes
after the obstruction clears. Large script distances are rejected before integer
multiplication can wrap; this is a managed validation choice, not a claim of
native behavior for out-of-range inputs. All 2,154 managed tests pass.

Zero-distance activation, signed negative-distance native semantics, general
height limits, complete clipping, texture changes and real-map parity remain
open alongside the previously recorded completion gates. Phases 1–3 are not
complete, and original invasion synchronization remains runtime-unverified.

### Nearest neighboring ceiling targets

Map and ACS dispatch now support Ceiling_RaiseToNearest (252) and
Ceiling_LowerToNearest (264). Source review of p_lnspec.cpp, a_ceiling.cpp and
p_sectors.cpp establishes strict directional selection: the smallest higher
neighbor for raising and largest lower neighbor for lowering, excluding equal
heights. Managed selection preserves fractional heights. Lowering takes optional
crush damage from argument 3; raising remains non-crushing. Change flags are
rejected because ceiling texture/type changes remain unconverted.

Twelve regressions cover map/direct/stack activation, closest-neighbor selection
among higher/lower/equal candidates, fractional travel, TagWait completion,
unsupported change results, absence of a suitable directional neighbor, and
damaging versus non-damaging obstruction. The obstruction fixture initially
failed its sector-membership assertion because its lines had no polygon; adding
actual room boundaries corrected the fixture. All 2,166 tests now pass.

The no-target managed path currently declines to create a mover, unlike native
creation of a no-travel thinker. Native lower-to-nearest also has a distinct
floor-height fallback for sectors with no lines. Those result/ownership/timing
edge cases are not converted in this pass. Slopes, linked-sector exclusions,
full clipping, height limits and runtime parity remain open; phases 1–3 and
original invasion synchronization validation remain incomplete.

### Nearest-ceiling no-travel ownership and results

The no-directional-neighbor gap recorded above is now corrected for specials
252/264 on sectors with boundaries. Native CreateCeiling creates and owns a
ceiling thinker even when the chosen height equals the current height. Managed
nearest-target activation now likewise succeeds, occupies the ceiling, and
finishes on the first mover tick. One-shot map lines are consumed and ACS result
forms return success. Pausing before that tick retains ownership and blocks
TagWait until completion or explicit removal.

Eight added cases cover both result opcodes, competing mover rejection and
subsequent ownership release, one-shot consumption, TagWait timing, and paused
no-travel mover removal. All 2,174 managed tests pass. Existing nearest-target
movement and other mover/ACS/invasion regressions remain passing.

Further source review qualifies the preceding no-lines note: although
FindNextLowestCeiling has a floor-height fallback, CreateCeiling dereferences
the sector's first line before calling it. Therefore that helper fallback alone
does not establish valid native activation behavior for a boundaryless sector.
Such synthetic/invalid geometry remains outside runtime parity claims. No-travel
behavior of other ceiling types, full native scheduling/clipping, linked sectors,
slopes and real-game invasion validation remain open. Phases 1–3 are incomplete.

### Zero-travel ceiling creation and crusher cycles

Native CreateCeiling creates a thinker before choosing its target and does not
reject an equal destination. Managed ceiling creation now retains equal-target
movers for all converted ceiling types, extending the preceding nearest-target
fix. Ordinary zero-travel requests succeed, own the plane, block competing
ceiling movers, and release ownership on their first tick. One-shot map
activations are consumed and TagWait remains blocked until that tick.

Zero-travel crushers retain their normal leg lifecycle: lower-only completes
on the first tick, crush-and-stay changes direction then completes on the second,
and looping crushers continue alternating legs until stopped. Thirteen added
regressions cover zero-distance value/times-eight requests, equal floor-gap and
highest-neighbor targets, ACS results, ownership, map consumption, TagWait and
the three crusher lifecycles. All 2,187 managed tests pass.

This closes the previously recorded equal-target creation gap for converted
ceiling types with positive speed. Zero/negative speeds, reversed crusher
destinations, native collision side effects for already-overlapping actors,
full scheduler ordering and real-runtime parity remain outside this pass.
Phases 1–3 and original invasion synchronization validation remain incomplete.

### Instant ceiling raise and lower

Map and ACS dispatch now support Ceiling_LowerInstant (193) and
Ceiling_RaiseInstant (194). Native p_lnspec.cpp scales the height argument by
eight and ignores argument 1. Native a_ceiling.cpp sets movement speed to that
distance, so movement happens in one thinker tick rather than inside activation.
The managed instant target types follow that rule, including a zero-speed,
zero-distance thinker that completes on its first tick. This exception does not
relax speed validation for ordinary movers. Lowering uses argument 4 as crush
damage; raising remains non-crushing. Unsupported changes, negative distances
and multiplication overflow are rejected.

Review of sector_t::MoveCeiling in dsectoreffect.cpp confirms destination-clamped
blocked moves complete even when rolled back. Eleven new cases cover map,
direct and stack activation, ignored speed arguments, delayed first-tick motion,
TagWait release, zero-distance ownership, non-crushing rollback, exact-distance
crushing, and rollback with damage after an overshoot past the floor clamp.
All 2,198 managed tests pass.

Native interpolation suppression, sound sequences, texture/type changes, full
actor/attached-sector clipping, negative-distance semantics and real-runtime
comparison remain unconverted or unverified. Phases 1–3 and original invasion
synchronization validation remain incomplete.

### Absolute-height ceiling movement

Map and ACS activation now support Ceiling_MoveToValue (47), its Times8 variant
(69), and Ceiling_MoveToValueAndCrush (280). Native p_lnspec.cpp and a_ceiling.cpp
specify world-height destinations, with direction chosen from the current height.
The first two handlers multiply the signed height by the optional negative flag
(any nonzero value); 69 also scales by eight. Managed arithmetic uses a wider
intermediate and rejects values outside the integer argument range. Negative
absolute heights are accepted without relaxing distance checks on other movers.

Special 280 uses argument 3 for damage and argument 4 for crush mode. The native
thinker excludes absolute movers from slowdown handling, so mode 3 does not
reduce their speed; explicit mode 2 stops against an intermediate obstruction.
Eighteen regressions cover map/direct/stack activation, upward/downward signed
targets, sign inversion, scaling, TagWait completion, overflow and unsupported
changes, and obstructed movement in all three explicit crush modes. All 2,216
managed tests pass.

Game-specific default modes, ceiling texture changes, full native clipping,
out-of-range height policy, zero-speed movement and runtime comparison remain
open. The existing floor/ceiling height clamps still apply. These additions do
not complete phases 1–3 or validate original invasion synchronization in-game.

### Zero-speed ceiling ownership

Review of SPEED in p_lnspec.cpp, DCeiling construction and sector_t::MoveCeiling
shows zero speed is accepted by native ceiling creation. Converted managed
ceilings now accept zero down/up speeds too. A mover away from its destination
retains ownership without advancing, blocks competing ceiling movement and
keeps TagWait blocked. A zero return speed permits a crusher's down leg but
holds it at the floor afterward. At an already-equal target, zero speed still
completes the leg on its first tick. Negative speeds remain rejected.

Ten regressions cover success results across value, absolute and crusher
variants, ownership and explicit removal, zero-return-speed map activation,
and zero-speed/equal-target completion. All 2,226 managed tests pass. This
supersedes the earlier audit's zero-speed rejection limitation for converted
ceiling movers, including the earlier instant-only exception.

Already-overlapping actors can trigger native collision processing even with
zero displacement; that behavior is not implemented by the managed ceiling
tick and remains a parity gap. Negative-speed semantics, game-specific defaults,
remaining target variants and native runtime validation also remain open.
Phases 1–3 and original invasion synchronization validation remain incomplete.

### Crusher mode fallback audit

Native CRUSHTYPE in p_lnspec.cpp treats every argument outside 1..3 as a default
request. Managed dispatch previously recognized only zero for default slowdown,
so negative or greater-than-three script arguments incorrectly selected normal
speed. A shared mode helper now applies the existing non-Hexen fallback to all
out-of-range values while preserving explicit Doom, Hexen and slowdown modes.
The native speed predicates and exclusions for crush-and-stay/absolute movers
remain in place.

Seventeen new regressions cover negative and greater-than-three values across
six affected specials, all explicit modes, zero fallback, and a faster mover
whose default must not slow. All 2,243 managed tests pass. The simulation still
does not model native game-family selection: default mode in Hexen must use
Hexen crushing, independent of map format, and remains a conversion gap.
This pass fixes fallback range handling only; full clipping, remaining variants,
phases 1–3 and real-runtime invasion validation remain incomplete.

### Explicit Hexen ceiling defaults

CompatSurface.HexenCrushDefaults now selects native Hexen default ceiling
crushing and CrushStop removal behavior through AuthoritySimulation.Start's
existing compatibility argument. Map format does not select this flag. Explicit
crush modes 1..3 retain precedence; out-of-range values use the selected default.
CrushStop mode 1 always pauses, mode 2 always removes, and other values remove
when the flag is enabled. Shared dispatch applies these rules to the converted
crusher and absolute-crushing variants.

Compatibility flags now participate in the simulation checksum because they
affect future behavior. This changes checksum values, including for default
configuration; callers comparing stored checksums must use the same version and
configuration. Thirteen regressions cover opt-in/default behavior on the same
Hexen-format fixture, explicit overrides, out-of-range defaults, stop removal
versus pause, and checksum determinism/configuration distinction. All 2,256
managed tests pass.

This implements selectable ceiling defaults, not game-family discovery or full
Hexen gameplay. Server/resource integration must still select the flag from the
actual game configuration. Native collision details, remaining specials and
runtime validation remain open. Phases 1–3 and original invasion synchronization
validation are still incomplete.

### Dedicated-server compatibility configuration

DedicatedServerOptions now exposes Compatibility and the host passes it into
AuthoritySimulation.Start alongside spawn options. The command-line flag
--hexen-crush-defaults selects the new ceiling default rules without inferring
game family from map format. Repeated flags are idempotent. Usage text and the
README describe the limited scope; programmatic hosts may combine compatibility
flags without the host dropping them.

Four integration cases boot generated map data through the real dedicated-server
constructor: absent/enabled/repeated command-line flags and combined programmatic
flags. Booted configuration and deterministic reference checksums agree. All
2,260 managed tests pass, including 55 server tests and prior invasion coverage.
This closes the manual server configuration gap from the preceding audit.
Automatic resource-based selection, client negotiation/persistence of these
rules, full Hexen gameplay and native runtime comparisons remain open. Phases
1–3 and original invasion synchronization validation are still incomplete.

### Lowest ceiling target and fixed movement direction

Ceiling_LowerToLowest (253) now supports map and ACS activation, selecting the
minimum neighboring ceiling rather than the nearest lower height. It uses
argument 3 for optional crush damage and rejects unsupported change flags.
Review of a_ceiling.cpp and sector_t::MoveCeiling also found that native movers
retain their assigned direction. SectorMotion now stores CeilingDirection,
included in its simulation checksum; crusher return legs move upward and
absolute movers choose direction at creation.

This corrects the existing highest-neighbor mover when all neighbors are lower:
native upward movement clamps immediately to a destination already below it.
Likewise the new lower-to-lowest mover reaches a higher destination immediately
when every neighbor is higher. Seven regressions cover map/direct/stack calls,
fractional minimum selection, TagWait, both opposite-side target cases and
crushing versus non-crushing lowering. All 2,267 managed tests pass.

For no neighboring sector, managed lowest selection keeps the current height;
native surrounding-height helpers can return extreme sentinel values. That
edge case, opposite-direction collision side effects, linked/attached sectors,
slopes and native runtime validation remain open. Phases 1–3 and original
invasion synchronization validation are incomplete.

### Additional neighboring-height ceiling variants

Map and ACS activation now support Ceiling_LowerToHighestFloor (192),
Ceiling_RaiseToLowest (265), and Ceiling_RaiseToHighestFloor (266). Review of
p_lnspec.cpp, a_ceiling.cpp and FindHighestFloorSurrounding confirms neighboring
floor versus ceiling selection, assigned direction and argument placement.
Special 192 adds argument 4 as a gap and uses argument 3 for crush damage;
265/266 remain non-crushing. Unsupported ceiling change flags are rejected.

Fourteen regressions exercise all three through map/direct/stack activation,
fractional neighbor targets, floor-versus-ceiling selection, gap placement,
TagWait completion, opposite-side targets for raising, and change rejection.
All 2,281 managed tests pass. Existing explicit-direction motion and checksum
state are reused without adding a second mover implementation.

No-neighbor sentinel semantics, negative gap arguments, linked/attached sectors,
slopes, complete clipping and runtime parity remain open. Managed no-neighbor
floor selection falls back to the current sector floor. Phases 1–3 and original
invasion synchronization validation remain incomplete.

### Highest/floor instant-target ceiling specials

Map and ACS dispatch now implement Ceiling_ToHighestInstant (263) and
Ceiling_ToFloorInstant (267). Despite their names, native p_lnspec.cpp supplies
a fixed speed of two units per tic. The former assigns downward movement toward
the highest neighbor, and the latter upward movement toward the floor plus
argument 3. Targets opposite that direction clamp on the first tick; targets
along it travel at two units per tick. Unsupported change flags in argument 1
are rejected. Argument 2 supplies the native stored crush value.

The native upward thinker calls MoveCeiling without crush damage. Managed
ceiling ticks now omit damage on that direction too, including when the target
is below the current ceiling. Nine regressions cover map/direct/stack calls,
one-shot consumption, first-tick clamping, ordinary-direction travel, TagWait,
and a blocked floor-target move that rolls back and finishes without damage.
All 2,290 managed tests pass.

Interpolation, sound, texture/type changes, negative gaps, no-neighbor sentinel
behavior and full native collision/attached-sector handling remain incomplete.
This pass does not finish phases 1–3 or validate invasion synchronization in a
running native/managed comparison.

### Signed lowering gaps and crusher distances

Native special handlers pass signed gaps/distances directly into downward
ceiling targets. Managed validation now permits negative offsets for
Ceiling_LowerToHighestFloor (192), Ceiling_LowerToFloor (254), and distance
crusher specials 97/104/168. Neighbor-floor gaps may place the target below
that neighbor's floor; the moved ceiling remains clamped at its own sector floor.
Relative raise/lower distances and instant distances keep their existing checks.

Twelve regressions cover map/direct/stack negative floor gaps, native argument
placement, completion/TagWait, floor clamping, and lower-only versus returning
looping crushers with negative clearance. All 2,302 managed tests pass.

Negative gaps for upward Ceiling_ToFloorInstant (267) remain rejected: native
upward movement does not apply the downward floor clamp, while managed ceilings
currently clamp both directions. Floor-crossing behavior, moving-floor target
interactions, slopes, linked sectors and complete native clipping need further
conversion. Phases 1–3 and real-game invasion validation remain incomplete.

### Directional floor clamping and retained destinations

Native sector_t::MoveCeiling applies its flat-sector floor clamp on downward
movement only, using the floor at the current tick. Managed ceiling creation
now retains the requested destination rather than clamping it to the initial
floor, and only downward ticks apply the floor limit. This preserves a below-floor
destination when a concurrent floor mover later makes it reachable. Upward
thinkers may clamp directly to a target below their own floor, as native code
does; Ceiling_ToFloorInstant (267) therefore now accepts negative gaps.

Eight regressions cover direct/stack/map negative upward gaps, upward neighbor
targets below the current floor, a concurrently lowering floor, TagWait timing,
and blocked upward rollback without crush damage. All 2,310 managed tests pass.
This closes the preceding audit's unconditional floor-clamp and rejected-negative
267-gap limitations for the tested flat-sector cases.

Native compatibility flags that disable floor clamping, portal/linked-sector
rules, slopes, full clipping and renderer behavior for crossed planes remain
unconverted or unverified. The existing upper height limit remains. Phases 1–3
and original invasion synchronization validation are incomplete.

### Crusher clearance above the starting ceiling

Native CreateCeiling does not reject a crusher destination above its starting
height. With fixed direction now represented, the managed rejection is removed:
the downward leg clamps to that destination on its first tick. Lower-only
crushers finish there; return-and-stay crushers clamp back on the next tick;
looping crushers continue their leg lifecycle. This closes the previously
recorded reversed-crusher-destination creation gap for unobstructed flat sectors.

Ten regressions cover specials 42/43/45, direct/stack distance variants
97/104/168, TagWait behavior, one-shot map consumption and ownership release.
All 2,320 managed tests pass. Already-overlapping actor effects, attached-sector
collision, extreme heights, renderer behavior and real-runtime comparisons
remain outside this pass. Phases 1–3 and original invasion synchronization
validation remain incomplete.

### Floor lower-to-nearest activation

Floor_LowerToNearest (22) now supports map and ACS activation. Review of
p_lnspec.cpp and native floor creation confirms selection of the closest lower
neighbor, eighth-unit speed arguments and a non-crushing mover. Equal or higher
neighbors are excluded. Unsupported texture/type changes are rejected.
If no lower neighbor exists, this new target type still creates a no-travel
thinker, preserving success results and ownership until the first tick.

Six regressions cover map/direct/stack activation with competing fractional
neighbor heights, TagWait completion, both result opcodes for a no-travel mover,
competing floor rejection/release, and unsupported change handling. All 2,326
managed tests pass. The shared managed floor tick is reused.

Other floor types' no-travel behavior, zero/negative speeds, native floor
clipping/direction edge cases, slopes, linked sectors, texture changes and
real-runtime comparison remain open. Phases 1–3 and original invasion
synchronization validation remain incomplete.

### Eight-times-distance floor variants

Floor_RaiseByValueTimes8 (35) and Floor_LowerByValueTimes8 (36) now support map
and ACS activation. Native p_lnspec.cpp scales only argument 2's distance by
eight; speed remains eighth-units per tic. Raising uses argument 4 for crush
damage and stops at intermediate obstruction, while lowering is non-crushing.
Unsupported change flags, negative distances and overflowing multiplication
are rejected before starting a mover.

Sixteen added regressions cover map/direct/stack activation, scaled travel and
TagWait timing, rejected values/changes, positive crush damage and recovery once
an obstruction clears, and nonpositive damage arguments. All 2,342 managed
tests pass. The existing floor mover implementation handles the new dispatch.

Zero-distance ownership, zero/negative speeds, signed native distance edge
cases, texture/type changes, full clipping and real-runtime comparisons remain
open. Phases 1–3 and original invasion synchronization validation are incomplete.

### No-travel floor creation

Native floor creation allocates its thinker before target selection and does
not reject equal destinations. Converted non-lift map floor movers now likewise
succeed at an equal target, hold floor ownership until their first tick and
preserve TagWait ordering. Lift creation remains separate and unchanged.
An older regression that expected a Doom raise with no higher neighbor to fail
was corrected to assert successful one-shot consumption and first-tick release.

Ten new cases cover ACS success/ownership for value, lowest, next-higher and
times-eight floors, map consumption, and independent ceiling movement during
floor ownership. All 2,352 tests pass after correcting the old expectation.
The first full run also hit the previously observed five-second server input
timeout; its targeted rerun and subsequent full run passed without changing
or disabling the test. This remains an intermittent validation limitation.

Zero/negative floor speeds, lift no-travel lifecycle, native collision side
effects with already-overlapping actors, floor direction edge cases and real-map
parity remain open. Phases 1–3 and original invasion synchronization validation
remain incomplete.

### Zero-speed floor activation

Native floor creation accepts zero speed and retains a thinker. Managed
non-lift map floor creation now does the same: activation reports success,
competing floor movement is blocked, and TagWait remains pending while the
destination is unreached. A zero-speed mover already at its target completes
on its first tick. Negative speeds remain rejected, and lift speed validation
is unchanged pending its separate native lifecycle audit.

Nine regressions cover seven converted floor specials, result values, waiting
scripts, competing floor actions, independent ceiling movement and zero-speed
equal-target map consumption/release. All 2,361 managed tests pass.
Native collision side effects with no displacement, negative-speed semantics,
lift behavior, remaining floor direction/clipping rules and real-runtime
comparison remain open. Phases 1–3 and original invasion synchronization
validation remain incomplete.

### Instant floor raise/lower activation

Floor_LowerInstant (66) and Floor_RaiseInstant (67) now support map and ACS
activation. Native p_lnspec.cpp ignores argument 1 and scales argument 2 by eight;
native floor creation assigns that distance as speed. The shared managed floor
path therefore moves on its first tick, including normal ownership/TagWait
handling before that tick. Raising honors argument 4 crush damage and native
stop-on-crush behavior; lowering remains non-crushing.

Ten regressions cover map/direct/stack activation, ignored speed arguments,
first-tick travel, zero-distance ownership/release, and blocked instant raises
that roll back but complete, with and without damage. All 2,371 managed tests
pass. Unsupported change flags, negative distances and overflow use the existing
validated times-eight dispatch checks.

Native interpolation/sound, texture changes, signed distance edge cases, full
clipping, remaining floor variants and real-runtime comparison remain open.
Phases 1–3 and original invasion synchronization validation are incomplete.

### Absolute-height floor movement

Floor_MoveToValue (37) and Floor_MoveToValueTimes8 (68) now support map and ACS
activation. Native p_lnspec.cpp specifies signed world-height targets, with
argument 3 negating the height for any nonzero value; 68 additionally scales
the height by eight. A wide arithmetic intermediate prevents integer wrap, and
the new absolute target permits negative heights without relaxing relative
distance validation. Both variants remain non-crushing and reject unsupported
texture/type changes in argument 4.

Fourteen regressions cover direct/stack/map activation, upward/downward signed
targets, sign inversion, scaling, TagWait completion, ownership release,
overflow and change rejection. All 2,385 managed tests pass.
The existing floor height clamps still apply. Native fixed-direction behavior
under moving-ceiling interactions, full clipping, extreme heights, crushing
absolute variant 279, texture changes and runtime comparisons remain open.
Phases 1–3 and original invasion synchronization validation are incomplete.

### Crushing absolute floor movement

Floor_MoveToValueAndCrush (279) now supports map and ACS activation. This native
checkout passes CRUSH(arg3) - 1, so positive damage arguments are reduced by one:
8 means seven damage, and 1 enables crushing with zero damage. Nonpositive
arguments disable crushing. SectorMotion now records CrushWithoutDamage,
included in the checksum, so zero-damage crushing does not collapse into the
existing non-crushing floor representation.

Native floor CRUSHTYPE uses explicit modes 1/2 and the game default for every
other value (including 3). The HexenCrushDefaults option now also supplies that
default for this converted floor special. Ten regressions cover map/direct and
stack-result calls, damage translation, zero-damage movement, stopping versus
continuing at obstruction, Hexen defaults, ownership and TagWait completion.
All 2,395 managed tests pass. CLI help/README scope now includes converted floor
crush defaults, not just ceilings.

This preserves the checked-in native expression rather than assuming a different
damage convention. Native zero-damage collision side effects beyond movement,
full clipping, fixed floor direction and moving-ceiling target behavior remain
incomplete. Phases 1–3 and real-game invasion validation remain unfinished.

### Floor direction and retained destinations

Native floor creation assigns direction once, and MoveFloor applies the ceiling
limit to upward movement only. Converted non-lift map floors now store their
assigned FloorDirection, included in simulation checksums, and retain the
requested destination instead of clamping it to the initial ceiling. Absolute
targets choose direction at creation. Lifts, stairs and internal legacy actions
retain their existing movement paths.

Lower-to-lowest now selects the actual lowest neighbor even when it is higher
than the current floor. Its downward direction clamps to such a target on the
first tick, including an unobstructed target above the current ceiling. Four
regressions cover these two cases and relative/absolute raises whose original
target becomes reachable after a concurrent ceiling raise. All 2,399 managed
tests pass, including existing stair/lift and obstruction coverage.

Native collision handling for opposite-direction destinations, attached sectors,
floor-move compatibility flags, slopes, lower height limits and runtime rendering
of crossed planes remain open. Phases 1–3 and real-game invasion validation
remain incomplete.

### Highest neighboring floor specials

Converted Floor_RaiseToHighest (24), Floor_LowerToHighestEE (256), and
Floor_LowerToHighest (242) through shared map/ACS dispatch. Native references
are p_lnspec.cpp's wrappers and a_floor.cpp's floorLowerToHighest creation case.
Targets use the highest adjacent floor, preserve fractional heights, and retain
the explicitly assigned raise/lower direction. Special 24 uses its crush damage
argument and stops on obstruction; unsupported texture/type changes still fail.

Special 242 applies arg2 minus 128 to the selected height only when that height
differs from the current floor, or when arg3 is exactly 1. The subtraction uses
double arithmetic to avoid integer overflow on ACS arguments. Its equal-height
case still owns the floor until the first tick, and opposite-side destinations
retain native direction-based clamping for unobstructed flat sectors.

Seventeen additional test cases cover map/direct/stack dispatch, fractional
maximum selection, motion completion and TagWait, opposite-side targets, rejected
changes, and positive/nonpositive crush arguments. Three of these test cases each
exercise six adjustment/Heretic-option scenarios. The complete managed suite now
passes 2,416 tests. A restore-enabled run emitted a nested missing-SDK 8.0.423
nuget diagnostic while still passing; the final no-restore test run passed without
that diagnostic. The nested SDK pin remains unchanged.

This closes these flat-sector target/argument gaps only. No-neighbor native
sentinels, texture/type changes, attached sectors, slopes, full native actor
clipping and real-map behavior remain unverified or incomplete. The original
invasion round/timer/enemy report still requires native/game runtime validation.

### Floors targeting ceiling heights

Converted Floor_RaiseToLowestCeiling (238), Floor_LowerToLowestCeiling (258),
and Floor_RaiseToCeiling (259) through shared map/direct/stack ACS dispatch.
Reviewed p_lnspec.cpp wrappers and a_floor.cpp creation cases. Raising to the
lowest neighboring ceiling caps the initial destination at the sector's own
ceiling. Lowering selects the neighboring ceiling without that cap and retains
its assigned downward direction. The native lower variant ignores the supplied
gap; raising to the sector ceiling subtracts its signed gap. Unsupported
texture/type changes return failure before creating a mover.

Sixteen added test cases cover all three call paths and neighboring ceilings
below, above and equal to the sector ceiling; fractional destinations; ignored
versus applied gap arguments; TagWait completion and released floor ownership;
change rejection; and positive/nonpositive crushing for the own-ceiling raise.
The full Release suite passes 2,432 tests. During test development, an inaccessible
internal-array fixture setup was replaced with level initialization, and a
post-completion ownership probe was corrected to lower a floor already at its
ceiling. No production workaround was added for those test errors.

No-neighbor native sentinel behavior remains different: the managed fallback
uses the sector ceiling, whereas a native sector with boundaries but no valid
neighbors can retain FLT_MAX. Slopes, attached sectors, texture/type changes,
full native obstruction behavior, signed-gap edge regression coverage, and
runtime map comparisons remain open. This pass does not complete phases 1–3.

### Fixed-speed lowest-floor and instant ceiling-floor specials

Converted Floor_RaiseToLowest (257) and Floor_ToCeilingInstant (260) through
shared map/direct/stack ACS dispatch. Source review covered p_lnspec.cpp,
a_floor.cpp creation and dsectoreffect.cpp's MoveFloor direction branches.
Special 257 deliberately ignores arg1 and uses speed 2, with change in arg2
and crushing in arg3, as the native wrapper actually implements. Its target is
the lowest neighboring floor, even if equal to or below the current floor.

Special 260 uses arg1 for change, arg2 for crushing and arg3 for a signed gap.
Its stored direction is downward and its speed is zero. Targets at or above the
floor therefore complete on the first tick; targets below it remain active and
hold TagWait. Negative gaps can place the floor above its ceiling because native
downward movement does not apply the upward ceiling clamp. Unsupported changes
are rejected before movement starts. These peculiar semantics are preserved.

Seventeen new cases cover all three activation paths, fractional destinations,
fixed speed, ignored arguments, equal/opposite-side targets, signed gaps,
zero-speed waiting, change rejection, crushing/noncrushing obstruction and
endpoint rollback with released floor ownership. All 2,449 managed tests pass.
No-neighbor sentinels, no-displacement overlap processing, attached/sloped
geometry, complete actor clipping and texture/type changes remain open. Signed
gaps for special 260 now have coverage; the prior special 259 edge-coverage gap
is not closed by these tests. No native runtime comparison was performed, and
phases 1–3 plus real-game invasion validation remain incomplete.

### Floor raise-and-crush variants

Converted Floor_RaiseAndCrush (28) and Floor_RaiseAndCrushDoom (99) for map and
ACS direct/stack calls. Reviewed their p_lnspec.cpp wrappers, boolean CRUSHTYPE
conversion, and a_floor.cpp destination selection. Special 28 targets its own
ceiling minus eight. Special 99 first subtracts eight from the lowest neighboring
ceiling, compares that result with its own ceiling, and only then substitutes
its own ceiling minus eight when exceeded. This differs from subtracting eight
from the minimum of both ceilings.

Both wrappers use raw damage: zero enables crushing without damage, negative
values disable crushing, and positive values preserve their full damage amount.
Mode 1 keeps moving when crushing; mode 2 stops; other modes use the explicit
HexenCrushDefaults compatibility flag. Creation uses the existing plane ownership,
completion, obstruction and TagWait paths. Existing legacy Doom-format floor
special dispatch remains separate from these argument-based specials.

Eight new test cases include six route/special combinations, each with four
neighboring-ceiling scenarios, plus two damage/mode tests each exercising 48
map/script, damage, mode and compatibility combinations. All 2,457 managed tests
pass. No-neighbor native sentinels, attached/sloped geometry, complete actor
clipping, sounds, and native runtime parity remain open. Existing generic floor
endpoint tests remain passing; the new crusher matrix covers intermediate
obstruction and the destination tests cover unobstructed completion. Phases 1–3
and the original invasion runtime report remain incomplete.

### Selective floor crush stop

Converted Floor_CrushStop (46) after reviewing EV_FloorCrushStop in a_floor.cpp.
Native selection is based on the floorRaiseAndCrush mover type, not damage or
whether the mover currently crushes an actor. Added FloorCrushStopEligible to
sector motion state, set only by special 28 and included in simulation checksums.
Stopping removes eligible motions in the selected tagged sectors or manual back
sector and succeeds even when nothing matches. ACS tag zero still requires line
context, which the current ACS implementation cannot supply.

Thirteen new cases cover map/direct/stack stop calls against specials 28, 99 and
279, preserved unrelated movers, immediate floor ownership release, TagWait
resumption, no-mover success and manual back-sector selection. The complete suite
passes 2,470 tests. This new state field changes motion checksum values; runtime
negotiation/savegame compatibility remains part of the broader unfinished work.
The broader Floor_Stop (275), native sound-sequence cleanup, complete stair-lock
and attached-mover behavior remain unconverted or unverified. Phases 1–3 and
real-game invasion synchronization validation remain incomplete.

### General floor stop and interrupted stairs

Converted Floor_Stop (275) through map/direct/stack ACS dispatch after reviewing
EV_StopFloor, stair completion in a_floor.cpp and DSectorEffect::OnDestroy.
It removes active ordinary floor/lift/crusher motions in selected sectors while
preserving ceiling movers. Empty selections succeed; manual tag zero selects the
back sector, while ACS without line context rejects tag zero.

Stopping a later stair step releases its plane and TagWait but bypasses native
stairlock completion. The managed representation retains a completed record with
StairStopped, preventing normal group cleanup and stair retriggering while allowing
ordinary floor movement. Stopping the seed removes its active record; the remaining
steps can finish and unlock. StairStopped is included in checksums. This preserves
the tested interrupted-chain behavior within the existing group model; full native
per-sector stairlock/prevsec/nextsec parity and unusual overlapping chains remain
open. Stopped records are intentionally retained, not active thinkers.

Nine added cases cover three call routes across five mover variants, preservation
of a simultaneous ceiling mover, missing-tag results, manual selection, stopped
later-step TagWait/ownership and stopping a seed while the remaining step finishes.
An initial lift fixture had no lower destination; raising its initial floor fixed
the test setup. All 2,479 tests pass. Sound cleanup, attached movers/elevators,
complete persistence and real-map/native runtime comparisons remain unfinished.
This closes the prior basic Floor_Stop conversion gap, not phases 1–3 or the
original invasion runtime validation.

### Audit correction: restarted stair chains sharing a seed

The floor-stop follow-up audit found that StairGroup was derived from the seed
sector. Stopping and restarting that seed while an old follower remained reused
the same group, so old active/stopped records could prevent the new chain's
completed record from being cleaned up and block later retriggers.

Stair creation now assigns a group distinct from every retained/pending stair
chain and stores StairSeedIndex separately for floor-stop seed handling. Group
allocation rejects integer exhaustion before pending motions are committed.
StairSeedIndex is included in simulation checksums. Two regressions stop/restart
a seed while its former follower is either active or stopped, verify independent
completion and repeat activation, and verify the old active follower eventually
finishes and unlocks. All 2,481 managed tests pass.

This fixes a concrete managed lifecycle bug in the previous pass. It does not
establish full native overlapping-chain equivalence: native prevsec/nextsec links
can be rewritten, and the managed group model still differs from that mechanism.
The broader geometry, stair sector-special traversal, persistence and runtime
parity gates remain open, as does real-game invasion synchronization validation.

### Stair direction and retained destination audit

Reviewed native stair creation and DFloor::Tick reset handling. Stairs now retain
their assigned build direction in FloorDirection and reverse that direction for
the return leg. Previously the managed mover approached its target dynamically,
which moved an already-past-target step slowly instead of applying the native
first-tick clamp. Ceiling limits now follow the effective direction, including
reset, and the original requested target is retained instead of clamping it to
the ceiling at creation. Existing lower representable limits remain unchanged.

Five new cases cover ordinary up/down stairs whose middle step starts beyond
its target, with and without reset, plus a concurrent ceiling raise that makes
the original destination reachable. Existing synchronized, obstruction, pause,
stop and chain regressions still pass. Negative calculated synchronized speeds
remain rejected pending their own conversion; this pass does not close that gap.
Sector-special stair traversal, attached/sloped geometry and full native clipping
remain incomplete.

Final full suite: 2,486 passed. One preceding run reproduced the documented
five-second Pump_LiveSessionReceivesGuestClientInput timeout; the isolated rerun
and subsequent complete run passed. No test timeout or disabling workaround was
added. This intermittent failure remains an audit finding, not a resolved issue.
Phases 1–3 and real-game invasion synchronization remain incomplete.

### Reliable setup retries after Ready

Investigated the recurring Pump_LiveSessionReceivesGuestClientInput timeout.
Twenty isolated repetitions and twenty server-suite repetitions passed before
changes, so its exact trigger was not reproduced. Code review found a separate
concrete reliability gap: PregameHost only periodically drove Connecting/Waiting
clients, leaving a Ready client's pending StartGame without a timer-driven retry.
It depended on further inbound traffic or another explicit StartGame call.

PregameHost.Pump now flushes pending reliable traffic for Ready clients using the
existing resend interval. FlushClient also avoids attempting a send when the queue
reports pending-but-not-due with a zero packet length. A regression deliberately
consumes the first StartGame datagram without acknowledgement, verifies no early
retry, checks retransmission at the deadline without inbound traffic, and sends
a real acknowledgement before asserting live readiness and queue cleanup. It
failed against the old implementation (send count stayed at one), then passed
with the fix. No timeout increase or test disabling was used.

All 2,487 managed tests pass. This closes the demonstrated Ready-state retry gap;
it does not prove the intermittent server-test timeout resolved, nor establish
complete loss/reorder/handoff or native protocol parity. Phases 1–3, snapshot
release gates, and real-game invasion round/timer/enemy validation remain open.

### Delayed connection acknowledgement audit

Found a guest handshake regression: HandleConnectAck unconditionally replaced the
session token and reset Phase to WaitingForAssignment, even after reliable setup
had advanced to Starting. Host connection acknowledgements are independently
retransmitted, so a late copy can regress an established guest. A differing token
also invalidated subsequent traffic without resetting existing reliable state.

ConnectAck now adopts a nonzero token only while no session token has been adopted.
An established guest ignores subsequent ConnectAck packets. Two controlled UDP
regressions cover matching and differing stale tokens after Starting, a dropped
first StartGameAck, a duplicate StartGame, unchanged session/phase, and retransmitted
StartGameAck with the original token. The initial replay tests failed against the
old implementation with WaitingForAssignment instead of Starting. Final tests
wait for observed duplicate processing rather than using an arbitrary settling
sleep. The complete suite passes 2,489 tests.

This fixes the demonstrated replay/state-regression path, not a general reconnect
protocol; a fresh connection needs fresh guest state. Full endpoint validation,
reconnect/session replacement, loss/reorder/live-handoff and native parity audits
remain open. The intermittent server-test timeout has not been conclusively tied
to this defect. Phases 1–3 and real-game invasion validation remain incomplete.

### Connection replay and setup endpoint validation

Found two additional handshake defects. Repeated valid Connect packets could
allocate multiple slots for one endpoint (or receive Full when replaying an
existing session), and PregameGuest accepted decoded setup traffic regardless of
source endpoint. The host now recognizes existing endpoints after credential/
engine validation and before capacity checks, resending their ConnectAck without
resetting token, status or slot. The guest filters setup input against the configured
server endpoint before processing acknowledgements, rejections or services.

Eight added cases cover duplicate Connect in Connecting/Waiting/Ready states
with capacity available or exhausted, plus non-server ConnectAck and token-matching
StartGame packets. The pre-fix run exposed duplicate allocations, missing replay
acknowledgements and foreign packet state changes (seven cases failed); the final
full suite passes 2,497 tests. Tests use controlled UDP endpoints and observe
protocol responses rather than extending existing handshake timeouts.

This closes basic setup endpoint filtering and duplicate admission gaps, not
cryptographic authentication or a reconnect protocol. Credential revalidation,
session expiration/replacement, initial Connect retry, full live handoff and
native interoperability need further audit. The intermittent server-test timeout
is still not conclusively diagnosed. Phases 1–3 and real-game invasion validation
remain incomplete.

### Initial Connect packet-loss recovery

PregameGuest previously sent Connect once and stayed in SentConnect forever if
that datagram was lost. It now records the successful send time and retries only
while SentConnect, using RuntimeConnectAckResendMilliseconds. Incoming traffic is
processed before considering a retry, and a backwards timestamp cannot underflow
the elapsed-time check. Timestamp zero is supported without a sentinel collision.
Adopted sessions and rejected connections do not keep retrying.

Three new cases drop the first request with clocks starting at zero and 1000,
verify no early resend, observe a request at the deadline, verify silence after
session adoption, and check rejection suppresses retries. The two loss cases failed
before the fix. Final full suite: 2,500 passed. Existing duplicate-admission and
stale-ack regressions also pass. This closes the initial Connect retry gap noted
in the preceding audit; reconnect/session expiration, retry deadlines/cancellation,
live handoff, native interoperability and the intermittent server-test timeout's
exact cause remain open. Phases 1–3 and real-game invasion validation are incomplete.

### Signed synchronized stair speeds

Reviewed the native a_floor.cpp stairSync calculation and MoveFloor's assigned
movement directions. Removed the managed rejection of a negative calculated
follower speed. Native stores speed times signed rise divided by signed stair
step; the sign is retained separately from build/reset direction. The previously
converted direction-aware mover now evaluates these targets without rejecting
the seed or other steps.

Replaced two rejection tests with four up/down build/reset cases, and added two
high-speed cases where the signed step moves beyond its destination before reset.
These verify raw signed movement, reset to original planes and released chain
locks. Net test increase is four; all 2,504 tests pass. The historical negative
calculated-sync-speed rejection gap is closed for the tested flat, unobstructed
steps. Explicit negative base speed/step inputs, native sector-special traversal,
negative-speed actor clipping, advanced geometry and full runtime comparisons
remain open. Phases 1–3 and real-game invasion validation remain incomplete.

### Sector-marked Hexen stair traversal

Converted Stairs_BuildDown/Up (26/27) and their synchronized variants (31/32).
Reviewed p_lnspec.cpp wrappers, EV_BuildStairs and P_NextSpecialSectorVC plus
Stairs_Special1/2 definitions (26/27). These variants alternate the required
sector marker, traverse either line orientation in level order, and ignore floor
texture changes. Busy marked steps are visited and consume a height increment
without creating a new mover; traversal continues beyond them. Marked followers
do not inherit the Doom speed-four crush behavior. Existing Doom stair variants
retain their texture/directional traversal.

Fourteen tests cover four specials through map/direct/stack ACS routes, differing
textures, reversed lines, rejected marker branches, TagWait completion, sync timing,
and traversal through busy steps. All 2,518 tests pass. This closes the basic
sector-special traversal gap for flat sectors. Native mutable marker changes,
visited/link behavior in complex cyclic/overlapping chains, complete marker-stair
obstruction/reset combinations, advanced geometry and real-map runtime comparison
remain open. Existing generic stair reset/obstruction tests remain passing but do
not constitute exhaustive coverage of these newly enabled variants. Phases 1–3
and real-game invasion validation remain incomplete.

### Marker-stair reset and obstruction audit

Added six regression cases for the newly enabled marker variants, with no
production-code changes needed. Four cases exercise up/down ordinary/synchronized
chains: reset begins at the native argument position, seed plane ownership is
retained while waiting, TagWait on the final step stays blocked through return,
and the completed chain can be triggered again. Synchronized tests supply a
different unused arg4 to detect accidental reset-argument substitution.

Two follower-obstruction cases contrast marker stairs with existing Doom stairs
at speed four. The marker follower stays blocked without damage, preserves its
chain lock, then completes after the actor stops blocking. Existing Doom exact-
arrival/overshoot cases still pass. Reviewed the p_lnspec.cpp reset argument
wrappers and a_floor.cpp stairUseSpecials-dependent crush assignment.

All 2,524 tests pass. These cases narrow the prior reset/obstruction coverage gap;
negative-speed clipping, delayed/blocked-reset combinations, complex cycles and
links, advanced geometry and native runtime comparisons remain open. This is a
source/regression self-audit, not proof that the gameplay phases are complete.
Real-game invasion round/timer/enemy validation also remains outstanding.

### Generic stairs and repeat direction

Converted Generic_Stairs (204), reviewed against LS_Generic_Stairs. Arg3 bit zero
selects up/down, bit one ignores texture differences, and arg4 supplies reset.
Successful activation of a repeatable map line flips only the direction bit;
busy/rejected activation leaves it unchanged. ACS calls without a line cannot
mutate a map line. The existing Doom traversal and mover behavior are reused.
LevelLine.Arg3 is now mutable for this native behavior, and line Special/Arg3
values participate in the simulation checksum so next-activation state is visible.
This changes checksum values; complete line-state save/replication remains open.

Fourteen cases cover all direction/texture combinations via map/direct/stack
calls, repeat success/failure toggling, preserved texture bits, and checksum
sensitivity. One test initially allowed only eight ticks for a twelve-unit return
path; correcting its expected duration resolved that test failure. All 2,538
managed tests pass. Generic reset-specific interactions, malformed/negative
arguments, native compatibility flags, complex traversal, shared mutable level
ownership and full map/runtime parity remain open. Phases 1–3 and real-game
invasion validation remain incomplete.

### Simulation ownership of mutable lines

The generic-stair follow-up audit confirmed that AuthoritySimulation.Start reused
the supplied PlayLevel and its mutable LevelLine objects. Consuming a special or
toggling Arg3 could therefore leak into another simulation or a later restart
from the same map template. Start now creates a PlayLevel copy with independent
line objects and a private line list before spawning actors. Actors reference
that owned level. Other map data is shared as before; this is deliberately not a
full deep clone of mutable nested resources or thing argument arrays.

Two added cases exercise one-shot and repeatable generic stairs across two
simulations plus a fresh restart, verifying independent Special/Arg3 values,
unchanged template, actor level references and matching final checksums. Updated
the existing Doom exit test to inspect the owned line and assert the template
remains unchanged. Preserving the copied collection as a List retains existing
runtime collision-test insertion behavior. All 2,540 tests pass.

Callers inspecting runtime line changes must now use simulation.Level.Lines.
Direct helper calls supplied with external lines still operate on those explicitly
supplied objects; automatic use/cross activation uses owned lines. Full runtime
level ownership, line-state persistence/replication and native runtime validation
remain unfinished. Phases 1–3 and the original invasion runtime report remain open.

### Raise-and-change floor simulation foundation

Added partial simulation support for Floor_RaiseByValueTxTy (239), following
p_lnspec.cpp and a_floor.cpp creation timing. Successful map calls immediately
copy front-sector FloorPic and Special, then raise by the requested value. ACS
without a trigger clears Special and retains FloorPic. Busy floors do not mutate
metadata; invalid model references reject. This is not complete native TxTy parity:
TransferSpecial also copies damage/type/interval/leakiness and special flags,
which the managed sector model does not yet represent.

PlayLevel.CopyForSimulation now independently copies sectors as well as lines;
FloorPic and Special are mutable runtime metadata. Both participate in simulation
checksums. Existing stair texture/marker selection reads that owned metadata.
The server sector snapshot already reads Special, but FloorPic has no client
replication/rendering path in the current protocol. Texture and sector-effect
persistence, native damage-property transfer and derived sector effects remain
release blockers for this feature; do not treat it as a completed visual conversion.

Four tests cover immediate map changes, busy rejection, template isolation and
ACS direct/stack clearing with TagWait completion. All 2,544 tests pass. Other map
collections remain shared; this is not a complete deep-copy or savegame solution.
Phases 1–3 and real-game invasion validation remain incomplete.

### Sector damage metadata loading and transfer

Compared UDMF loading in `src/maploader/udmf.cpp`, `TransferSpecial` in
`src/playsim/p_sectors.cpp`, and `ClearSpecial` in `src/gamedata/r_defs.h`.
Managed sectors now retain damage amount, type, interval and leakiness. UDMF
level construction preserves signed damage amounts, defaults intervals to 32,
narrows interval/leakiness to the native signed 16-bit fields, clamps intervals
to at least one after narrowing, and clears related properties when damage is
zero. Raw parsed values remain available in the UDMF document model.

Floor special 239 transfers these properties immediately with its model sector,
or clears them when invoked without a trigger. Busy sectors remain unchanged,
simulation copies preserve the source template, and all four fields participate
in simulation checksums. Native field-width review caught an initial omission;
boundary regressions now cover wraparound before normalization.

Ten new map-loading cases and four checksum cases pass; existing map/ACS floor
tests now also verify damage transfer, clearing and isolation. Full Release
suite: 2,558 passed; warnings-as-errors build: zero warnings/errors.

This supersedes only the four missing metadata fields in the preceding audit.
Sector damage is not yet applied to actors. Native special/damage flags, binary
sector-special damage initialization, namespace-specific UDMF gating, client
texture/damage replication and persistence remain incomplete. Real-game invasion
validation and phases 1–3 remain open. This is a source/regression self-audit.

### Ordinary player sector damage and healing

Reviewed `P_ActorInSpecialSector` in `src/playsim/p_spec.cpp` and its actor-tick
call in `src/playsim/p_mobj.cpp`. The managed actor tick now applies ordinary
flat-sector damage to living players after movement, using the shared clock
before its increment. Contact is checked against the current floor, and interval
checks include tic zero. Invalid runtime intervals repair to 32. Positive damage
uses existing armor, invulnerability, pain and death handling without an attacker;
negative damage heals up to 100 without reducing overhealth or resurrecting.
Healing arithmetic widens before negation to avoid integer-minimum overflow.

Sixteen regressions cover cadence, landing without resetting the shared clock,
armor, invulnerability, airborne exclusion, healing limits, death, invalid
interval repair and ACS special 239 clearing active damage. The first version of
the clearing fixture called a nonexistent overload; it was corrected to execute
actual ACS bytecode. Final full Release suite: 2,574 passed. Build with warnings
as errors: zero warnings/errors.

This closes only ordinary flat-floor player damage/healing application. Damage
types currently use generic damage; type-specific resistance/death, radiation
suits/leakiness, hazard/exit/terrain flags, monster opt-in, liquid/3D-floor contact,
native binary damage-special initialization, full tick-order parity, replication
and persistence remain open. Existing managed invasion regressions pass, but the
original real-game round/timer/enemy report still needs native runtime validation.
Phases 1–3 remain incomplete; this is a source/regression self-audit.

### Sector opt-in for non-player and airborne damage

Reviewed `checkForSpecialSector` in `src/playsim/p_spec.h`, contact checks in
`P_ActorInSpecialSector`, UDMF `hurtmonsters`/`harminair` loading, and native
`TransferSpecial`/`ClearSpecial`. Managed UDMF levels now retain these two flags,
including sectors with zero initial damage. They are copied into simulation
sectors and included in checksums. These native MoreFlags are deliberately not
transferred or cleared by floor special 239; destination contact rules remain.

The damage tick permits non-player actors when HurtMonsters is enabled and
bypasses flat-floor contact when HarmInAir is enabled. Existing damage eligibility
and lifecycle handling still apply. Healing now caps non-player actors at their
recorded spawn health, matching the ordinary native monster path; healing amount
is limited to 65,536 using widened arithmetic. Player healing retains its 100 cap.

Four loading tests and eighteen simulation tests cover flag combinations,
airborne players/monsters, default non-player exclusion, monster healing/death,
destination flag preservation during damage transfer and checksum differences.
All 2,596 managed tests pass; warnings-as-errors build has zero warnings/errors.

Actor-level NOSECTORDAMAGE/FORCESECTORDAMAGE flags, damage-type effects, protection,
hazard/exit/terrain flags, liquids/3D floors, skill-adjusted/custom player health,
binary damage-special initialization, namespace gating, replication/persistence
and full native tick-order parity remain open. Dynamically supplied non-player
actors also require their spawn-health metadata to be populated for healing.
This is a source/regression self-audit, not native runtime certification.
Phases 1–3 and the real-game invasion report remain incomplete.

### Actor sector-damage override precedence

Compared the managed eligibility path with `checkForSpecialSector` in
`src/playsim/p_spec.h`. Actors now expose NoSectorDamage and ForceSectorDamage
runtime flags. Force wins over actor immunity and sector monster opt-in, while
floor contact, shared interval timing and normal damage protection still apply.
The same eligibility controls healing. Both flags participate in checksums.

Twenty-two regression cases cover the full player/monster, sector opt-in,
immunity and force matrix, healing, airborne contact, invulnerability, enabling
force between interval boundaries and checksum differences without health changes.
Full Release suite: 2,618 passed; warnings-as-errors build: zero warnings/errors.

This converts runtime eligibility only. Loading these flags from actor definitions
or scripts, replication and complete saves remain unfinished; ordinary existing
spawns retain false defaults. The previous damage-type, protection, hazard/exit,
geometry, skill and native-runtime gaps remain open. Phases 1–3 and the original
real-game invasion report are not complete. This is a source/regression self-audit.

### Sector_SetDamage map and ACS dispatch

Converted special 214 through shared map and ACS direct/stack dispatch, comparing
`LS_Sector_SetDamage` and `MODtoDamageType` in `src/playsim/p_lnspec.cpp` and tag
iteration in `src/playsim/p_tags.cpp`. Matching sectors receive amount, damage
type, interval and leakiness; tag zero selects untagged sectors. Missing tags
still return success. Nonpositive intervals select native legacy defaults using
the original amount before signed 16-bit field conversion. Explicit intervals
and leakiness narrow without extra clamping; runtime contact repairs invalid
intervals as before. Special numbers and contact flags are preserved.

Twelve new tests cover map consumption, both ACS argument routes, multiple tags,
live damage and disabling it, default thresholds, negative amounts, field
overflow, interval repair, unknown damage types, zero tags and missing targets.
Initial gameplay assertions exposed absent sector boundaries in the test fixture;
adding actual room geometry fixed actor placement. Final full Release suite:
2,630 passed; warnings-as-errors build: zero warnings/errors.

Damage types remain metadata with generic gameplay damage, and leakiness still
awaits radiation-suit support. Full tag-manager/multiple-tag representation,
advanced actor definitions, protection/hazard effects, replication/persistence
and native runtime parity remain open. Phases 1–3 and real-game invasion
validation remain incomplete. This is a source/regression self-audit.

### Ordinary damage-special initialization

Compared `SetupSectorDamage` and `InitSectorSpecial` in
`src/maploader/specials.cpp`. Simulation startup now initializes ordinary Doom
binary specials 4/5/7/16 and extended specials 68/69/71/80/85/104/196 with their
damage/healing amounts, 32-tic interval, leakiness and damage type. Existing
nonzero explicit damage metadata takes precedence. Recognized specials are
consumed after lighting initialization, on simulation-owned sectors only.

Sixteen new cases cover numeric namespaces, amounts, first-tic gameplay,
explicit positive/negative damage precedence, source-template isolation and
unconverted special preservation. Full Release suite: 2,646 passed;
warnings-as-errors build: zero warnings/errors. Existing lighting and invasion
regressions also pass.

This converts ordinary low-byte damage initialization only. Specials carrying
high-bit flags are currently left untouched by this initializer. Boom/MBF21
damage/death flags, translated UDMF namespaces, exit/hazard/lava behavior and
Strife special 104's lighting side remain unfinished. Damage types and radiation
suit leakiness still lack their specialized gameplay effects. Full native map
parity, phases 1–3 and real-game invasion validation remain incomplete. This is a
source/regression self-audit, not native runtime certification.

### Combined strobe and damage regression audit

Compared `SpawnLights` in `src/maploader/specials.cpp`. Extended special 104
now starts the same five-bright/fifteen-dark strobe as native Strife, before
damage initialization consumes the special. This closes the missing lighting
side explicitly identified in the preceding pass.

Seven new cases verify the bounded initial strobe delay, native dark duration,
independent light/damage cadence over 65 ticks for Doom 4 and extended 68/104,
map-template isolation, ACS disabling damage while the light keeps running,
and stopping the light while damage continues. All 2,653 managed tests pass;
warnings-as-errors build: zero warnings/errors. No native runtime certification
is implied; exact native lighting RNG, remaining sector flags, specialized damage
effects, phases 1–3 completion and real-game invasion validation remain open.

### Boom damage-bit initialization and precedence

Reviewed the Doom sector bit translation in `wadsrc/static/xlat/doom.txt` and
damage setup in `src/maploader/specials.cpp`. Startup now interprets Doom binary
damage bits 0x20/0x40/0x60 and extended bits 0x100/0x200/0x300 before ordinary
low-number specials. Explicit nonzero damage wins; otherwise the flag damage wins
over the low special, with native amounts, type, interval and leakiness. Consuming
a recognized low special preserves the remaining raw flags, since full secret,
friction and related flag conversion is still incomplete.

Eleven new cases cover each format/amount, first-tic damage, precedence, unrelated
flag preservation and compatibility gating. Corrected an older test that treated
Doom special 69 as unconverted: it actually combines damage bits with special 5.
The MBF21 death-bit branch remains deliberately unimplemented and is not treated
as ordinary damage when that compatibility mode is enabled. All 2,664 managed
tests pass; warnings-as-errors build: zero warnings/errors.

This supersedes the preceding ordinary-initialization high-bit exclusion only
for supported damage combinations. Translated UDMF namespaces, full native flag
consumption/transfer, MBF21 death/exit behavior, protection, specialized damage
effects, phases 1–3 and real-game invasion validation remain open. This is a
source/regression self-audit, not exhaustive native parity validation.

### Translated UDMF sector numbering

Reviewed namespace selection and sector translation in `src/maploader/udmf.cpp`,
`TranslateSectorSpecial` in `src/gamedata/p_xlat.cpp`, and the Doom/base translator
files. Lighting and damage initialization now share a map-level predicate for
Doom binary and UDMF Doom/ZDoomTranslated sector numbers. ZDoom retains extended
numbering. Doom UDMF suppresses fire-flicker and phased/sequence extensions that
`doom_base.txt` disables; ZDoomTranslated keeps those extensions.

Fifteen parser-to-simulation tests cover namespace case, ordinary/Boom damage,
combined lighting/damage and disabled versus enabled lighting extensions. Initial
fixtures omitted player spawn flags; the tests were corrected with explicit
skill/mode flags. Final full Release suite: 2,679 passed; warnings-as-errors build:
zero warnings/errors.

This closes namespace selection for the currently converted sector effects only.
Custom translators, other game namespaces, complete sector translation/flag
consumption, advanced damage effects, phases 1–3 completion and real-game invasion
validation remain open. This is a source/regression self-audit.

### Damaging exit-floor foundation

Compared native damage-end setup and `P_ActorInSpecialSector`. Doom sector 11
and extended 75 now initialize 20 damage every 32 tics, leakiness 256 and the
end-god-mode/end-level flags when no explicit damage takes precedence. A separate
player GodMode property protects against ordinary damage; floor contact removes
it before the interval check without removing actor invulnerability. On damage
ticks, player health at or below 10 triggers the normal exit. These flags transfer
and clear with floor special 239 and participate in checksums, as does GodMode.

Seven new cases cover exit timing/threshold, explicit damage precedence, airborne
exclusion, god mode versus invulnerability and explicit damage bypass. Existing
floor-transfer/ACS-clearing tests now assert both flags. Full Release suite:
2,686 passed; warnings-as-errors build: zero warnings/errors.

This is the ordinary managed exit-floor foundation. Deathmatch no-exit rules,
additional cheat modes, radiation-suit interactions, cheat command integration,
complete persistence/replication and exact native damage bypass semantics remain
unfinished. Phases 1–3 and real-game invasion validation remain open. This is a
source/regression self-audit, not native runtime certification.

### Deathmatch damaging-floor exit restriction

Converted the `!deathmatch || !(dmflags & DF_NO_EXIT)` condition from native
`P_ActorInSpecialSector`. Simulation startup accepts a no-exit setting, combines
it with the spawn game mode, and records the resulting DamageExitAllowed policy
in the checksum. DedicatedServerOptions.NoExit is passed through server boot.
Blocked exits still apply damage, remove ordinary god mode and can kill players;
single-player/cooperative modes ignore this restriction.

Seven simulation cases cover the complete mode/setting matrix, continuing lethal
damage and checksum differences before gameplay. Four server cases verify policy
propagation. Full Release suite: 2,697 passed; warnings-as-errors build: zero
warnings/errors. This supersedes the damaging-floor restriction gap only.

NoExit is currently an API/server-options setting; command-line controls,
network rule negotiation, live changes, other exit mechanisms and full save
restoration remain unfinished. The existing native-runtime, specialized damage
and phase 1–3 gaps remain open, including real-game invasion validation. This is
a source/regression self-audit.

### Ordinary exit-line deathmatch restriction

Reviewed `CheckIfExitIsGood`, native normal/secret exit specials and the ordinary
telefrag-damage protection rules. Converted Doom use/cross exits and extended
243/244 now consult the existing deathmatch exit policy. Forbidden actor-triggered
exits apply 1,000,000 self-attributed damage through the shared damage boundary,
bypass ordinary armor/invulnerability, return failure and preserve the line.
World-triggered internal exits still succeed without an activator. The internal
normal/ID24 exit entry points use the same check.

Seven new tests cover six normal/secret map routes with restriction enabled and
disabled, player death/source attribution, armor preservation, failed-line
retention, repeat behavior and world-exit exemption. Full Release suite: 2,704
passed; warnings-as-errors build: zero warnings/errors.

Remaining gaps include always-apply-deathmatch-flags, kill-all-monsters/hub gates,
ACS activator propagation and complete exit dispatch, special telefrag protection
flags/cheat variants, exit position/MAPINFO transitions, command-line rule control
and networking/persistence. Phases 1–3 and real-game invasion validation remain
incomplete. This is a source/regression self-audit.

### ACS exit dispatch and activator foundation

ACS fibers now retain an optional starting actor. Map ACS starts, nested Execute
and ExecuteAlways propagate it; resuming preserves the original actor, matching
native script start/resume code in `src/playsim/p_acs.cpp`. Direct and stack normal
and secret exit specials (243/244) use explicit shared exit dispatch, avoiding the
legacy internal special-number fallback. Actor-triggered exits obey no-exit;
world or destroyed-activator exits use the world exemption. Active actor identity
participates in the ACS checksum. Legacy one-argument fallback receives the actor.

Thirteen new tests cover both argument routes, normal/secret exits, world
exemption, actor attribution through map/nested execution, resume/destroyed actor
behavior and checksum identity. Full Release suite: 2,717 passed; warnings-as-errors
build: zero warnings/errors.

This is an activator/exit foundation, not complete ACS context conversion. Trigger
line/side context, SetActivator and actor queries, arbitrary actor lifecycle/save
restoration, client scripts, exit position/hub progression and remaining native
exit rules still need work. Phases 1–3 and real-game invasion validation remain
incomplete. This is a source/regression self-audit.

### ACS trigger-line propagation

Compared native DLevelScript activationline capture and resume behavior. Managed
fibers now retain the optional triggering line through map starts, nested normal
and always execution, and suspension. Converted door/floor/stair/ceiling dispatch
receives that line for manual targeting and model selection. The legacy fallback
also receives it. Line identity and action-relevant fields participate in the ACS
checksum; a resumed script retains its original trigger.

Six new regressions verify direct/stack tag-zero raise-and-change, nested execution,
front-sector texture/damage transfer, back-sector motion, preservation after
one-shot trigger consumption, resume context and checksum distinctions. Existing
door/stair/ceiling regressions pass. Full Release suite: 2,723 passed;
warnings-as-errors build: zero warnings/errors.

Back-side activation context, complete native manual activation rules, arbitrary
line lifecycle/save restoration, other ACS actor/context operations and full
native parity remain unfinished. Phases 1–3 and real-game invasion validation
remain open. This is a source/regression self-audit.

### ACS LineSide and captured activation side

Converted native PCD_LINESIDE (80), which pushes the script's stored backSide.
Fibers now retain a side value through starts, nested execution and suspension;
the value participates in checksums. Map ACS use captures the actor's side, while
crossing captures its previous position so movement cannot invert the reported
origin side. Explicit callers may supply the side; world scripts default to front.

Seven regressions cover both use/crossing directions, nested/resumed context and
checksum distinctions. Full Release suite: 2,730 passed; warnings-as-errors build:
zero warnings/errors. This closes the stored-side query foundation only: exact
on-line/degenerate geometry semantics, full side-dependent special restrictions,
legacy unknown-format activation, persistence and remaining ACS context operations
still need work. Phases 1–3 and real-game invasion validation remain incomplete.
This is a source/regression self-audit.

### ACS activator identity queries

Compared native PCD_PLAYERNUMBER (247) and PCD_ACTIVATORTID (248). The managed VM
now pushes the activator's visible PlayerNum or -1 for a non-player/world context,
and its map ThingId or zero for world context. Destroyed actors use world defaults;
dead but existing actors retain identity. Simulation checksums now include ThingId
and player slot identity because these values are script-observable.

Thirteen regressions cover player slots, monster/world contexts, zero/nonzero
TIDs, destroyed versus dead actors and TID checksum differences. Full Release
suite: 2,743 passed; warnings-as-errors build: zero warnings/errors. Native reserved
slot/roster integration, dynamic TID changes, SetActivator, complete actor query
support and persistence remain unfinished. Phases 1–3 and real-game invasion
validation remain open. This is a source/regression self-audit.

### ACS actor position queries

Converted GetActorX/Y/Z (196/197/198) after reviewing native ACS dispatch,
SingleActorFromTID and head insertion into the native TID hash. TID zero resolves
the retained activator; nonzero TIDs resolve the newest remaining matching actor
in the managed spawn list. Missing/destroyed actors return zero; coordinates keep
their signed 16.16 representation. Empty stacks terminate safely. Both bytecode
walker formats now recognize X/Y as zero-immediate-operand instructions.

Ten new cases cover fractional axes, duplicate TID order, destroyed actor removal,
world/missing lookups and underflow. Two prior opcode-alias regressions were updated
to expect real coordinate queries instead of unsupported-instruction termination.
Full Release suite: 2,753 passed; warnings-as-errors build: zero warnings/errors.

Z bob offsets, native out-of-range double conversion, dynamic TID rehashing,
client-side actor namespaces and complete actor query/state parity remain open.
Phases 1–3 and real-game invasion validation remain incomplete. This is a
source/regression self-audit.

### ACS flat-sector plane and angle queries

Converted GetActorFloorZ (259), GetActorAngle (260) and GetActorCeilingZ (282)
using the existing activator/TID lookup. Reviewed the native query cases and
AngleToACS conversion. The managed flat-sector foundation reads current sector
planes in signed 16.16 format and returns the upper 16 bits of the actor's BAM
angle as ACS turns. Missing actors return zero; underflow stops the script.

Fifteen new cases cover fractional floor/ceiling values, completed floor movement,
cardinal/fractional/wrapped angles, missing actors and underflow. Full Release
suite: 2,768 passed; warnings-as-errors build: zero warnings/errors.

Native actor floorz/ceilingz can differ from the containing sector's planes with
multi-sector contact, slopes, portals and 3D floors; those cases remain unfinished.
Native angle double-precision edge behavior, complete actor queries, phases 1–3
and real-game invasion validation remain open. This is a source/regression
self-audit, not a claim of full native parity.

### ACS actor and sector light queries

Native source audit found GetActorLightLevel mislabeled as 342 in the managed
enum; corrected it to 340 and added recognition in both bytecode walker formats.
Implemented actor light lookup through the existing activator/TID path and
GetSectorLightLevel (281) through the first matching sector tag. Both read current
simulation lighting. Missing actors return zero; missing sector tags return -1.

Ten runtime regressions cover actor/tag lookup, animated lights, first-sector and
zero-tag behavior, defaults and underflow. Two decoder tests lock down the native
opcode and wordcode/compact instruction boundaries. Full Release suite: 2,780
passed; warnings-as-errors build: zero warnings/errors.

Actor queries currently use flat-sector lighting; native 3D-floor light lists,
dynamic actor namespaces and complete light/resource parity remain unfinished.
Phases 1–3 and real-game invasion validation remain open. This is a source and
regression self-audit.

### ACS SetActorAngle

Converted native PCD_SETACTORANGLE (276) after reviewing its helper in
`src/playsim/p_acs.cpp`. TID zero changes only the retained activator; nonzero
TIDs change every matching non-destroyed actor, including dead actors. ACS turns
wrap into BAM angle storage. Missing targets consume arguments and continue;
underflow stops without mutation. Existing angle queries observe the new value.

Twelve regressions cover signed/overflow angle wrapping, activator targeting,
multiple matches, dead/destroyed actors, missing targets, stack preservation and
query integration. Full Release suite: 2,792 passed; warnings-as-errors build:
zero warnings/errors. Interpolated function variants, client view interpolation,
actor pitch control and broader native state/replication parity remain unfinished.
Phases 1–3 and real-game invasion validation remain open. This is a source and
regression self-audit.

### ACS actor pitch foundation

Native opcode review corrected GetActorPitch/SetActorPitch from 333/334 to
331/332. Added both operations, sharing activator/TID targeting with angle queries
and setters. Pitch state now belongs to all actors and participates in checksums;
player input still uses the existing +/-89 degree managed clamp. ACS setters
normalize signed turns for non-player actors and apply the player clamp. Setters
consume two arguments; queries replace the TID argument and return zero if absent.

Eleven regressions cover signed/wrapped monster pitch, player clamping, multi-actor
TID setting without changing yaw, missing targets and stack underflow. The older
unsupported-opcode test now targets native PrintBind (333), since 332 is implemented.
Full Release suite: 2,803 passed; warnings-as-errors build: zero warnings/errors.

Custom native player pitch limits, interpolation, non-player pitch persistence and
replication, rendering/attack integration and native precision edge cases remain
unfinished. Phases 1–3 and real-game invasion validation remain open. This is a
source/regression self-audit, not complete native parity validation.

### Actor pitch pose-archive integration

The pitch audit found that CaptureState discarded non-player pitch and restore
applied the player limit to every actor. Pose archives now write version 6 and
retain all actors' pitch in the existing fixed-point field. Versions 1–5 remain
readable. Restore validates player pitch against +/-89 degrees and non-player
pitch against +/-180 before mutating state, then restores pitch for every actor.
The version bump prevents older readers from silently discarding the new values.

Nine new regressions cover player/monster fractional and boundary round trips,
version-5 compatibility and invalid pitch rejection without partial mutation.
Full Release suite: 2,812 passed; warnings-as-errors build: zero warnings/errors.
The wire record size is unchanged. Direct callers must supply normalized monster
pitch for restore. This remains a pose archive: actor membership, inventory,
ACS/movers, invasion state and full rule restoration are not complete savegames.
Pitch networking/rendering and phase 1–3/native-runtime gaps remain open, including
real-game invasion validation. This is a source/regression self-audit.

### ACS sector-plane height queries

Converted native opcodes 261/262 (`GetSectorFloorZ`/`GetSectorCeilingZ`) into
the managed VM. Audited against `src/playsim/p_acs.cpp`: arguments are `(tag,
x, y)` with integer map coordinates, nonzero tags select the first matching
sector, and results are ACS fixed-point heights. Tag zero uses point lookup.
Queries read current simulation heights, including ongoing movement, rather
than the original map heights. Missing sectors return zero; incomplete argument
stacks terminate the fiber before a following action can execute.

Added 16 regression cases covering both planes, fractional heights, duplicate
tags, coordinate order/units, missing tags/unresolved points, each argument
underflow size, moving floors/ceilings, and preservation of the lower stack.
Full Release suite: 2,828 passed; warnings-as-errors build: zero warnings/errors.

Scope remains flat sectors. Managed polygon point lookup is not native BSP
lookup (particularly for points outside enclosing geometry); slopes, portals,
and 3D-floor geometry remain unconverted. This source/regression audit does not
establish full native gameplay or invasion synchronization parity.

### ACS triggering-line row offset and sidedef metadata

Converted native opcode 258 (`GetLineRowOffset`) against `src/playsim/p_acs.cpp`.
The query pushes the triggering line's front-sidedef middle-texture vertical
offset as an integer truncated toward zero, regardless of activation side.
World scripts return zero; malformed managed front-side references also safely
return zero. Nested scripts inherit the trigger and resumed scripts retain the
original trigger. The query does not consume existing stack values.

Source review found that level building discarded sidedef row offsets. Binary
row offsets now reach `LevelSide.MidTextureOffsetY`; UDMF combines `offsety`
with fractional `offsety_mid` in ZDoom, ZDoomTranslated and Vavoom namespaces,
following `src/maploader/udmf.cpp`. Other namespaces retain only the base offset.
The simulation checksum includes the newly observable offset.

Added 20 regressions: signed binary offset limits, UDMF namespace gating and
fractional addition, query truncation/front-side selection, missing/invalid
triggers, nested/resumed context, lower-stack preservation and checksum changes.
Full Release suite: 2,848 passed; warnings-as-errors build: zero warnings/errors.
This converts loading and querying the middle vertical offset; texture rendering,
runtime texture-offset mutation/scrollers, top/bottom offsets, and full native
sidedef special handling remain outside this pass. Full phases 1–3 and native
invasion runtime verification remain incomplete.

### ACS activator health and armor queries

Converted native opcodes 120/121 (`PlayerHealth`/`PlayerArmorPoints`) against
`src/playsim/p_acs.cpp`. Both push a value without consuming stack arguments.
Health reads any existing activator, including monsters and dead actors; armor
reads the managed player's current inventory. World or destroyed activators
return zero. Added opcode 121 to both old and compact bytecode walkers as a
zero-immediate instruction. Health and armor already participate in simulation
checksums, so no extra checksum fields were necessary.

Added 22 regression cases covering player/monster/dead/over-100 health,
player armor after death, missing/destroyed activators, unarmored monsters,
damage absorption, nested map-script context, resumed-script context with
updated values, lower-stack preservation, and old/compact decoder boundaries.
Full Release suite: 2,870 passed; warnings-as-errors build: zero warnings/errors.

Parity limits: native BasicArmor inventory can belong to non-player actors;
the managed inventory model currently belongs to players only. Managed health
and damage clamp at zero, so negative native overkill health is not represented.
These commands expose the current managed state; full inventory/lifecycle parity
and the phases 1–3 completion gates remain open. Compact decoding coverage does
not establish compact-bytecode VM execution parity.

### ACS Timer and authority tick boundary

Converted opcode 93 (`Timer`) against `src/playsim/p_acs.cpp`, which pushes
`Level->time` without consuming stack operands. Source audit of `src/p_tick.cpp`
found that native ACS thinkers execute before `Level->time` increments. Managed
thinker execution advances the clock before the separate ACS pass, so reading
that clock directly would return one tic too many. Authority ticks now capture
their starting tic and pass it to ACS; direct VM ticks read the current clock
without advancing it. No simulation stage was reordered.

Added 12 regressions for initial and later authority ticks, raw tic units,
direct VM calls (including the largest positive clock value), same-tick
consistency, delay elapsed time, suspend/resume, restored pose clocks, and
nested script execution time. Query tests also retain stack values below Timer.
Full Release suite: 2,882 passed; warnings-as-errors build: zero warnings/errors.

This audits clock observation within the current managed scheduler. Nested
scripts still follow the existing managed fiber snapshot schedule; full native
thinker scheduling, hub clocks, pause/freeze behavior, complete ACS saves, and
invasion round/timer/enemy synchronization in a running native game remain
outside this pass. The earlier phase completion gates remain open.

### ACS game mode and standard skill queries

Converted opcodes 91/92 (`GameType`/`GameSkill`) against `src/playsim/p_acs.cpp`
and the default skill return behavior in `src/gamedata/g_skill.cpp`. Authority
simulation now retains its spawn mode and normalized standard skill. GameType
returns 0/1/2 for single/cooperative/deathmatch independently of actor count;
GameSkill returns the configured standard skill index, clamped consistently
with the existing spawn filter. Both push a value without consuming arguments.
The settings are immutable after boot and included in simulation checksums.

Added 18 regressions covering all supported modes, standard skill indexes and
out-of-range normalization, omitted options, lower-stack preservation, checksum
distinction/equivalence, and dedicated-server propagation. Full solution run
passed 2,898 tests; the subsequent server-only run passed all 61 tests including
two added boot cases, bringing verified total coverage to 2,900. Final Release
warnings-as-errors build passed with zero warnings/errors.

Native title-map mode and custom MAPINFO skill ACSReturn values are not yet
represented. This does not implement complete skill gameplay rules or native
roster/network-game queries. Pose archives still rely on the destination
simulation's boot settings; they are not full session saves. Phases 1–3 and
native invasion runtime validation remain incomplete.

### ACS trigonometric operations

Converted native opcodes 220/221/222 (`Sin`, `Cos`, `VectorAngle`) in the enum,
both bytecode walkers and the word-format VM. Source audit covered
`src/playsim/p_acs.cpp` and angle conversions in `src/common/utility/vectors.h`.
Sin/Cos consume Q16 turns and produce rounded 16.16 values; angle inputs are
reduced modulo a full turn. VectorAngle consumes `(x,y)` and returns normalized
unsigned Q16 turns truncated toward zero. Missing operands stop the fiber.

Added 32 regressions for cardinal/diagonal angles, negative/wrapped turns,
integer limits, zero vector, quadrant and argument ordering, scaled vectors,
truncation, underflow, lower-stack preservation, and old/compact decoding.
Full Release solution: 2,932 passed; warnings-as-errors build: zero warnings/errors.

Managed operations use System.Math. Native `cmath.h` selects CRT, custom or
fast math depending on build flags; bit-for-bit results near rounding boundaries
across those backends have not been established by native differential tests.
This source/regression audit does not close full numerical/native parity,
compact VM execution, phases 1–3, or native invasion runtime validation.

### ACS packed-byte stack pushes

Converted PushByte (167), PushBytes (175), and Push2Bytes through Push5Bytes
(176–179) against `src/playsim/p_acs.cpp`. The word-format VM now consumes raw
unsigned bytes in source order and continues at the exact next byte offset,
including unaligned word opcodes. Counted pushes accept zero through 255 values.
Payload bounds are checked before pushing any values; truncated inputs stop
the fiber safely. The existing instruction budget still bounds execution.

The word bytecode walker previously could not handle these packed operands.
It now advances by their exact byte length. Added OperandByteCount metadata
for exact lengths in both walkers while retaining legacy OperandWordCount
units (rounded-up words for new packed word instructions; bytes for compact).

Added 23 regressions spanning execution order/unsigned values, all fixed push
widths, zero and maximum count, instruction-budget continuation, truncation,
and word/compact decoding boundaries. An initial test-helper compile error
from array Reverse overload resolution was corrected before the final run.
Full Release suite: 2,955 passed; warnings-as-errors build: zero warnings/errors.

This supports packed operands in the word-opcode VM; it does not implement
compact opcode execution, remaining byte-immediate specials, or full native
ACS scheduling/parity. Phases 1–3 and native invasion validation remain open.

### ACS byte-immediate specials and delay

Converted Lspec1DirectB through Lspec5DirectB (168–172) and DelayDirectB (173)
against `src/playsim/p_acs.cpp`. Byte specials read an unsigned special ID and
one through five unsigned arguments, zero-fill omitted arguments and reuse the
existing context-aware dispatch. Byte delay reuses countdown and legacy Hexen
extra-tic behavior. Truncated operands stop execution before special mutation.
The word walker now advances by exact byte lengths for these forms.

Added 26 regressions covering all argument widths against word-form sector
damage results, unsigned values, every payload truncation, delay endpoints,
stack preservation, actor context for restricted exits, both walker formats,
and oversized old-format directory counts. Corrected an existing fixture that
incorrectly padded byte operands to whole words. During fixture repair, use of
an enhanced-only test builder exposed an actual old-directory integer overflow:
count validation now uses division and offset checks use widened arithmetic,
rejecting malformed data instead of throwing. Test helper compilation and
minimum-lump-size issues were corrected before final verification.

Full Release suite: 2,981 passed; warnings-as-errors build: zero warnings/errors.
Only already-supported line specials are dispatched; RandomDirectB, compact
VM execution, remaining ACS/native gameplay parity, and native invasion runtime
validation remain open. Phases 1–3 are not complete.

### Packed ACS control-flow follow-up audit

Auditing the recent packed-byte conversion found two integration defects.
The VM rejected jump destinations not divisible by four, although packed
operands can place valid word opcodes at any byte offset. Jump validation now
retains range checks without imposing alignment. Native `PCD_CASEGOTOSORTED`
explicitly aligns its table count to four bytes; the VM now does so too.
The word walker previously discarded fractional-word padding when computing
table length. It now checks/counts the table in bytes, reports exact operand
length, and rejects truncated or oversized tables before traversal.

Added 21 regressions covering Goto, IfGoto, IfNotGoto and CaseGoto targets at
all three unaligned offsets; sorted-table matches/misses after packed operands;
and word-walker padding/truncation at each alignment. Existing invalid-jump
regressions remain passing. Source comparison used `src/playsim/p_acs.cpp`.
Full Release suite: 3,002 passed; warnings-as-errors build: zero warnings/errors.

This fixes control flow in the current word-format program model. Full behavior
module addressing/relocation, compact VM execution, GotoStack jump-table
dispatch and other unconverted ACS operations remain open. Phases 1–3 and
native invasion runtime validation remain incomplete.

### ACS indexed jump-table dispatch

Converted GotoStack (363) against `src/playsim/p_acs.cpp` and `FBehavior::Jump2PC`
in `src/playsim/p_acs.h`: the popped value indexes a jump table rather than
being a raw address. AcsProgram now accepts JumpPoints as byte offsets within
its Code. Registration bounds the table and validates destinations before
changing registered state, clones the table, and hashes it with program data.
Existing fibers retain their original table when a program is replaced.
Missing operands or invalid indexes terminate safely; unaligned targets work.

Added 14 regressions for all target alignments, index bounds, underflow,
lower-stack preservation, ownership, checksum differences, replacement
atomicity and active-fiber stability. Full Release suite: 3,016 passed;
warnings-as-errors build: zero warnings/errors.

Scope is explicitly registered managed programs. Native JUMP chunk loading,
module-relative address relocation and cross-module execution are not wired
into this API yet. This supersedes only the prior GotoStack dispatch gap;
full ACS parity, phases 1–3 and native invasion validation remain incomplete.

### Enhanced ACS JUMP chunk decoding and bounded relocation

Added MapBehaviorJumpTableCodec following native JUMP loading in
`src/playsim/p_acs.cpp`: the first JUMP chunk supplies ordered little-endian
module offsets, including an explicitly empty first table. Unknown chunks
are skipped after bounds checks. Results own their storage and are exposed
read-only; malformed input returns no partial table. Counts are bounded.

The relocation helper converts unsigned module offsets into byte offsets
within a supplied code range, using widened arithmetic and requiring room
for a complete word opcode. It retains unaligned destinations and rejects
targets before/after that range rather than silently binding other code.
Added 17 regressions covering duplicate/absent/empty chunks, unsigned offsets,
ownership, truncated headers/payloads, malformed tails/sizes, relocation
boundaries/alignment, and decode-to-GotoStack execution.
Full Release suite: 3,033 passed; warnings-as-errors build: zero warnings/errors.

The codec accepts an already-delimited enhanced chunk region. Automatic map
module registration, chunk-region extraction, cross-region jump targets,
ordinary branch relocation and compact opcode execution remain unintegrated.
This is the decoding/binding foundation, not complete BEHAVIOR loading or
full phases 1–3 completion. Native invasion validation remains outstanding.

### ACS module-relative branch addressing

Added AcsProgram.CodeBaseOffset for the original module address of a copied
code region. Source audit of `FBehavior::Ofs2PC` in `src/playsim/p_acs.h` and
branch cases in `p_acs.cpp` confirms that ordinary branch operands are module
offsets. Goto, conditional branches and case branches now subtract the code
base with widened bounds checks. Sorted case tables align their count using
the module position, rather than the copied region's local position.
Restart remains local to the script entry; pre-relocated GotoStack targets
remain local and are not relocated twice. Code-base metadata is validated,
copied and included in the program/fiber checksum.

Added 17 regressions covering the four ordinary branch forms, sorted-table
alignment for each code-base residue, indexed jumps, bounded restart loops,
out-of-region targets, invalid code regions and checksum changes. Full Release
suite: 3,050 passed; warnings-as-errors build: zero warnings/errors.

Callers still supply code regions and their original offsets explicitly.
Automatic module registration, cross-region execution, function/module calls,
compact opcode execution and native invasion runtime checks remain unfinished.
This closes addressing within a supplied region, not the full map integration
or phases 1–3 completion gates.

| Phase | Existing verified foundation | Work still preventing completion |
| --- | --- | --- |
| 1 — gameplay | XYZ movement, gravity/jump, flat-sector collision, timed states, damage/death, armor, buffered commands; current regressions pass | Native collision/dropoff/stacking/corner semantics; advanced geometry; complete actor state/action execution; player lifecycle/roster integration and remaining input commands |
| 2 — combat/AI | Managed weapon cadence, player damage dice/range and SSG vertical spread, pitch-aware traces, projectile definitions, 18 monster attack profiles, lost-soul charges, pain-elemental spawns, arch-vile attacks/resurrection, target-height floating, default slots and persistent per-actor hearing | Complete native state/action tables and boss death actions, arch-vile visual fire/remaining resurrection semantics, remaining soul/spawn semantics, obstacle float/path logic, sound compatibility/advanced alert rules, exact RNG/autoaim/refire/psprites/BFG and weapon-specific effects |
| 3 — maps/mods | Checked WAD/PK3 loading, Doom/Hexen/UDMF geometry and spawn flags, fractional planes, ordered overlays, basic DEHACKED, immediate lights plus fade/glow/strobe/flicker/stop and selected doors/floors/lifts/ceilings/crushers/stairs/teleport/exit specials | Remaining line/sector specials and advanced mover variants, remaining sector lighting/exact native lighting RNG and switch textures, full activation-side/monster/projectile rules, slopes/portals/3D floors, MAPINFO progression, resource namespaces/client consumption and complete DEHACKED/BEX/actor-definition execution |
| Shared release blocker | Failed snapshot builds preserve source state | Paging or a negotiated replacement snapshot protocol, atomic receive/apply, bounded tombstones, loss/reorder/late-join coverage; current 255-record / packet-size limits remain |
| Cross-phase validation | Full managed suite and native invasion policy compile | Running this native checkout with representative IWAD/maps/mods and comparing behavior; native class/resource negotiation and original invasion round/timer/enemy symptom still need real runtime confirmation |
| Later persistence phase | Backward-compatible v6 pose archive with fractional sector planes and actor pitch | Full actor membership/inventory/AI/projectiles/movers/ACS/invasion saves; pose archives are not complete savegames |

Earlier detailed reviews remain relevant: [foundation](HCDE_CSHARP_GAMEPLAY_FOUNDATION_AUDIT.md),
[combat/AI](HCDE_CSHARP_COMBAT_AI_AUDIT.md), [maps/mods](HCDE_CSHARP_MAPS_MODS_AUDIT.md),
[phase 2/3](HCDE_CSHARP_PHASE23_COMPLETION_AUDIT.md),
[movers/damage](HCDE_CSHARP_MOVERS_DAMAGE_AUDIT.md), and
[command buffering](HCDE_CSHARP_INPUT_BUFFER_AUDIT.md). New default weapon-selection
and immediate sound-alert coverage supersedes only those specific open items.

## Validation

- Release solution: **3,050 passed, zero failed/skipped**, 2,127 cases above baseline.
  Protocol 15; Gamedata 10; Transport 10; Master 1; RCON 6; MapLoader 396;
  Playsim 1,991; Client 12; Net.Core 351; Pregame 97; Server 61.
- The lighting pass initially saw a five-second timeout in
  `Pump_LiveSessionReceivesGuestClientInput`; it passed the targeted rerun and the
  subsequent full solution run. No timeout/test-disabling workaround was added.
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
