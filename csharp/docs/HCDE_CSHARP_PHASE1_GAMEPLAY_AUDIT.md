# Phase 1 gameplay audit

Date: 2026-09-27. Scope: the five gameplay-foundation gates in
[`HCDE_CSHARP_MASTER_PLAN.md`](HCDE_CSHARP_MASTER_PLAN.md). This is a source and
regression review of the managed playsim. It is not a native engine trace.

**Verdict: phase 1 is not complete.** None of the five gates can be checked off.
The managed movement, damage, inventory, and command paths below are real
subsets. They do not match `P_TryMove`, the actor state table, the player
roster, or a recorded native session.

### Follow-up, 2026-09-28

Five named holes now have a bounded implementation and tests. They do not close
their gates.

- Dropoff: monsters stop when the destination floor is strictly more than 24
  below the floor they are in. Players, projectiles, and floating actors cross.
  A drop of exactly 24 is allowed. The test is the sector under the actor
  center. Radius-only ledge contact, `P_SlideMove` corners, riders, corpses,
  slopes, portals, and 3D floors are still absent. Checksum includes the new
  flag and height. The save pose does not. Restore keeps them only because it
  edits the live actor.
- ENTER: each spawned player starts every ENTER script with `ACS_ALWAYS` and
  one zero argument, after OPEN is queued. Maps with no player still leave
  ENTER idle. Directory order is unchanged.
- Respawn: a player death queues `SCRIPT_Death` and waits 35 tics. A fresh use
  or attack press then asks single-player to reload the map, or revives a coop
  or deathmatch pawn at that player's start. Deathmatch inventory returns to
  the pistol start. The corpse is not stood up in single-player, and the map
  is not actually reloaded. Cooperative and single-player respawn run the
  inventory filter. Every lose flag defaults off, so the pack stays. Lose
  inventory is a pistol start. Lose keys, weapons, or armor drops that
  piece. Lose ammo puts bullets back at 50 and zeroes the other pools.
  Halve ammo divides a pool above 1, and bullets do not fall under 50.
  Deathmatch ignores the filter. A deathmatch revive uses a thing-11
  start when the map has one. Twenty blocked tries still use the last
  spot, and that spot does not telefrag. `sv_spawnfarthest` picks the
  start farthest from another living player. Level load still uses player
  starts. A map with no thing 11 still revives at the player's own start.
  `sv_norespawn` stays off until set. While it is set the corpse stays, and
  a buffered press or a forced deathmatch revive runs on the tic it clears.
  `sv_weapondrop` is off until set, and then a death
  leaves the selected weapon's map thing. Fist and pistol leave nothing.
  While dead, the view falls toward 6
  and the body turns at most 5 degrees a tic toward the killer.
- Overkill: a killing blow subtracts the full post-armor remainder, so health
  can stay negative. Extreme death is selected only when health is strictly
  below `-spawn health` and that state exists. The default table has no
  extreme-death frame, so those actors still use the normal death state.
  A monster with no armor still takes the full hit. The save
  pose already stores health and now keeps the negative value.
- Use: the trace follows the player's yaw for `UseRange` (default 64). The
  back of a line activates only when `playeruseback` is set. A front-only
  line that is open does not eat a switch farther along the trace. Portals,
  puzzle items, sector use actions, and `ML_BLOCKUSE` are not part of this
  trace. `UseRange` is checksummed and is not in the save pose.
- Corners: a blocked player step moves up to the nearest wall along that step,
  clips the into-wall velocity, and clips once more if a second wall blocks
  the slide. The east wall can be first in the line list and the player still
  follows the north wall when that one is closer. Monsters and missiles keep
  the single clip. Bounding-box traces and icy bounce are absent.
