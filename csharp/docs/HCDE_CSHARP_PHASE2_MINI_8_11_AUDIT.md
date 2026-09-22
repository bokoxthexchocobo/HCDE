# Phase 2 mini-phases 8–11 audit

**Scope:** MAPINFO boot fields, DEHACKED thing/frame/sound patches, 16.16 fixed point and the tic clock, thinker stat order.  
**C++ references:** `g_mapinfo.cpp`, `d_dehacked.cpp`, `m_fixed.h`, `m_round.h`, `basics.h`, `dthinker.cpp`, `statnums.h`.  
**Result:** Accepted for this subset after one audit fix. Not a full MAPINFO or DEHACKED port.

## 8 — MAPINFO

Accepted fields: map lump, level name (`lookup` and `$`), `levelnum`, default level numbers for `MAP##` and `ExMy`, `next` / `secretnext` / `secret`, `sky1` / `sky2` with the `TICRATE / 1000.0` speed scale, and cluster name, enter/exit text, flat, pic, hub, and music.

Checked against the C++ parser:

- A numeric `map` header becomes `MAP%02d` and turns on the Hexen hack. Numeric `next` is then `&wt@%02d`, and a cluster created only by that map is a hub.
- A cluster block that already exists is reset. A later Hexen map reference does not force `hub` back on. The first draft did; the audit test `TryParse_ExistingClusterDefinitionIsNotForcedIntoAHub` covers the fix.
- Hexen sky speeds are divided by 256 before the tic scale.
- `E1M10` keeps ID24 level number 10 and old level number 0, matching `GetDefaultLevelNum`.

Left out on purpose: `defaultmap`, episodes, skills, gameinfo, intermissions, cutscenes, end-sequence names, music order, and string-table matching for IWAD cluster text.

## 9 — DEHACKED

Thing patches apply health, reaction time, pain chance, width, height, speed, missile damage, bits, editor number, alert/attack/pain/death/action sounds, and the standard state labels. Width and height use `value / 65536`. Speed stays the raw integer, as in `PatchThing`. Thing 1 ignores close and far attack frames. Frames 47 and 48 keep their original 5 and 4 tic durations unless `Duration` replaces them. Sprite subnumbers above 63 are reported and masked with `0x3f`. Bit `0x8000` is fullbright.

`Sound` blocks are read and discarded. That matches the current `PatchSound`, whose body is commented out. BEX `[SOUNDS]` renames are applied. Actor sound fields keep the 1-based number written in the patch; `DehFindSound` subtracts one only when it indexes the engine sound map.

The built-in actor list is the first 12 Doom thing slots (player through imp). Thing numbers outside that list are errors unless a caller passes a larger baseline. DSDHacked actor synthesis is off, matching `dsdhacked == false`.

Left out on purpose: code pointers, ammo, weapons, sprites, text, cheats, bits names, and MBF21 frame args.

## 10 — Fixed point

`Fixed` is 16.16. `FromDouble` uses half-even rounding, the same mode as `llrint` in `RoundHalfEven`. `FromInt`, `ToInt`, multiply, and divide follow `m_fixed.h`. Divide by zero throws instead of invoking undefined behavior. `BamAngle` uses `0x40000000` for 90 degrees. `GameTicClock.TicRate` is 35.

## 11 — Thinkers

`ThinkerCollection.Run` ticks stats 32 through 127, then drains fresh thinkers until none remain. A just-spawned thinker gets `PostBeginPlay` before `Tick`. Thinkers spawned during the pass are ticked before `Run` returns. Destroyed thinkers skip `Tick`. Stats below 32 are stored and not ticked.

`Run` also advances `GameTicClock` by one. In C++ that increment lives in the game loop, not in `RunThinkers`. The clock is advanced here so a headless server has one tic per pass.

Left out on purpose: traveller lists, serialization, client-side thinkers, and the profiler path.
