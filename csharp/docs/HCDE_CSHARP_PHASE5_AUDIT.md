# Phase 5 audit — pickups and floors

**Date:** 2026-09-22  
**Scope:** The first slice of the C#-only plan: vanilla touch pickups, pistol-start inventory, blockmap-limited movement, actor blocking, and an 8-unit floor raise, floor lower, and lift.  
**C++ references:** `a_pickups.cpp` for the gift amounts, `p_map.cpp` for the blockmap and thing collision, `a_floor.cpp` and `a_plats.cpp` for the special numbers.  
**Result:** Accepted for this slice. Not a port of those files. No C++ was deleted.

`dotnet test` in `csharp/`: **633 passed, 0 failed.** That is the Phase 4 total of 624 plus 9 Phase 5 tests. Local SDK is 10.0.201; `global.json` still pins 8.0.423, so the run moved that file aside and put it back. The projects still target `net8.0`.

## What a tick does now

`AuthoritySimulation.Tick` still runs thinkers first. After that it:

1. Puts a solid actor back to its previous position when its circle overlaps another solid.
2. Gives overlapping pickups to the first player who can take them, and removes the ones that were taken.
3. Activates linedef specials, including floor raise, floor lower, and lift.
4. Steps sector motions, then ACS, invasion, and the optional rewind, as in Phase 3.

A binary map load copies a decoded `BLOCKMAP` onto `PlayLevel.Blockmap` when that lump reads. UDMF levels leave it empty and still scan every line.

## Gates

### Inventory and pickups

A player starts with 50 bullets, fist, and pistol. Armor, shells, rockets, cells, and the other weapons start empty. Touch is a circle overlap after the tic, including standing on the item. A gift that does not change the player leaves the actor on the map.

| Thing | Effect |
| --- | --- |
| 2011 stimpack | +10 health, cap 100 |
| 2012 medikit | +25 health, cap 100 |
| 2014 health bonus | +1 health, cap 200 |
| 2013 soulsphere | +100 health, cap 200 |
| 2015 armor bonus | +1 armor, cap 200. Empty armor becomes 33% save. |
| 2018 green armor | Armor becomes 100 at 33% save, if current armor is below 100. |
| 2019 megaarmor | Armor becomes 200 at 50% save, if current armor is below 200. This is the blue combat armor. |
| 2007 / 2048 clip and box | +10 / +50 bullets, cap 200 |
| 2008 / 2049 shells and box | +4 / +20 shells, cap 50 |
| 2010 / 2046 rocket and box | +1 / +5 rockets, cap 50 |
| 2047 / 17 cell and pack | +20 / +100 cells, cap 300 |
| 5, 6, 13 cards and 40, 39, 38 skulls | One flag per color. A skull and a card of the same color are the same key. |
| 2005 chainsaw | The weapon, no ammo. |
| 2001 / 82 shotgun and super shotgun | The weapon and 8 shells. |
| 2002 chaingun | The weapon and 20 bullets. |
| 2003 rocket launcher | The weapon and 2 rockets. |
| 2004 / 2006 plasma and BFG | The weapon and 40 cells. |

A weapon already owned is taken only when its ammo can still rise.

Checked: a stimpack takes 90 health to 100 and stays when health is already 100. A medikit caps 90 at 100. A soulsphere takes 100 to 200. A health bonus takes 100 to 101. Green armor sets 100 at 33% and is refused at 100. Megaarmor replaces 100 with 200 at 50%. An armor bonus on empty armor is 1 at 33%. A clip takes the pistol start from 50 to 60 and stays at 200. A blue card is taken and the blue skull beside it stays. A shotgun grants the weapon and 8 shells. A chainsaw grants no shells. A second shotgun at full shells stays. Standing on a stimpack does not stop the player's step.

Left out: backpack, powerups, weapons firing, armor absorption on damage, dropped-item flags, and the rest of `a_pickups.cpp`. Inventory is not in the `HCSV` save.

### Blockmap and actors

When `PlayLevel.Blockmap` is set, a move tests the linedefs listed in the 128-unit blocks touched by the start and end circles. A blocking line that is not in those lists does not block. Linedef 0 is a real index. The opening 0 in a block list is the Doom dummy and is skipped. With no blockmap, every line is tested, which is the Phase 2 path.

Solids are pushed back to the position they held at the start of the tic. They do not slide along each other. Pickups, the teleport destination (type 14), and the invasion spot (type 3004) are not solid. The spot stays a normal actor, which is why the bot can walk off it.

Checked: a wall at x=40 stops a player at x=32 when the blockmap lists that line in the player's block, and does not stop him when the line is listed only in the next block. A solid at (50, 64) keeps the player at x=32. The invasion bot at (10, 12) still walks east on the second tic.

Left out: `P_TryMove`, step-up, dropoff, block-everything flags, and sliding along another actor.

### Floors

Specials 18, 19, and 62 use the vanilla walk-trigger numbers. The motion is an 8-unit step at 8 units per tic, not a search of neighboring floors. A raise stops at the ceiling. A lower goes 8 units down. A lift lowers 8, waits 5 tics, and returns. A sector already in motion is left alone. These specials are one-shot unless the repeat flag is set. Special 1 still repeats on its own.

Checked: crossing the line raises floor 0 to 8 and clears the special. The lower reaches -8. The lift is at -8 after one tic and back at 0 after six more.

Left out: next-higher floor, lowest neighbor, stairs, crushers, and the rest of `p_lnspec.cpp`.

## Verdict

Phase 5's pickup and floor gates are in and tested. `a_pickups.cpp`, `p_map.cpp`, and the floor thinkers are not replaced, so those C++ files stay. Combat is the next slice.