- Stacking: a grounded player steps onto a solid non-player when that top is
  within 24 and the head still fits, then stands there instead of falling
  through. A top of 25 blocks. A low ceiling blocks. A monster does not step
  up. Corpses are not platforms. A static sector pinch applies 10 crush
  damage every four tics when headroom is below height.   Mover crushers propagate crush damage to actors standing on the
  blocked carrier. Sector pinch crush does the same. A carrier's step delta
  moves standing riders horizontally and vertically. `MF2_ONMOBJ` is set on
  an actor support floor above the sector floor and relaxes dropoff while set.
  Rising sector floors carry grounded actors through post-mover fitting; carriers
  are fitted before `OnMobj` riders.   A per-sector scroll vector carries grounded actors after thinkers run.
  `Scroll_Floor` (223) sets carry from line args when mode is positive.
  Hexen sector specials 201–224 add player-only carry on top of that vector.
  Map-load <c>Scroll_Floor</c> (223) carry lines register via
  <c>ScrollCarryInitialize</c> (multiple thinkers sum per sector). Displacement carry
  (`Arg1` &amp; 3) tracks control-sector center height each tic. Texture scroll and polyobjects are absent. `Scroll_Ceiling` (224) activates on
  tagged sectors but does not scroll ceilings yet. Carry samples the center sector
  plus radius cardinals and averages multi-sector scroll like native players.
  Per-scroller `ScrollCarryAffect` (players / monsters / static) filters carry
  like `DScroller::m_Affect`; default is all kinds. `CompatSurface.BoomScroll`
  matches `COMPATF_BOOMSCROLL` (monsters sum multi-sector carry; players average).
  `MF8_INSCROLLSEC` is set per tic for monsters on carry scrollers; players ignore it.
  `CallFunc` ACS helpers include `CheckClass` (200), `DamageActor` (201),
  `GetActorVelX`/`Y`/`Z` (9–11), `GetActorViewHeight` (14), `GetChar` (15),
  `GetArmorType` (19), `CheckActorProperty` (22), `SetActorVelocity` (23),
  `CheckActorClass` (27), `UniqueTID` (46),
  `IsTIDUsed` (47), `Sqrt` (48),
  `FixedSqrt` (49), `VectorLength` (50), `strcmp` (63), `stricmp` (64),
  `StrLeft`/`StrRight`/`StrMid` (65–67), `GetActorClass` (68), `GetWeapon` (69),
  `CheckProximity` (98, catalog radius), `CheckActorState` (99, Spawn/See/Pain/Death labels),
  `SoundVolume` (70, no-op success), `PlayActorSound` (71, actor count only),
  `SpawnDecal` (72, actor count only), `CheckFont` (73, built-in Doom font names),
  `DropItem` (74, catalog pickup spawn; optional amount override),
  `SetLineActivation` / `GetLineActivation` (76–77, cross/use/use-back/use-through),
  `GetActorPowerupTics` (78, `PowerBuddha` on players),
  `CheckFlag` (75), `SetActorFlag` (202),
  `Floor`/`Round`/`Ceil` (207–209, fixed quantize), `GetActorFloorTexture` (204), `StrArg` (206, `SpawnOptions.StrArgs`),
  `ChangeActorAngle` (79), `ChangeActorPitch` (80), `GetArmorInfo` (81), `DropInventory` (82, strip only),
  `ChangeActorRoll` (89), `GetActorRoll` (90), `PickActor` (83, `CombatTrace` ray + TID assign),
  `IsPointerEqual` (84, `AAPTR` subset), `CanRaiseActor` (85, archvile corpse gates),
  `SetActorTeleFog` / `SwapActorTeleFog` (86–87, per-actor fog name overrides),
  `SetActorRoll` (88, absolute BAM roll; shares **89** `ChangeActorRoll` path),
  `LineAttack` (60, `CombatTrace.PickActor` + `ActorDamage`), `PlaySound` (61, actor count only),
  `StopSound` (62, no-op success), `QuakeEx` (91, no-op success),
  `SetSectorDamage` (94, tag match via `SectorDamage.ApplyTagged`),
  `SetSectorTerrain` (95, floor/ceiling name on tagged sectors),
  `GetActorFloorTerrain` (205, reads `FloorTerrain`), `SpawnParticle` (96, no-op success),
  `SetMusicVolume` (97, stores `MusicVolume`; playback absent) —
  catalog / `DoomPlayer`; `UniqueTID` (46) random start when tid **0**
  spawn names only, not full DECORATE/`PClass`. Backpack give/take via ACS inventory
  opcodes matches `GiveBackpack` / `RemoveBackpack`.
- Bridges: a grounded monster steps onto a solid actor marked
  `MF4_ACTLIKEBRIDGE` when that top is within 24 and the head fits. A top
  of 25 blocks. A dead actor with the flag is not a platform. Players
  already step onto any solid non-player. A carried rider is absent.
- Ice corpses: an actor with `MF_ICECORPSE` steps onto a corpse whose top
  is within 24 and stops against a taller one. A player and a living
  monster still walk through that corpse. Shatter and blood are absent.
- Crouch: holding BT_CROUCH lowers crouchfactor by 1/12 each tic, down to
  0.5, and height and view height follow it. Releasing stands back up when
  the taller body fits. A jump while crouched does not leave the ground; it
  locks a stand-up. Death restores the spawned height. A fresh BT_TURN180
  turns 20 degrees a tic for 9 tics and ignores yaw. Holding it does not
  start another turn. Crouch sprites, fake floors, alt attack, reload,
  zoom, and run are absent.
- Monster armor: a non-player uses the same integer save as the player.
  Green percent 33 saves `damage / 3`. The amount caps the save, and a
  depleted amount clears the percent. A monster with no armor still loses
  the full hit. Players keep using pistol-start inventory. `MaxFullAbsorb`
  is saved in full before the percent, and `MaxAbsorb` stops the running
  total. Both stay 0 on green and mega, which is the older formula. A new
  suit clears the caps and keeps the count. Drowning damage skips armor.
  A stored pickup stays while the worn amount is at least its save amount.
  When the worn amount reaches 0 the percent is cleared and the highest
  save percent is used. An equal percent keeps the earlier pickup. Doom
  green and mega still replace or reject and are not stored. Other damage
  types still use armor.
- Buddha: `MF7_BUDDHA` leaves health at 1 when a hit would kill. A hit of
  1,000,000 and `DMG_FORCED` still kill. `DMG_FOILBUDDHA` kills a monster
  and does not kill a player. Armor is applied first. `CF_BUDDHA2` is a
  player flag. It also survives a telefrag and `DMG_FORCED`, and that
  forced hit no longer skips armor or god mode. `PowerBuddha` lasts 60
  seconds. While it remains, a killing blow leaves health at 1. A
  telefrag and `DMG_FORCED` still kill. A second grant does not extend
  the timer until 128 tics or fewer remain. An inflictor with
  `MF7_FOILBUDDHA` kills a monster Buddha. A null inflictor does not, and
  the source's flag does not count unless that actor is the inflictor.
  Players ignore the inflictor flag.
- Drain: a player with drain strength 0.5 gains `int(strength * post-armor
  damage)`, capped at 100. Health already at or above 100 is left alone.
  `MF5_DONTDRAIN` and a self hit grant nothing. Strength 0 is off. There
  is no PowerDrain item and no heal sound.
