# Chunk 1464 — SpawnFriction, Set3DFloor, and Spawn3DFloors in specials.cpp (860-1036).

## Scope

SpawnFriction, Set3DFloor, and Spawn3DFloors in specials.cpp (860-1036).

## Native ranges

- `src/maploader/specials.cpp` **MapLoader::SpawnFriction** 860–1036 (177 lines)

**Credited native LOC:** 177 (tally `nativeLoc` 177).

## Managed

`VmSpecials1464Slice.cs`
Managed LOC (tally): 233.
Conversion commit: `0f456d8c65c839ac68f1aa84afa338e4018d249e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Friction and 3D floors (**860–1036**) | yes |
| Band hull (**860–1036**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| SpawnSpecials | ends at line 852 |
| SpawnLights | starts at line 1044 |

Friction length comes from the argument when it is set and is capped at 200, otherwise from the linedef length. Vavoom contents replace the solid flag and scale alpha. Hexen maps either store a line id or fold the high tag byte. Light-only floors use the three flag presets. A missing top texture is cleared after the tagged sectors are visited.

| Managed (`VmSpecials1464Slice.cs`) | Native (`specials.cpp`) |
|-------------------------------------|------------------------|
| SpawnFriction, Set3DFloor, Spawn3DFloors | **860–1036** |
| Band hull (exclusive slice) | **860–1036** |

### Chunk **1464** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **177** |
| **Net headline** | **+177** vs the prior chunk |


**Post-review ratio:** 233/177 managed/native ≈ 132% (src file LOC at HEAD; see tally).

## Tests

- `VmSpecials1464SliceTests.FrictionClampsAndContentsSwapsSolid`
- `VmSpecials1464SliceTests.HexenLineIdLightOnlyAndPlaneFlags`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1464 --to 1464` (0 hard / 0 soft).

## Seams

SpawnFriction through the end of Spawn3DFloors. Banners at 885-889 and 986-990 are stepped over. SpawnLights starts at 1044.

## Credit rationale

UNCredited specials.cpp 860-1036 (+177). One contiguous span. Each function alone is under 150 lines. The span does not cross credited or recognition lines.
