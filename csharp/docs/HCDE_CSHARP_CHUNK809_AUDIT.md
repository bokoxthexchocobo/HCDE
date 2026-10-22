# Chunk 809 — Whole `FTagManager` mutation/query APIs and `FSectorTagIterator` / `FLineIdIterator` from `p_tags.cpp` (36–393 except `DumpTags`, 404–464).

## Scope

Whole `FTagManager` mutation/query APIs and `FSectorTagIterator` / `FLineIdIterator` from `p_tags.cpp` (36–393 except `DumpTags`, 404–464).

## Native ranges

- `src/playsim/p_tags.cpp` **FTagManager::AddSectorTag** 36–63 (28 lines)
- `src/playsim/p_tags.cpp` **FTagManager::RemoveSectorTags** 71–85 (15 lines)
- `src/playsim/p_tags.cpp` **FTagManager::RemoveLineIDs** 93–107 (15 lines)
- `src/playsim/p_tags.cpp` **FTagManager::AddLineID** 115–141 (27 lines)
- `src/playsim/p_tags.cpp` **FTagManager::HashTags** 149–181 (33 lines)
- `src/playsim/p_tags.cpp` **FTagManager sector/line queries** 189–285 (97 lines)
- `src/playsim/p_tags.cpp` **FTagManager::CountSectorTags** 311–348 (38 lines)
- `src/playsim/p_tags.cpp` **FTagManager::CountLineIDs** 356–393 (38 lines)
- `src/playsim/p_tags.cpp` **FSectorTagIterator::Next** 404–431 (28 lines)
- `src/playsim/p_tags.cpp` **FSectorTagIterator::NextCompat** 439–448 (10 lines)
- `src/playsim/p_tags.cpp` **FLineIdIterator::Next** 457–464 (8 lines)

**Credited native LOC:** 337 (tally `nativeLoc` 337).

## Managed

`VmPTagManager.cs`
Managed LOC (tally): 298.
Conversion commit: `2bebd7cfbf4581f39c88cf127db780b255e35512`.

## Padding review (2026-10-21)

| Managed | Native (`p_tags.cpp`) |
|---------|----------------------|
| `AddSectorTag` | **36–63** |
| `RemoveSectorTags` | **71–85** |
| `RemoveLineIds` | **93–107** |
| `AddLineId` | **115–141** |
| `HashTags` | **149–181** |
| `SectorHasTags` / queries | **189–285** |
| `CountSectorTags` / `GetSectorTag` | **311–348** |
| `CountLineIds` / `GetLineId` | **356–393** |
| `SectorTagIterator::Next` | **404–431** |
| `NextCompat` | **439–448** |
| `LineIdIterator::Next` | **457–464** |

**Post-review ratio:** 298/337 managed/native ≈ 88% (src file LOC at HEAD; see tally).

## Tests

- `VmPTagManagerTests.SectorHasTag_DetectsDuplicateTags`
- `VmPTagManagerTests.LineIdIterator_WalksHashChain`
- `VmPTagManagerTests.SectorTagIterator_FindsTaggedSectors`
- `VmPTagManagerTests.LineHasId_FindsMatchingId`
- `VmPTagManagerTests.GetSectorTag_ByIndex`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 809 --to 809` (0 hard / 0 soft).

## Seams

Sector/line `Index()` accessors and `Printf` diagnostics are injected; `DumpTags` left uncredited.

## Credit rationale

First tally credit for `p_tags.cpp`; consolidates duplicate `LineIdIterator` helper off `VmPortalBuildBlockmap`.