- Backpack: thing 8 lifts bullets, shells, rockets, and cells to
  400/100/100/600 and gives 10, 4, 1, and 20. A later backpack only adds
  those amounts, and it is still taken when every pool is full. A pistol
  start clears it. Tossing it drops the caps back to 200/50/50/300, cuts
  ammo above those caps, and leaves a depleted pack. Picking that up
  restores the caps and gives no ammo. A player who already has a pack
  still gets ammo from a depleted one. There is no drop-inventory key.
- Skill ammo: baby and nightmare multiply ammo, weapon ammo, and backpack
  amounts by 2. The other skills stay at 1. `sv_ammofactor` multiplies that
  and the product truncates toward zero. A dropped weapon skips the factor.
  Health, armor, and the ammo caps are left alone. `sv_doubleammo` stays
  off until set, then replaces the skill factor with 2. It does not stack
  on baby. Heretic and Hexen use 1.5 and are absent.
- Weapon raise: a slot press stores `PendingWeapon` and leaves the ready
  weapon in place. The offset lowers by 6 from 32 to 128. The bottom tic
  commits the pending weapon and does not also raise. Fire is refused until
  the offset returns to 32. A later press replaces the pending weapon.
  Pressing the ready weapon does not cancel it. A spawned weapon and a
  direct assignment start ready. A revive clears the pending weapon.
  `CF_INSTANTWEAPSWITCH` finishes the handoff at 32 on that same tic, so
  an attack queued with the press can fire. Sprites and flash are
  absent.

## Gate 1 — collision, dropoff, stacking, corners

Native references: `P_TryMove`, `P_CheckPosition`, `P_XYMovement`, and
`P_ZMovement` in `src/playsim/p_map.cpp` and `src/playsim/p_mobj.cpp`.

What exists is `ActorPhysics` in `csharp/src/HCDE.Playsim/ActorPhysics.cs`.
It moves a cylinder in steps of at most 2 units, slides along one blocking
line, steps up by `MaxStepHeight` (default 24), and rejects a ceiling that the
actor cannot fit under. Overlapping actors block when their vertical intervals
overlap and both have `BlocksActors`.

The dropoff subset now follows the native comparison in `P_TryMove`: refuse
when `floorz - dropoffz > MaxDropOffHeight` unless the actor may cross.
`MaxDropOffHeight` defaults to 24. `AllowDropOff` stands in for `MF_DROPOFF`
on players and for `MF_MISSILE` on projectiles. `Floating` stands in for
`MF_FLOAT`. `NoGravity` does not grant a crossing. The inequality is strict, so
a drop of 24 passes and a drop of 25 does not.

That is still not the native test:

- `dropoffz` here is the floor of the sector under the actor's current center.
  Native tracks the lowest floor contacted while the radius straddles a line.
  A monster can therefore overlap a tall ledge without being stopped, as long
  as its center has not entered the low sector. A point outside every sector
  skips the check.
- `COMPATF_CROSSDROPOFF`, `MF2_ONMOBJ`, `MF5_AVOIDINGDROPOFF`, and
  `MF5_NODROPOFF` are not represented.
- Player corner contact now follows `FSlide::SlideMove` for two clips. The
  wall is the line the center reaches first, then `HitSlideLine` removes the
  into-wall velocity (vertical clears X, horizontal clears Y). A second wall
  gets one retry. The trace is the actor center, not the three bounding-box
  corners, and there is no blockmap. Ice does not bounce. Monsters and
  missiles are not on the `MF2_SLIDE` path, so they keep the single clip
  against the first blocking line. An actor blocked by another actor still
  splits the step on X, then Y.
- Player stacking follows the `P_TryMove` step onto a non-player. The top has
  to be within `MaxStepHeight` of the feet, and the ceiling has to clear the
  player's head. The player then uses that top as the floor, including a fall
  onto it. A grounded monster does the same only for `MF4_ACTLIKEBRIDGE`.
  A top of 25 still blocks. Corpses stay non-solid for ordinary movers, so
  a player walks through one. `MF_ICECORPSE` is the exception from
  `P_TestMobjZ`: that actor steps onto a corpse of height 24 and is stopped
  by a corpse of height 56. A carried rider list is absent.
- Slopes, portals, and 3D floors are not implemented. Sector lookup is a ray
  cross of every line, not the BSP or blockmap. One-sector synthetic maps fall
  back to sector 0 even when the point is outside the geometry.

Step-up, actor blocking, the flat dropoff cases, the player two-wall slide,
the player step onto a short actor, the monster step onto a bridge, and the
ice-corpse step onto a corpse are covered by managed tests. Static sector
pinch crush is covered. Full stacking crush and riders are not
matched. Gate 1 stays open.

## Gate 2 — actor states, actions, and player lifecycle

Native references: actor `Spawn`/`See`/`Melee`/`Missile`/`Pain`/`Death`/`Raise`
blocks under `wadsrc/static/zscript/actors/doom`, and player spawn/death/respawn
in `src/playsim/p_mobj.cpp` and `src/playsim/p_user.cpp`.

Managed actors have a small state machine used for pain, death, and a few
monster attack timers. The status audit already records that this is not the
native state table: chase, look cadence, pain durations, and raise animations
are still timers or skipped. Player starts are editor numbers 1–4. There is no deathmatch start queue and
no intermission roster.

