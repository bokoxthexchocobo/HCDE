# Chunk 809 — Whole `FTagManager` mutation/query APIs and `FSectorTagIterator` / `FLineIdIterator` from `p_tags.cpp` (36–393 except `DumpTags`, 404–464).

## Scope

Whole `FTagManager` mutation/query APIs and `FSectorTagIterator` / `FLineIdIterator` from `p_tags.cpp` (36–393 except `DumpTags`, 404–464).

## Native ranges

- `src/playsim/p_tags.cpp` **FTagManager::AddSectorTag** 36–63 (28 lines)
- `src/playsim/p_tags.cpp` **FTagManager::RemoveSectorTags** 71–85 (15 lines)
- `src/playsim/p_tags.cpp` **FTagManager::RemoveLineIDs** 93–107 (15 lines)
- `src/playsim/p_tags.cpp` **FTagManager::AddLineID** 115–141 (27 lines)
- `src/playsim/p_tags.cpp` **FTagManager::HashTags** 149–181 (33 lines)
- `src/playsim/p_tags.cpp` **FTagManager sector/line queries** 189–193 (5 lines)
- `src/playsim/p_tags.cpp` **FTagManager sector/line queries** 201–205 (5 lines)
- `src/playsim/p_tags.cpp` **FTagManager sector/line queries** 207–210 (4 lines)
- `src/playsim/p_tags.cpp` **FTagManager sector/line queries** 219–231 (13 lines)
- `src/playsim/p_tags.cpp` **FTagManager sector/line queries** 239–242 (4 lines)
- `src/playsim/p_tags.cpp` **FTagManager sector/line queries** 250–254 (5 lines)
- `src/playsim/p_tags.cpp` **FTagManager sector/line queries** 262–274 (13 lines)
- `src/playsim/p_tags.cpp` **FTagManager sector/line queries** 282–285 (4 lines)
- `src/playsim/p_tags.cpp` **FTagManager::CountSectorTags** 311–332 (22 lines)
- `src/playsim/p_tags.cpp` **FTagManager::CountSectorTags** 334–348 (15 lines)
- `src/playsim/p_tags.cpp` **FTagManager::CountLineIDs** 356–377 (22 lines)
- `src/playsim/p_tags.cpp` **FTagManager::CountLineIDs** 379–393 (15 lines)
- `src/playsim/p_tags.cpp` **FSectorTagIterator::Next** 404–431 (28 lines)
- `src/playsim/p_tags.cpp` **FSectorTagIterator::NextCompat** 439–448 (10 lines)
- `src/playsim/p_tags.cpp` **FLineIdIterator::Next** 457–464 (8 lines)

**Credited native LOC:** 291 (tally `nativeLoc` 291).

## Portfolio delta

**Gross CLI band:** **429** lines in the UNCredited-range probe; tally credits **291** whole-function native LOC only.
**138** uncovered lines in that band are inter-function gaps, comments, or non-function lines (not credited).

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **429** |
| **Credited** (whole functions, this row) | **291** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+291** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

`VmPTagManager.cs`
Managed LOC (tally): 298.
Conversion commit: `a14c17716dba1ce2d68ebbfad16ae35e43808f46`.

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

### Out-of-body uncredit (809)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **337** |
| **Out-of-body removed** | **46** |
| **Honest** | **291** |


**Post-review ratio:** 298/291 managed/native ≈ 102% (src file LOC at HEAD; see tally).

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
