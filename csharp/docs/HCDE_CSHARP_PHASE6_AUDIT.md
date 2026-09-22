# Phase 6 audit — firing

**Date:** 2026-09-22  
**Scope:** A fixed-damage attack from the selected weapon. Pistol and chaingun, shotgun and super shotgun, plasma and BFG, fist and chainsaw.  
**C++ references:** `p_pspr.cpp` for the trigger, `p_map.cpp` hitscan, `p_interaction.cpp` for armor absorption.  
**Result:** Accepted for this slice. Not those files. No C++ was deleted.

`dotnet test` in `csharp/`: **638 passed, 0 failed.** That is the Phase 5 total of 633 plus 5 combat tests. Local SDK is 10.0.201; `global.json` still pins 8.0.423, so the run moved that file aside and put it back.

## What a shot does

After movement and pickups, a player with `Attack` set fires the selected weapon. The pistol is selected at spawn. A weapon the player does not own does not fire.

Melee (fist, chainsaw) reaches 64 units past the target radius, only in front of the player, and costs no ammo. It deals 10. Hitscan weapons spend one unit of ammo even on a miss. Pistol and chaingun deal 10 and spend a bullet. Shotgun and super shotgun deal 30 and spend a shell. Plasma and BFG deal 20 and spend a cell. The ray is the facing direction. The closest solid circle in front is the hit. Range is 2048. There is no spread.

A player target with armor saves `damage * savePercent / 100`, limited by the armor left. The rest comes off health. Health stops at 0. The actor stays.

Checked: a pistol drops a monster at x=200 from 30 to 20 and bullets from 50 to 49. A monster behind the player is missed and the bullet is still spent. Empty bullets do not fire. A fist hits a monster at x=80 and misses one at x=200, with bullets unchanged. Green armor 100 at 33% saves 3 of a 10-point hit, so armor is 97 and health falls by 7.

Left out: random damage, shotgun pellets, rockets and projectiles, refire, weapon sprites, monster attacks, pain, and death.

## Verdict

The firing gate is in and tested. `p_pspr.cpp` and `p_enemy.cpp` stay. The renderer, the ZScript VM, Vulkan, and ZMusic are still the C++ engine.