ENTER scripts now start when a player is spawned. Native does this in
`p_mobj.cpp` for `PST_ENTER` and for a live player who is not being restored
from a save, using `StartTypedScripts(SCRIPT_Enter, p->mo, true)` so the call
is `ACS_ALWAYS` with one zero argument and `runNow` false. The managed start
uses `ExecuteAlways` after the OPEN queue, in script-number order, once per
spawned player. A map with no player leaves the script idle. Save restore does
not start it again. Number order is not the native directory walk.

Death and respawn follow `player.zs` `DeathThink` and `G_DoReborn` for a
subset. The killing transition queues every `SCRIPT_Death` with `ExecuteAlways`
and one zero argument, and sets the earliest respawn tic to the current tic
plus 35. A use or attack command whose button was up on the previous command
arms the respawn. A button held across the killing tic does not. Weapon
choices and movement in a dead player's command are discarded, so they are
not replayed if health is restored. When the clock reaches the earliest tic,
single-player sets `ReloadRequested` and leaves the corpse where it fell.
`AllowSinglePlayerRespawn` revives instead. Cooperative and deathmatch revive
the same pawn at the thing whose type is `PlayerNum + 1`, or the first player
start if that one is missing. Deathmatch instead uses thing 11 when any
exist. Health returns to the spawn health, velocity is
cleared, and the spawn state is entered. Deathmatch calls
`ResetToPistolStart`. Cooperative and an enabled single-player respawn call
`FilterCoopRespawn`. `ForceRespawn` skips the
press in deathmatch only. `SCRIPT_Respawn` is started after that revive.
Pickup and return scripts stay registered and idle.

The filter is `FilterCoopRespawnInventory` for this pistol-start pack.
`dmflags` starts at 0, so every lose flag is off and the pack is kept.
Lose inventory is `ResetToPistolStart`. Lose keys drops all three, unless keys are shared. Sharing copies a
cooperative key pickup to every other player, including a dead one, and
then lose-keys leaves those keys in place. Deathmatch does not share.
Losing the whole pack still clears keys. A card and a skull of the same
color are one key. Puzzle items are absent. Lose weapons keeps fist and pistol. Lose armor
clears the amount and the percent, because there is no default armor.
Lose ammo sets bullets to 50 and the other pools to 0. Halve ammo divides
a pool only when its amount is above 1. Bullets then stay at least 50.
A pool of 1 stays 1. Lose ammo wins when both ammo flags are set. A
shotgun that can no longer fire is replaced by the pistol, or by the fist
when the pistol has no bullet. A backpack stays unless everything is lost.
Powerups are absent. Deathmatch does not call the filter.

Deathmatch respawn follows `G_DeathMatchSpawnPlayer` for the reborn only.
Thing 11 is not spawned as an actor. The pick uses a separate counter, not
the native `DMSpawn` table and not the combat rolls. An open spot is one
no living solid actor overlaps. Twenty blocked tries return the last spot.
The revive then telefrags a monster standing there. Deathmatch also
telefrags another player, and that hit goes through god mode and
invulnerability. Cooperative leaves the other player alone. The overlap
is a square of the two radii, and a body completely above or below is
spared. `NoTelefrag` skips the hit unless `AlwaysTelefrag` is also set.
A shootable actor with no monster brain is not a monster. Level load
does not stomp. `SpawnFarthest`
is `sv_spawnfarthest`. It applies only when another player is alive, and a
tie keeps the earlier start. A miss falls through to the random thing-11
pick. Cooperative respawn ignores thing 11. A deathmatch map with none
still uses the player's own start. Level load still spawns on player starts.

`NoRespawn` is `sv_norespawn`. It stays off. While it is set, the wait can
finish and the press stays buffered, and neither the reload request nor a
forced deathmatch revive runs. Clearing it on a later tic uses that press,
or the force flag, without asking for another button.

Not in this subset: `G_InitNew` or an autoload after the reload request,
and the coop reborn native does
on level exit. `ReloadRequested`, `AllowSinglePlayerRespawn`,
`ForceRespawn`, `NoRespawn`, the six coop lose flags, `SpawnFarthest`, `DmSpawnRandomState`, `RespawnArmed`, `RespawnEarliestTic`, and the attack-held edge
are in the checksum and not in the save pose. A restore keeps them only
because it edits the live actor. `UseHeld` is in the pose. The attack edge is
not, so a pose alone does not restore it.

While the pawn is dead, `DeathThink` lowers view height by 1 toward 6 and
pitch by 3 toward 0, then turns at most 5 degrees toward
`LastDamageSourceId`. A killing blow with no source, or a blow dealt by the
pawn itself, does not turn. An ice corpse still turns and does not drop the
view or the pitch. Respawn restores the standing view and zeroes pitch.
`WeaponDrop` copies `sv_weapondrop`. It stays off, so a death leaves no
thing. When it is on, chainsaw through BFG spawn that weapon's map thing
at the corpse and the inventory is kept. Fist and pistol have no vanilla
thing, so they spawn nothing. The pickup pays the catalog amount, not the
ammo in hand. The flag is in the checksum and not in the pose.

Damage and poison fade counters are absent. View height is already in the
checksum and not in the pose, so a save from before the drop does not put
it back. Angle is in both.

Single-player spawning still requires the thing's single-player flag, including
a player start. Native player starts are not dropped for a missing mode bit.
An ENTER script does not run for a start the spawner rejects.

Gate 2 stays open.

## Gate 3 — input commands and activation

