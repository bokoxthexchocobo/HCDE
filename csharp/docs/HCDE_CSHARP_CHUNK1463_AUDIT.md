# Chunk 1463 — MapLoader::SpawnScrollers copy list and scroll thinkers in specials.cpp (1199-1434).

## Scope

MapLoader::SpawnScrollers copy list and scroll thinkers in specials.cpp (1199-1434).

## Native ranges

- `src/maploader/specials.cpp` **MapLoader::SpawnScrollers** 1199–1434 (236 lines)

**Credited native LOC:** 236 (tally `nativeLoc` 236).

## Managed

`VmSpecials1463Slice.cs`
Managed LOC (tally): 291.
Conversion commit: `f6526bdd69caf24ab7195450fd52723aeb78deb3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| SpawnScrollers (**1199–1434**) | yes |
| Band hull (**1199–1434**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Banner | lines 1193-1198 stay uncredited |
| CreateScroller | starts at line 1437 |

Copy scrollers are collected first and skipped when the front sector already has that tag. Ceiling, floor, and model lines take displacement from the linedef when that bit is set, and the floor carry keeps the sign of dx. Cardinal wall scrolls restore the special. A texture-both line with a nonzero first argument adds no thinker and leaves the special cleared.

| Managed (`VmSpecials1463Slice.cs`) | Native (`specials.cpp`) |
|-------------------------------------|------------------------|
| Spawn | **1199–1434** |
| Band hull (exclusive slice) | **1199–1434** |

### Chunk **1463** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **236** |
| **Net headline** | **+236** vs the prior chunk |


**Post-review ratio:** 291/236 managed/native ≈ 123% (src file LOC at HEAD; see tally).

## Tests

- `VmSpecials1463SliceTests.CeilingCopiesDisplacementAndFloorCarryOnly`
- `VmSpecials1463SliceTests.ModelOffsetsCardinalAndBoth`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1463 --to 1463` (0 hard / 0 soft).

## Seams

MapLoader::SpawnScrollers from its opening line through the closing brace. The banner at 1193-1198 stays uncredited. CreateScroller starts at 1437.

## Credit rationale

UNCredited specials.cpp 1199-1434 (+236). One contiguous function. The span does not cross credited or recognition lines.
