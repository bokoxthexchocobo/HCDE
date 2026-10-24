# Chunk 378 — drawThings visibility, sprite rotation index, and thing color classification

## Scope

drawThings visibility, sprite rotation index, and thing color classification

## Native ranges

- `src/am_map.cpp` **DAutomap::drawKeys** 2991–3033 (43 lines)
- `src/am_map.cpp` **DAutomap::drawThings** 3040–3170 (131 lines)

**Credited native LOC:** 174 (tally `nativeLoc` 174).

## Managed

`VmAutomapDrawThings.cs`, `VmAutomapDrawThingsTests.cs`
Managed LOC (tally): 97.
Conversion commit: `ccd284bdcb32bcc009e9656885beef19286c07a1`.

## Tests

- `VmAutomapDrawThingsTests.ShouldDrawThingOnMap_RespectsCheatAndFlags`
- `VmAutomapDrawThingsTests.ClassifyThingTriangleColor_MonsterFriendly`
- `VmAutomapDrawThingsTests.ComputeSpriteRotationIndex_ClampedToZeroSixteen`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 378 --to 378` (0 hard / 0 soft).

## Seams

`DrawMarker`, sector thing iterators, and texture lookup are not hosted.

## Credit rationale

Credits `DAutomap::drawKeys` and `DAutomap::drawThings` from `am_map.cpp`.
