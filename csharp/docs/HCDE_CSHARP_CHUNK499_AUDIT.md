# Chunk 499 — P_DoCrunch blood tail, SpawnShootDecal, SpawnDeepSplash, P_ActivateThingSpecial

## Scope

P_DoCrunch blood tail, SpawnShootDecal, SpawnDeepSplash, P_ActivateThingSpecial

## Native ranges

- `src/playsim/p_map.cpp` **P_DoCrunch** 6679–6685 (7 lines)
- `src/playsim/p_map.cpp` **P_DoCrunch** 6701–6752 (52 lines)
- `src/playsim/p_map.cpp` **P_PushUp** 6822–6830 (9 lines)
- `src/playsim/p_map.cpp` **P_PushDown** 6886–6892 (7 lines)
- `src/playsim/p_map.cpp` **P_ChangeSector** 7107–7115 (9 lines)
- `src/playsim/p_map.cpp` **P_ChangeSector** 7238–7238 (1 lines)
- `src/playsim/p_map.cpp` **SpawnShootDecal** 7273–7296 (24 lines)
- `src/playsim/p_map.cpp` **SpawnDeepSplash** 7304–7318 (15 lines)
- `src/playsim/p_map.cpp` **P_ActivateThingSpecial** 7320–7383 (64 lines)
- `src/playsim/p_map.cpp` **P_ActivateThingSpecial** 7385–7391 (7 lines)

**Credited native LOC:** 195 (tally `nativeLoc` 195).

## Managed

`VmPMapCrushTailDecalActivate.cs`, `VmPMapCrushTailDecalActivateTests.cs`
Managed LOC (tally): 107.
Conversion commit: `ff76d11ef723b366907763575653de257e516cb8`.

## Tests

- `VmPMapCrushTailDecalActivateTests.Crush_DamageCadence`
- `VmPMapCrushTailDecalActivateTests.Crush_SkipInvulnBlood`
- `VmPMapCrushTailDecalActivateTests.Activate_DefaultSwitchAddsActivate`
- `VmPMapCrushTailDecalActivateTests.Decal_PrefersWeaponGenerator`
- `VmPMapCrushTailDecalActivateTests.DeepSplash_3dPath`
- `VmPMapCrushTailDecalActivateTests.Activate_SpecialUsesThingOnDeath`
- `VmPMapCrushTailDecalActivateTests.Activate_ClearOnSuccess`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 499 --to 499` (0 hard / 0 soft).

## Seams

`Spawn`, `P_DamageMobj`, `DImpactDecal::StaticCreate`, `P_HitWater`, and `P_ExecuteSpecial` are not hosted.

## Credit rationale

Credits crusher blood/splash policy, weapon-vs-actor decal selection, deep splash trace choice, and thing activation/switch/special dispatch from `p_map.cpp` (6679–7391) including EOF tail after `P_ChangeSector`.
