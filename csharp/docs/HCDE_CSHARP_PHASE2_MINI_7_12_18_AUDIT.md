# Phase 2 mini-phases 7 and 12–18 audit

**Scope:** Build a level from a binary or UDMF map, spawn its things, move the player from usercmds, slide against blocking lines, tick an authority checksum, boot that level on `hcdeserv`, and put the ticked pose into the snapshot tail.  
**C++ references:** `maploader/udmf.cpp`, `p_setup.cpp`, `doomdata.h` (`ML_BLOCKING`), `actorinlines.h` (`P_IsBlockedByLine`), `g_game.cpp` (`cmd->forwardmove <<= 8`), `wadsrc/static/zscript/actors/player/player.zs` (`MovePlayer`), `doomdef.h` (`ORIG_FRICTION_FACTOR`).  
**Result:** Accepted for this subset after one audit fix. Not a full `P_SetupLevel`, `P_SpawnMapThing`, or `P_TryMove`.

`dotnet test` in `csharp/`: **596 passed, 0 failed.**

## 7 — Level build

`LevelBuilder` turns a decoded binary map or a UDMF text map into vertices, sectors, sides, lines, and things. `TryFromWad` uses `TEXTMAP` when the catalog format is UDMF and `BinaryMapDecoder` otherwise. The catalog stores the TEXTMAP lump under the Things kind, so the UDMF path reads that descriptor.

A binary sidedef index of `0xFFFF` becomes side `-1`. A line blocks movement when it is one-sided or has `ML_BLOCKING` (flag bit 0). UDMF `blocking = true` sets that bit. A two-sided line without the bit does not block in this subset.

Checked against the map data:

- The minimal `MAP01` wad keeps the player start at (100, 200), angle 90, type 1, the one-sided line from (0, 0) to (100, 0), sector light 160, and the `STARTAN2` middle texture.
- An empty `namespace = "Doom";` TEXTMAP builds an empty level.
- A UDMF blocking line and a two-sided open line keep their endpoints and the blocking bit.

Left out on purpose: nodes, segs, reject, blockmap, slopes, 3D floors, and polyobjects. Those lumps still decode elsewhere. They are not part of the playable level.

## 12 — Actor spawn

Thing type 0 is skipped. Types 1 through 4 become player pawns. The player number is the type minus one, matching player starts 1–4. Other types become actors on the default thinker stat. Players use stat 33.

A DEHACKED actor whose editor number matches the thing supplies health. Width and height are used when they are greater than zero. Width is already the radius, as `PatchThing` stores it. A missing or zero size falls back to radius 16 and height 56 for a player, and radius 20 and height 56 otherwise.

Map skill, ambush, and multiplayer bits are stored on the thing and not filtered.

## 13 — Player

`PlayerPawn.Tick` adds `yaw << 16` to the binary angle, then thrusts. That is the same scale as `Angle += cmd.yaw * (360/65536)` in `MovePlayer`: one yaw unit is 1/65536 of a circle. Angle 0 faces east.

The first draft treated the wire `forwardmove` as a classic Doom char and moved `command / 32` map units. `g_game.cpp` shifts that value left by 8 before it is sent. `MovePlayer` then multiplies by `Speed / 256` and `ORIG_FRICTION_FACTOR` (`2048/65536`). With Doom's `Speed 1` and `ForwardMove 1,1`, one tic from rest is `command / 8192` map units. Side thrust uses angle minus 90 degrees. The test `Player_ForwardAtAngleZeroMovesEast` uses command 8192 and expects one map unit east.

The pawn applies that thrust as a position change for the current tic. It does not keep velocity, friction, or air control. A later tic with no command stays put.

`SimulationCommandSink` copies `UserCmd` forward, side, and yaw onto the pawn. The host reads client input before the tick, so the command is in the same tic as the snapshot.

## 14 — Collision

`LineSlide` tests the destination circle against each blocking segment and also rejects a move whose center segment crosses the line. If the move hits, the remaining delta is projected onto the line and tried once. A move straight into a wall, with no component along the wall, stays put. A two-sided line without `ML_BLOCKING` does not block.

This is not `P_TryMove`. There is no blockmap, no actor-to-actor blocking, no step height, no dropoff, and no opening check. `ML_BLOCKEVERYTHING`, `ML_BLOCKMONSTERS`, and `ML_BLOCK_PLAYERS` are not consulted. One-sided lines block here because the back side is missing, which matches the walk result of a closed line opening.

## 15 — Authority tick

`AuthoritySimulation.Tick` runs the thinker list, which advances the tic clock, then recomputes a checksum from the tic, the RNG seed, and each actor's id, X, Y, and health. The checksum changes when the clock advances and when the player moves.

This hash is not `SnapshotChecksumMixer`. The net checksum still hashes health, sector heights, and the rolling hashes. Position reaches clients through the pose in the snapshot tail.

## 16 — Headless map boot

`DedicatedServerHost` builds an `AuthoritySimulation` from `IwadBytes` and the pregame map name when that map decodes. `Pump` ticks it. No renderer or audio is involved. `HeadlessMapBoot.TryBoot` is the same load plus one tick for tests.

The constructor does not tick. The first `Pump` is tic 1. Existing pregame tests still use the minimal `MAP01` wad. They still reach a live session, and sector light 160 still arrives.

## 17 — Sim snapshots

After the tick, the host copies sector floor, ceiling, light, and special, actor health, and the player position and yaw into the live session's world store. `WorldStateTailBuilder` writes `GuestPlayerState.PosX`, `PosY`, and `YawBams`. Those fields stay 0 until something sets them, so tails built from older stores keep their previous pose bytes.

`ApplyPose` now keeps the position and yaw it reads. A publish after a northward thrust changes the coop tail bytes and the stored Y.

The host only writes the store created by its own live-session bootstrap. It does not replace a store installed on some other session.

## 18 — In-process acceptance

`Acceptance_MinimalMapLoadsSpawnsAndTicks` loads the minimal `MAP01` wad, finds the type 1 thing, ticks, and checks that the checksum changes.

These were not executed:

- `tests/netcode_step12/netcode_step12_stress.py`
- `tests/mbf21_validation/validate_mbf21_stage1.py`
- `tests/id24_validation/validate_id24_smoke.py`

They need the C++ client and an IWAD. The repository has no MBF21 or ID24 fixture wad. A ZScript `PASS` is still Phase 3–4. Phase 2 sign-off is not complete.

Left out on purpose: weapons, inventory, monsters, specials, ACS, friction, step-up, and client prediction.
