# Chunk 495 — Whole `P_AdjustFloorCeil`, `P_FindAboveIntersectors`, `P_FindBelowIntersectors`, and `P_DoCrunch` compat head (`p_map.cpp` 6523–6700).

## Scope

Whole `P_AdjustFloorCeil`, `P_FindAboveIntersectors`, `P_FindBelowIntersectors`, and `P_DoCrunch` compat head (`p_map.cpp` 6523–6700).

## Native ranges

- `src/playsim/p_map.cpp` **P_AdjustFloorCeil** 6523–6566 (44 lines)
- `src/playsim/p_map.cpp` **P_FindAboveIntersectors** 6574–6622 (49 lines)
- `src/playsim/p_map.cpp` **P_FindBelowIntersectors** 6630–6678 (49 lines)
- `src/playsim/p_map.cpp` **P_DoCrunch** 6686–6700 (15 lines)

**Credited native LOC:** 157 (tally `nativeLoc` 157).

## Managed

`VmPMapSectorIntersectors.cs`
Managed LOC (tally): 101.
Conversion commit: `7556875fb0ce907058bb8f8e380e5df698f35819`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `PassMobjMonsterCheck` / `BridgeSkipsFloorSync` / `RestorePassmobjFlag` / `FromPMoveMonster` / `OrPassmobjFlags` | `P_AdjustFloorCeil` **6523–6566** |
| `SkipNoclip` / `RequiresSolid` / `BlockDistOutOfRange` / `SkipSpecialPickup` / `SkipCorpseUnlessIce` / `SkipAlwaysBlockingPair` / `AboveIntersection` | `P_FindAboveIntersectors` **6574–6622** |
| `BelowIntersection` / `HideBloodWhenTypeLow` | `P_FindBelowIntersectors` **6630–6678** |
| `CrusherCompatSkipsCorpse` / `CrushRequiresGrind` / `PlayCrushSoundWhenIdle` | `P_DoCrunch` **6686–6700** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **157** |
| Net behavior-ported | **157** |


**Post-review ratio:** 101/157 managed/native ≈ 64% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapSectorIntersectorsTests.AdjustFloor_MidtexPassmobj`
- `VmPMapSectorIntersectorsTests.AdjustFloor_BridgeSkipsSync`
- `VmPMapSectorIntersectorsTests.Above_SkipsNoclip`
- `VmPMapSectorIntersectorsTests.Above_IntersectionBand`
- `VmPMapSectorIntersectorsTests.Below_IntersectionBand`
- `VmPMapSectorIntersectorsTests.Crunch_CompatSkipsCorpse`
- `VmPMapSectorIntersectorsTests.Crunch_DamageCadence`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 495 --to 495` (0 hard / 0 soft).

## Seams

`P_CheckPosition`, `P_FindFloorCeiling`, `FMultiBlockThingsIterator`, `P_DamageMobj`, and blood spawn are not hosted.

## Credit rationale

Credits 3DMidTex passmobj, bridge spawn-pos floor sync, intersector filtering, and crush compat corpse skip from `p_map.cpp` (6523–6700).
