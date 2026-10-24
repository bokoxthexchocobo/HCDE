# Chunk 487 — SpawnMapThing unknown-type head, filters, and invasion/sound tail

## Scope

SpawnMapThing unknown-type head, filters, and invasion/sound tail

## Native ranges

- `src/playsim/p_mobj.cpp` **FLevelLocals::SpawnMapThing** 6569–6663 (95 lines)
- `src/playsim/p_mobj.cpp` **FLevelLocals::SpawnMapThing** 6768–6833 (66 lines)

**Credited native LOC:** 161 (tally `nativeLoc` 164).

## Managed

`VmPMobjSpawnMapThingBands.cs`, `VmPMobjSpawnMapThingBandsTests.cs`
Managed LOC (tally): 84.
Conversion commit: `c36df4c3c46a5cd1687634081f773f5f7a398739`.

## Tests

- `VmPMobjSpawnMapThingBandsTests.Head_RejectsZeroEdNum`
- `VmPMobjSpawnMapThingBandsTests.Head_PolySpawnSkipped`
- `VmPMobjSpawnMapThingBandsTests.Mask_DeathmatchUsesDmFlag`
- `VmPMobjSpawnMapThingBandsTests.Seq_Clamp255ToNegativeOne`
- `VmPMobjSpawnMapThingBandsTests.Invasion_DelayMinimumTenSeconds`
- `VmPMobjSpawnMapThingBandsTests.Tail_BlocksNotDmMatchInDeathmatch`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 487 --to 487` (0 hard / 0 soft).

## Seams

`Printf`, `Net_*` invasion hooks, `Spawn`, and `DoomEdMap` are not hosted.

## Credit rationale

Credits ednum/unknown-thing handling, game/skill mask gates, poly/deathmatch start routing, sound-sequence override, invasion pickup spots, and shareware frame fallback from `p_mobj.cpp` (6569–6663, 6768–6833).