Native references: `ticcmd_t` / `usercmd_t` in `src/g_game.h` and `src/d_protocol.h`,
and use/cross activation in `src/playsim/p_map.cpp`.

`PlayerCommand` carries forward, side, yaw, pitch, attack, jump, use, and a
weapon-selection list. Those are buffered and applied on the simulation tic.
Weapon slot, next, and previous follow the default Doom slot table in
`PlayerInventory`.

Use follows `P_UseLines` for a flat map. The trace starts at the player and
runs along yaw for `UseRange`, which defaults to 64. A hit on the back side
activates only when the line has `SPAC_UseBack` (`playeruseback` in UDMF).
A UseBack-only line does not activate from the front. `SPAC_Use` and
`SPAC_UseBack` eat the press. `SPAC_UseThrough` keeps the trace going. An
open front-only line does not consume the press, so a switch past it can
still run. The opening test is the hitscan window at chest height, not
`P_LineOpening`. `ML_BLOCKUSE`, `COMPATF_USEBLOCKING`, sector use actions,
puzzle items, and portals are absent. Doom specials still activate from the
front without the `PlayerUse` flag, because the special number selects the
action. Hexen's three-bit activation field has no back-use bit, so those
lines stay front-only unless `PlayerUseBack` is set.

`UseRange` and the three use flags are in the checksum. `UseRange` is not in
the save pose, so a restore keeps a non-default range only on the live actor.
The line flags live on the map, which the pose does not rebuild.

Crouch follows `PlayerPawn.CheckCrouch` and `CrouchMove`. The button is
`BT_CROUCH` (bit 3) on the user command. Factor moves by `1/12` and clamps
to `[0.5, 1]`. Height is `FullHeight * factor`. View height is `41 * factor`.
Standing up calls the move test at the new height and keeps the crouch when
the ceiling is too low. Jump is checked first: a crouched player stands
instead of jumping, and that stand-up stays locked until a crouch press.
Death sets the factor back to 1. `CrouchFactor`, `FullHeight`, `ViewHeight`,
and the stand-up lock are in the checksum and not in the save pose. Height
itself is already checksummed. A same-actor restore keeps the live height.

A slot press sets `Pending` and leaves `Selected` on the ready weapon.
`A_Lower` adds 6 to the psprite Y until 128. That bottom tic copies the
pending weapon into `Selected`, clears the pending weapon, and does not
also raise. `A_Raise` then subtracts 6 until 32. A second press replaces
the pending weapon and keeps lowering from the current offset. Pressing
the ready weapon leaves the pending switch alone. Cycling uses the pending
weapon when one is set. Fire is refused while the offset is away from 32,
including the attack queued with the switch. A spawned pistol starts at 32,
and assigning `Selected` directly does not play the animation, so a test
that sets the weapon and fires on the next tic still hits. Respawn snaps
the offset back to 32 and clears the pending weapon. `CF_INSTANTWEAPSWITCH`
snaps that lower to the bottom and brings the new weapon up to 32 in the
same tic, so the attack on that command can fire. `WeaponOffsetY`, the
lowering bit, `Pending`, and the instant-switch flag are in the checksum
and not in the pose.

A fresh `BT_TURN180` sets `TurnTicks` to 9. That tic and the next eight
each add 20 degrees and ignore yaw. Holding the button does not start
another turn. Releasing it and pressing again does. A revive clears the
ticks and the held bit. `TurnTicks` and the held bit are in the checksum
and not in the pose.

View bob uses horizontal speed squared times 0.25, capped at 16. The
camera term is that amount times `sin(BobTimer / 20 * 360) * 0.5`.
Standing still is 0. Eye height is unchanged. The timer is not stored; a
pose restore rebuilds it from the saved clock tic.

Weapon bob is `Bob_Normal` at the end of the tic. The same horizontal
amount swings X by its cosine and Y by the absolute sine. The angle is
`BobTimer * 128 * 360 / 8192` degrees. Timer 16 is straight up: X is 0
and Y is 16. Timer 0 is straight aside: X is 16 and Y is 0. A weapon
that is lowering or firing stays at 0, because `wbobfire` is 0. Vertical
speed is not included. Other bob styles, the between-tic blend, and the
3D bob are absent.

Still absent from the command path: alt attack, reload, zoom, run, custom
slots, and psprite sprites and flash. Gate 3 stays open.

## Gate 4 — health, overkill, and inventory

Native references: `P_DamageMobj` in `src/playsim/p_interaction.cpp`, armor in
`wadsrc/static/zscript/actors/shared/inventory/armor.zs`, and the player pawn
inventory in `doomplayer.zs`.

