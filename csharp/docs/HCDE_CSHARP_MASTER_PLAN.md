# HCDE C# conversion master plan

Updated: 2026-10-03.

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

- Release solution: **5,413 passed; zero failed or skipped**.
- Release build with warnings as errors: zero warnings/errors.
- Tests by project: Playsim 4,286; MapLoader 533; Net.Core 351; Pregame 97;
  Server 65; Protocol 15; Client 12; Gamedata 29; Transport 10; RCON 6; Master 1;
  Scripting 8.
- Historical baseline: 923 tests at `55f8fa46`; 4,490 additional cases since then.
- October sync incorporated four upstream commits through `c367f081`, including
  ACS binding, gameplay parity, pickups/drops and inventory work. The current
  continuation adds read-only actor height/radius property queries and checks,
  followed by configurable ACS JumpZ integrated into player jumping and ACS
  actor gravity integrated into vertical acceleration, including ice defaults,
  configurable ACS player view height distinct from changing eye height, and
  ACS attack offsets integrated into hitscans and player missile origins, plus
  incoming/outgoing ACS damage multipliers before armor and script-configurable
  monster melee range with native distance and vertical boundaries, followed
  by ACS actor friction applied to the managed ground velocity decay and
  NoTrigger suppression of automatic movement line crossings and independent
  signed ACS actor score storage. The pointer-property audit adds TracerTID
  reads and corrects TargetTID to native query-only behavior, including missiles.
  ACS Speed now feeds player thrust, managed monster chase speed and projectile aim.
  ACS Damage now controls the base used by projectile actor and geometry impacts.
  ACS Dropped now exposes the marker initialized on spawned pickups. ACS
  ReactionTime now reads/writes the existing monster reaction countdown and gates
  player yaw/thrust/jumping while the counter decrements. Reaction state is
  actor-owned and survives brain removal/replacement. Raise animation waits
  now use a separate counter and no longer overwrite ACS ReactionTime. Revived
  monsters copy supported friendship/hate fields and clear targets before raising.
  Damage retaliation preserves living remembered enemies under the native hate-TID
  policy and updates memory when acquiring a target after the current target clears.
  Chase thresholds now clear immediately for dead/missing targets and count down
  for living targets, including non-shootable actors and nonzero negative values.
  AI ticks now preserve accepted damage-retaliation decisions instead of overriding
  rejection policies with an unconditional last-attacker target.
  Chase and hearing now reject friends, including targets whose friendship changes
  during an attack and friends retained in remembered noise. Visual player
  acquisition also skips friends, choosing a farther eligible player or idling.
  Supported map monsters and spawned lost souls now receive IsMonster; explicit
  DEHACKED COUNTKILL changes set/clear the map actor classification. Spawned souls
  also inherit supported friendship and hate settings from their pain elemental.
  Soul target copying rejects NoTarget/NeverTarget and records valid targets in
  hearing memory before charging. Pain-elemental death bursts now clear
  friendliness when the retained target is a friend, and targetless deaths no
  longer substitute the last damage source. Skull charge initiation preserves
  actor-owned ReactionTime instead of zeroing it. The charge helper now accepts
  configurable speed with native nonpositive fallback and vertical travel scaling.
  Soul target copying and initial charge no longer require damage eligibility,
  allowing dead/non-shootable/invulnerable targets while excluding destroyed ones.
  Zero horizontal charge velocity now ends skull flight and clears all velocity,
  unless NoAutoOffSkullFly is set; that flag is exposed and checksum-covered.
  Skull collision damage now uses actor Damage with lost-soul default 3 on map,
  bot and pain-elemental spawns. Explicit DEHACKED Missile damage now initializes
  map actor Damage, including zero/negative values and chained patches. Explicit
  DEHACKED Reaction time also initializes actor-owned counters before brain attachment.
  DEHACKED Mass now parses signed assignments and overrides map actor catalog mass.
  Explicit DEHACKED Bits now also initialize NoGravity, Dropped, Solid and Shootable.
  Patched Ambush combines with the map spawn flag and gates hidden noise acquisition.
  Patched Float and DropOff initialize existing movement behavior independently of gravity.
  Patched SpawnCeiling places map actors below the ceiling and subtracts Z offsets.
  DEHACKED Pain chance now uses native signed 16-bit narrowing. Speed assignments
  decode native fixed-point magnitudes of 256 or greater and preserve fractions.
  Explicit DEHACKED Width/Height retain zero, negative and fractional dimensions at spawn.
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

