# Phase 3 audit — simulation subset

**Date:** 2026-09-22  
**Scope:** Headless-server gates from [`HCDE_CSHARP_MIGRATION.md`](HCDE_CSHARP_MIGRATION.md) Phase 3: line specials, an ACS subset, bots, save/load, compat flags, invasion, authority rewind, and in-engine RCON.  
**C++ references:** `p_spec.cpp` special numbers, `p_acs.cpp` word pcode layout already walked by `MapBehaviorBytecodeWalker`, `p_saveg.cpp` as the archive idea, `d_net_rcon.cpp` nonce auth, `d_net_rewind` keyframes, `d_net_invasion` waves.  
**Result:** Accepted for this subset. Not a port of those C++ files.

`dotnet test` in `csharp/`: **612 passed, 0 failed.** That is the previous 596 plus 16 Phase 3 tests. Local SDK is 10.0.201; `global.json` still pins 8.0.423, so the run moved that file aside and put it back. The projects still target `net8.0`.

## What the tick does now

`AuthoritySimulation.Tick` still runs thinkers first. After that it:

1. Activates linedef specials crossed since the actor's previous position.
2. Steps sector door motions.
3. Resumes ACS fibers.
4. Spawns an invasion wave when that director is enabled.
5. Captures a rewind keyframe when rewind is enabled.
6. Rebuilds the checksum and a status string.

The checksum now mixes sector floors and the exit flag as well as tic, seed, and actor pose. Phase 2 movement tests still pass.

Binary and UDMF level build copy linedef special, tag, and (UDMF) args onto `LevelLine`.

## Gates

### Line specials

| Special | Behavior |
| --- | --- |
| 1 door raise | Floor moves toward ceiling − 4 at 8 units/tic, waits 5 tics, closes. Repeats. A sector already in motion is left alone. |
| 11 door open | Same motion, stays open, one-shot. |
| 52 exit | Sets `Exited`. One-shot lines clear their special. |
| 70 teleport | Moves the activator to the first thing of type 14. |
| 80 ACS execute | Script number is arg0, or the line tag when arg0 is 0. Runs on the same tic. A missing script does not clear the line. |

Tag 0 uses the front side's sector. Any other tag selects sectors with that tag. A failed compat check does not clear the special.

Checked: a player at x=32 crossing a vertical line at x=32.5 opens a ceiling-12 sector to floor 8 and, six tics later, the door is shut. Exit clears the special. Teleport lands on the dest thing. Special 80 with tag 1 exits on that tic.

Left out: switches, walkover vs gun, locked doors, elevators, floors, stairs, teleporters with tags, and the rest of the special table.

### ACS

Word-format opcodes: nop, terminate, suspend, push number, add, subtract, delay-direct, lspec1-direct, goto, if-goto, drop. Anything else ends the fiber. Delay N skips N later tics before the fiber resumes. A fiber is capped at 64 instructions per resume. The stack underflow ends the fiber.

Checked: push 2, push 3, add, drop, delay 1, then exit. The level exits on the third tic, not the first. A crossed special 80 starts script 1 in the same tic.

Left out: the rest of `p_acs.cpp`, strings, inventory, world/map variables, little-endian enhanced bytecode execution (the walker still only decodes it), and ZScript `PASS`.

### Bots and invasion

`BotPawn` thrusts forward with command 2048 each tic and slides on the same line test as the player. `AddBot` gives it the next actor id.

Invasion is off until `Invasion.Enabled` is set. The first enabled tic spawns one bot per map thing of type 3004, or one bot at (64, 64) when there is no such thing. The next wave waits 35 tics. An exit stops further waves.

Thing type 3004 is still spawned as a normal actor by the Phase 2 spawner. Invasion adds bots at those coordinates on top. The spots are not removed from the actor list.

Checked: a bot placed at (32, 32) moves east. An invasion spot at (10, 12) becomes a bot on tic 1 and walks on tic 2.

Left out: monster AI, pain, attacks, the C++ invasion budget, and skill filters.

### Save/load

`HCSV` version 1 stores tic, exit flags, actor id/x/y/angle/health, and sector floor/ceiling. Restore matches actors by id and does not spawn or delete them. A bad magic or a truncated body is rejected. Door motions are not in the file. A restored floor stays where it was saved; a still-live motion can keep moving on the next tic.

Checked: after a move and a door step, a second move is undone by the archive, including floor 8 and tic 1.

Left out: `p_saveg.cpp` thinkers, inventory, RNG, scripts, and hub state.

### Compat facades

| Flag | Effect |
| --- | --- |
| MBF21 | Special 270 raises the tagged floor by 8, once. Without the flag the line stays. |
| ID24 | Special 243 is a secret exit. Without the flag it does nothing and the line stays. |
| Eternity | Sector special 200 sets `ReverbActive` at level start. No audio is played. |

Checked: each flag is required, and the Eternity flag is inert when the sector special is absent from the caller's compat set.

Left out: MBF21 dehacked, ID24 map translation, and the Eternity mixer.

### Rewind

When enabled, each tic appends an `HCSV` blob, capped at 8. `RestoreOldest` applies the first blob. Clients are not told. Default off, so Phase 2 checksum tests do not capture frames.

Checked: two forward tics, then the oldest frame puts the player back on the first tic's x. The ring still holds both frames.

Left out: client resync, projectile rewind, and the C++ lag-comp bracket.

### RCON

`InEngineRconServer` listens on loopback. The nonce and FNV-1a auth match the existing `hcdercon` client. Allowlist: `ping`, `status`, `map`. `exec` and every other verb return `ERR command not allowed`. A bad password returns `ERR auth failed` and no command runs.

`status` reads `StatusLine`, which is replaced at the end of the constructor, `Tick`, and `RestoreState`. The RCON thread does not walk actor fields.

`hcdeserv` starts that listener only when `--rcon-password` is set and a map simulation exists. `--rcon-port` defaults to an ephemeral port when it is 0.

Checked: ping, status containing `map=MAP07`, map, denied `exec`, and a wrong password. The CLI stores the password and port.

Left out: kick, map change, say, and a remote bind. The listener is loopback only.

## Verdict

Phase 3's server gates are in and tested. The C++ playsim, the ACS VM, savegames, compat specs, invasion, and rewind are not replaced. The client subset is a separate audit: [`HCDE_CSHARP_PHASE4_AUDIT.md`](HCDE_CSHARP_PHASE4_AUDIT.md).