`ActorDamage.Apply` subtracts health after armor and can enter a pain state.
`Pain.Fire` is used when that frame is registered. A missing frame uses
the normal pain state. A per-type pain chance replaces the actor's chance
for that name only. The match is ordinal. `PainThreshold` defaults to 0.
A surviving hit whose post-armor amount is below it does not flinch.
An amount equal to it does. Armor is subtracted first: 30 damage through
green armor deals 20, so a threshold of 21 stays quiet. `MF6_FORCEPAIN`
on the inflictor flinches anyway, and it also skips the pain roll. The
source's flag does not count unless that actor is the inflictor. A hit
that armor absorbs completely still flinches. `MF5_NOPAIN` on the target
and `MF5_PAINLESS` on the inflictor block the flinch, including a forced
hit. Painless wins when the inflictor also forces pain. The source's
painless flag does not count. `DMG_NO_PAIN` still blocks. A no-pain monster
still wakes. These flags only gate the pain frame.
`DamageType Electric` rolls the flicker stream after the pain chance. A roll
below 96 enters the pain frame and clears fullbright. A miss sets
`RF_FULLBRIGHT` and skips the flinch. Forced pain still flinches. Poison
howling is absent. A typed `Wound` frame replaces pain when health is at
or below `WoundHealth` after the hit. That early-out skips wake on that
tic. The match is ordinal.
Green armor uses integer division by 3 so that 3 damage saves 1. Other
percents use `damage * percent / 100`. `MaxFullAbsorb` is saved in full
before that percent, and `MaxAbsorb` caps the running total. Both default
to 0, so green and mega keep the older save. A replacement suit clears the
caps and leaves `AbsorbCount`. The remainder is subtracted in full.
`DamageType Drowning` is the only `NoArmor` type. A sector of that type,
and a direct hit tagged `Drowning`, skip armor. `Slime` and a different
capitalization still use it. God mode still blocks the hit.
Health may stay negative. `GibHealth` defaults from spawn health
(`GetGibHealth` is `-SpawnHealth`). A result strictly below that threshold
enters `ExtremeDeathState` when the actor's state table actually has that
frame. The default four-frame table does not, so the actor uses the normal
death state and the negative health remains. Health equal to the threshold is
not an extreme death. Players absorb from `PlayerInventory`. A non-player
absorbs from its own `Armor` and `ArmorSavePercent`, using that same formula,
which is the monster branch of `P_DamageMobj` calling `AbsorbDamage`. A
former human with no armor, hit for 80 from 30 health, still ends at -50.
Green armor of 100 on that hit saves 26 and leaves health at -24. When the
amount reaches 0 the percent is cleared, matching `BasicArmor`. `DMG_NO_ARMOR`
is `BypassArmor`. A player suit that reaches 0 clears its percent. A stored
`BasicArmorPickup` then replaces it. The count of saved armor stays.
`MF7_BUDDHA` is `Actor.Buddha`. The health assignment is clamped to 1
before the setter runs, so the death state and the player death note do
not fire. The comparison uses the raw hit, which is `TELEFRAG_DAMAGE`
(1,000,000), not the remainder after armor. `DMG_FORCED` skips armor,
invulnerability, and Buddha. `DMG_FOILBUDDHA` removes Buddha from a
non-player only. `CF_BUDDHA2` is `PlayerPawn.Buddha2`. It survives a
telefrag and `DMG_FORCED` by the same clamp to 1. The forced flag is
cleared first, so armor still applies and god mode still blocks the hit.
`PowerBuddha` is `PlayerPawn.PowerBuddhaTics`. Duration -60 is 2,100 tics.
While that count is above 0, the player follows ordinary Buddha: a killing
blow stops at 1, and a telefrag or `DMG_FORCED` still kills. `DMG_FOILBUDDHA`
does not remove it from a player. A second grant is taken without extending
the timer while more than 128 tics remain. At or below that line the timer
resets to 2,100. Each player tic, including death, counts down. A deathmatch
revive and a cooperative lose-inventory revive clear it. A cooperative
revive that keeps inventory leaves the remaining tics. `MF7_FOILBUDDHA`
is `Actor.FoilBuddha`. On the inflictor it kills a non-player Buddha. A
null inflictor does not, and the source's flag does not count unless that
actor is also the inflictor. Players, including PowerBuddha and Buddha2,
ignore it. Direct hits and blasts pass the missile. Melee, hitscan, a
charge, and the archvile's direct hit pass the attacker. BFG spray still
has no puff, so it does not read a puff's foil flag. A non-killing hit
still subtracts normally. A player
with `DrainStrength` gains `int(strength * post-armor damage)`, capped at
100. Health already at or above 100 does not rise. `DontDrain` and a self
hit grant nothing. A killing blow uses `Death.Extreme.Fire` when that
frame exists and health is past the gib line, otherwise `Death.Fire`
even past that line, otherwise the gib frame, otherwise the normal
death. The type name is ordinal, so `fire` is not `Fire`. Damage type
`Extreme` forces the gib frame. An Ice kill with no Ice death uses
`GenericFreezeDeath` for a player or a monster, and that frame is not
extreme. A typed Ice death still wins. `MF4_NOICEDEATH`, a decoration,
and the name `ice` fall through to the gib frame. `MF4_EXTREMEDEATH` on
the inflictor can pick the gib frame without passing the gib line.
`MF4_NOEXTREMEDEATH` blocks every extreme frame, including past the gib
line, and a typed extreme death falls back to the plain typed frame. The
source's flags do not count. A shootable frozen corpse shatters on a hit
that is not blocked ice: ice damage from an inflictor without
`MF7_ICESHATTER` does nothing. Shatter zeroes velocity, sets
`MF6_SHATTERING`, and snaps the state to one tic. Shatter spawns deterministic
`IceChunk` debris with combat RNG when the corpse is on a simulation. Chunk
heads, terrain melt, and sounds are absent.
Voodoo dolls
are not implemented. A non-player with a brain clears `ReactionTics` on a
post-armor hit or forced pain, sets chase target to the source, and enters
`SeeState` from spawn when that frame exists. Pain runs first, so a
flinch blocks see on that tic. Wake-up reloads chase `Threshold` from
`DefThreshold` when the source is already the target or when
`OkayToSwitchTarget` allows it. A positive threshold blocks switching
until it counts down on an enabled brain tick. `MF4_QUICKTORETALIATE` may
switch while threshold is still positive. `MF4_NOTARGETSWITCH` blocks a new
target while the current one is set. `MF4_NOHATEPLAYERS` blocks wake-up against
players but not monsters. `MF7_NEVERTARGET` blocks chase on the
source. `MF_FRIENDLY` and `FriendPlayer` drive a subset of `IsFriend` for
wake retargeting and `MF_JUSTHIT`. Teamplay and designated teams are absent.
`TIDtoHate` and `MF3_NOTARGET` drive a subset of `OkayToSwitchTarget` and
`CanAttackHurt`: shared hate ids block teammates, a shooter may hurt an actor whose `ThingId` matches `TIDtoHate`. Wake-up with
infighting off still needs hostility; at standard infighting the hated tid can
override same-species blocks. `NOTARGET` is ignored for that hated tid or when
the source is hostile. `pr_switcher` can keep the current chase target when its
`ThingId` matches `TIDtoHate`, line of sight holds, and the roll is below 128.
TID look iterators are absent. Wake-up stores `lastenemy` when
switching targets, and keeps a living player there across later monster
switches. The brain tick resumes a living non-friend `lastenemy` after
player scan when nothing else is acquired, then clears the slot. Goals and
look states are absent.
`MF_JUSTHIT` is set on a pain flinch when the source is the chase target
or there is no chase target. A wound or electric fullbright does not set
it. A chase target aimed at someone else still allows it when that target is
not a friend. A friendly chase target blocks `MF_JUSTHIT` from a third
source.
The brain clears it on the next enabled tick. `MF5_NOINFIGHTING` and the
level `infighting` cvar gate wake-up against non-players when infighting
is off. `IsHostile` is a subset: two plain monsters are not hostile, and
opposing friendlies with different `FriendPlayer` values are. At standard infighting (`0`), wake-up and monster damage both block the same
`DoomEdNum` species unless `MF6_DOHARMSPECIES` is set or the source is hostile.
With infighting off, monster damage is blocked unless the victim is hostile to
the shooter. `MF7_FORCEINFIGHTING` upgrades a level with infighting off to
standard rules. With infighting off, monster-monster damage stays gated by
`MF3_ISMONSTER`; barrels and other non-monsters are not. `MF7_HARMFRIENDS`
lets a shooter hurt friendlies at standard infighting. Projectile groups and
deathmatch teamplay are absent. `IsMonster`, `HarmFriends`, and `TidToHate` are
in the checksum and not in the pose. A non-player extreme death clamps health to `GibHealth - 1` when
the post-hit amount is still on or above the gib line. Players keep
overkill. A player extreme death sets `CF_EXTREMELYDEAD`. A plain typed
death does not. Revive clears it.