Full audit, 2026-09-27: **not complete.** See
[`HCDE_CSHARP_PHASE1_GAMEPLAY_AUDIT.md`](HCDE_CSHARP_PHASE1_GAMEPLAY_AUDIT.md).
Movement, damage, pistol-start inventory, and the buffered command subset are
implemented. A flat dropoff limit, per-player ENTER startup, negative
overkill health, a flat use trace, and a bounded player respawn were added
after that audit and are still subsets. The respawn waits 35 tics, then a
fresh press asks single-player to reload or revives coop and deathmatch at
the player start. The map is not reloaded. A blocked player slides along the
nearest wall and can clip a second wall once. A grounded player can stand on
a solid actor whose top is within the step height. Holding crouch shrinks
the player to half height; a jump while crouched stands them up. A monster
with armor uses the same integer save as the player. Ordinary Buddha
stops a killing blow at 1 health. A weapon slot change lowers and raises
before that weapon can fire. A grounded monster steps onto a solid
bridge whose top is within the step height. An ice corpse steps onto a
short corpse; other actors walk through it. A dead player turns toward
the killer and the view falls to 6. Weapon drop stays off until asked,
then leaves the selected weapon's map thing. A draining player gains
half the post-armor hit, up to 100 health. A backpack lifts the four
ammo caps and gives one pack; another backpack only adds ammo. A tossed
pack is depleted and gives no ammo when picked up again. A
cooperative revive keeps that pack until a lose flag is set. A
deathmatch revive uses a thing-11 start when the map has one.
`sv_norespawn` holds that revive until the flag clears. Baby and
nightmare double ammo pickups. `sv_doubleammo` replaces that
factor with 2 on every skill. Armor can save a full slice before
its percent, then stop at a total cap. Drowning damage skips armor. Buddha2 leaves a player at 1 health
through a telefrag and through forced damage. PowerBuddha lasts 60
seconds and stops a killing blow the way ordinary Buddha does. An
inflictor with the foil flag kills a monster Buddha and leaves a player
alone. A stored armor pickup replaces a suit that reaches 0, best
save percent first. A slot press keeps the old weapon until the lower
reaches the bottom. Instant switch finishes that handoff on the same
tic. A fresh turn-180 button takes nine tics and ignores yaw.
View bob follows horizontal speed and leaves eye height alone.
The ready weapon bobs in the normal style and holds still while it
fires or lowers. A revive telefrags a monster standing on the spot,
and deathmatch telefrags another player there too. Cooperative key
sharing copies a key to the other players. A typed death uses that
frame when the table has it. A typed pain frame does the same, and
that type can carry its own pain chance. A pain threshold blocks a
flinch below that post-armor amount. Forced pain on the inflictor
flinches through that threshold. A no-pain target and a painless
inflictor still block the flinch. An Ice kill with no Ice death uses
the generic freeze frame for a player or a monster. Inflictor
extreme-death flags can force or block the gib frame. Monster damage
wake clears reaction time and can enter see from spawn. Electric pain
can fullbright instead of flinching. A typed wound frame replaces pain
when health is low enough. A pain flinch can set just-hit. A non-player extreme death clamps to
gib minus one. A frozen corpse can shatter when shot. A player extreme death sets
extremely dead. Chase threshold, no-target-switch, and quick retaliate gate wake retargeting.
Static sector pinch crush. Friendly `IsFriend` gates wake and just-hit. Last-enemy memory on wake
switch. Full stacking crush, psprite
sprites, and a native tick trace are not.

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
   Word-format progress, 2026-09-27: a Hexen map's ACS\0 lump now registers its
   scripts at simulation start and queues OPEN scripts. The boot tic can change
   a sector light, including a branch whose target is a module address past an
   early terminate. Enhanced, compact, and redesigned headers are rejected.
   Closed scripts are registered and not auto-started. ENTER scripts start once
   per spawned player, with ACS_ALWAYS, after the OPEN queue. Death and respawn
   scripts start on those events, also with ACS_ALWAYS and one zero argument.
   Pickup, return, and the other known types still wait. This does not load libraries, functions, map
   arrays, or compact bytecode, and a jump from one script into another
   script's bytes still stops. The gate is only partly met.
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

- Audit follow-up: DEHACKED Thing decimal-prefix parsing and unsigned floating values are converted. Large native dimensions still exceed managed signed 16.16 actor storage and require a representation conversion. Live-session server timeout intermittency remains unresolved.