The pose archive already stored health as an int32. Restore used to clamp it
at 0, which would have dropped overkill on load. It now writes the saved value
back. `GibHealth`, `ExtremeDeathState`, the non-player armor fields,
`Buddha`, `FoilBuddha`, `Buddha2`, `PowerBuddhaTics`, `SpareArmor`, `DontDrain`, `DrainStrength`, `HasBackpack`, `IgnoreAmmoSkill`,
`AmmoFactor`, `DoubleAmmo`, and the four ammo
caps are in the checksum and not in the pose. They survive a restore
only because the restore edits the live actor.

`PlayerInventory` is a pistol start: 50 bullets, fist, pistol, four ammo pools,
three keys, and the nine Doom weapons. Pickups cover stimpack through BFG,
green and mega armor, armor bonus, and the backpack (thing 8). The first
backpack sets the Doom caps 400/100/100/600 and gives 10 bullets, 4 shells,
1 rocket, and 20 cells. Another one only adds those amounts, including when
the pools are already full, and the thing is still taken. `ResetToPistolStart`
clears the flag and the caps, so a deathmatch revive loses the backpack.
Tossing the pack does the same to the caps and cuts ammo that is over
the pistol-start maximum. The thing on the floor is depleted: the next
pickup restores the caps and adds nothing. A depleted pack still adds
ammo when the player already has one. There is no drop-inventory key.
Ammo gifts, weapon ammo, and backpack amounts use the Doom skill factor:
2 on baby and nightmare, 1 on the others. `AmmoFactor` is `sv_ammofactor`
and defaults to 1. `DoubleAmmo` is `sv_doubleammo`. It stays off. While
it is set, every Doom skill uses factor 2 instead of its own factor, and
`AmmoFactor` still multiplies that. A factor of 0 leaves
the pickup on the map. A dropped weapon sets `IgnoreAmmoSkill`, so collecting
it pays the catalog amount. Health, armor, keys, and the ammo caps are not
scaled. There is no actor inventory list, no custom items, and
no DEHACKED max-ammo rewrite of this object. Non-player actors do not have
that inventory. Their armor is the two fields above, not a pickup list.

Gate 4 stays open.

## Gate 5 — native traces

The master plan's last phase 1 gate is tick-level comparison with a native
build, not another managed fixture. No IWAD, native `hcde` binary, command
line, or trace pair is recorded for movement, damage, or a player life. The
managed suite can replay its own checksum. That does not satisfy the gate.

The Release playsim suite is green aside from unrelated projects. Phase 7 ACS
coverage now implements stack divide-by-zero as zero, `ThingCountDirect`,
`ThingCountSector` (sector tag filter), `ThingCountName` / `ThingCountNameSector`
(Doom spawn names via bound BEHAVIOR or program string tables), `PlayerCount`,
start-of-tic `Timer`,
`SinglePlayer`, `PlayerInGame`, `PlayerIsBot` (always false until player-slot
bots exist), `IsNetworkGame` (always false on local authority), `GameType`, `GameSkill`, activator queries (`ActivatorTid`, `PlayerHealth`, `LineSide`),
and a player-inventory subset (`PlayerFrags`, skull-tag key opcodes,
`CheckInventory` / `CheckInventoryDirect`, and activator
`GiveInventory` / `TakeInventory` direct/stack forms, TID give/take/check, and
`ClearInventory` / `ClearActorInventory` on players),
`SetActorProperty` / `GetActorProperty` for a bounded `APROP_*` subset (health,
ambush, invulnerable, friendly, no-target, spawn health, mass, step/dropoff,
target TID via monster brain; ambush, target TID, no-target, spawn health,
invulnerable damage block, mass, max step/dropoff height, and no-target wake
blocking have regression tests),
activator `UseInventory` and `UseActorInventory`
for owned weapons (pending raise only; tid 0 hits every spawned player) and
activator health-, armor-class, and `Backpack` `UseInventory` / `GiveInventory`
(no keys via use or item instances),
`GetAmmoCapacity` / `SetAmmoCapacity` on activator ammo pools, and
`ACSF_GetMaxInventory` (93) via `CallFunc` (DECORATE max amounts still absent),
and `CheckWeapon` / `SetWeapon` on the ready weapon class name.
Gate 5 still needs a recorded native IWAD/binary trace pair, not managed-only
fixtures. The warn-as-error Release build is clean.

A managed baseline fixture (`ManagedGameplayTraceTests`, documented in
`HCDE_CSHARP_PHASE1_NATIVE_TRACE.md`) pins map/seed/tic count and checksum for
future native diff. It does not close the gate.

Gate 5 stays open.

## What would have to be true to close phase 1

1. Corner slide and stacking each name a `p_map.cpp` path and a fixture.
   The player slide fixture is in place: the farther wall is listed first,
   and the player still advances along the nearer one. The stacking fixture
   is in place: a top of 24 is a step and a top of 25 blocks. A monster
   steps onto a bridge of height 24 and stops at 25. An ice corpse steps
   onto a corpse of height 24 and stops at 56. Static sector pinch crush
   is in a separate fixture. The flat dropoff fixture is
   also in place: a monster
   stops at a drop taller than 24, and a player does not.
2. Player death, respawn, and the roster follow `p_user.cpp` / `p_mobj.cpp`
   for the supported game modes. ENTER startup, the 35-tic wait, the fresh
   press, the single-player reload request, and the coop/deathmatch revive
   are in place. Cooperative respawn keeps the pack until a lose flag is
   set, and deathmatch still ignores that filter. A deathmatch revive uses
   a thing-11 start when the map has one. `sv_norespawn` holds the revive
   until the flag clears. A revive telefrags a monster on the spot, and
   deathmatch telefrags another player there too. Cooperative key sharing
   copies a key pickup to the other players and keeps those keys when
   lose-keys is set. The map is not reloaded,
   and there is no roster. Level load does not stomp.
3. The command set matches the native fields this port claims, or the
   unsupported fields are rejected instead of ignored. The flat use trace
   now has range, yaw, and side. Crouch changes height and view height.
   A slot change lowers the old weapon, commits the new one at the bottom,
   and raises before it can fire. Instant switch finishes that handoff on
   the same tic.    A fresh turn-180 button turns 20 degrees a tic for 9 tics.
   View bob follows horizontal speed. The ready weapon bobs in the normal
   style and holds still while it fires or lowers. Alt attack, reload,
   zoom, run, and psprite sprites are still absent.
4. Non-player armor uses the same integer save as the player when the
   monster actually has armor. A bare monster's overkill is still not
   reduced. `MaxFullAbsorb` and `MaxAbsorb` follow BasicArmor, and both
   stay 0 unless set. Drowning damage skips armor. A stored armor pickup
   replaces a suit that reaches 0. Ordinary Buddha stops a killing
   blow at 1 health. Drain returns half of a post-armor hit when the
   player has strength 0.5, and it stays off at strength 0. A backpack
   lifts the four ammo caps and gives one pack; a pistol start clears it.
   A tossed pack is depleted: it restores the caps and gives no ammo.
   Baby and nightmare double those ammo amounts. `sv_doubleammo` uses 2
   on every skill. A dropped weapon does not.
   Buddha2 survives a telefrag and forced damage. PowerBuddha lasts 2,100
   tics and stops a killing blow. An inflictor foil flag kills a monster
   Buddha and leaves a player alone.    A typed death uses that frame
   ahead of the gib frame when the table has it. A typed pain frame is
   used the same way, and that type can carry its own pain chance.
   `PainThreshold` blocks a flinch when the post-armor hit is smaller.
   An inflictor with forced pain flinches through that threshold and
   through a failed pain roll. A no-pain target and a painless inflictor
   still block that flinch. Electric pain can fullbright instead of
   flinching. A typed wound frame replaces pain when health is low enough.
   An Ice kill with no Ice death uses the generic freeze frame for a
   player or a monster. Inflictor extreme-death flags can force or block
   the gib frame. Negative overkill and
   the gib threshold are in place.
5. One recorded native session and the matching managed run agree on position,
   health, and inventory at the same tics.

Until those exist, the phase 1 boxes in the master plan stay unchecked.
