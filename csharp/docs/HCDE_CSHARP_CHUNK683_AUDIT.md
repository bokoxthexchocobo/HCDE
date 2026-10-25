# Chunk 683 — Whole `FMultiBlockLinesIterator` and `FBlockThingsIterator` (`p_maputl.cpp` 693–856, 864–1026).

## Scope

Whole `FMultiBlockLinesIterator` and `FBlockThingsIterator` (`p_maputl.cpp` 693–856, 864–1026).

## Native ranges

- `src/playsim/p_maputl.cpp` **FMultiBlockLinesIterator::FMultiBlockLinesIterator** 693–702 (10 lines)
- `src/playsim/p_maputl.cpp` **FMultiBlockLinesIterator::FMultiBlockLinesIterator** 704–714 (11 lines)
- `src/playsim/p_maputl.cpp` **FMultiBlockLinesIterator::GoUp** 722–737 (16 lines)
- `src/playsim/p_maputl.cpp` **FMultiBlockLinesIterator::GoDown** 745–760 (16 lines)
- `src/playsim/p_maputl.cpp` **FMultiBlockLinesIterator::Next** 768–821 (54 lines)
- `src/playsim/p_maputl.cpp` **FMultiBlockLinesIterator::startIteratorForGroup** 829–842 (14 lines)
- `src/playsim/p_maputl.cpp` **FMultiBlockLinesIterator::Reset** 850–856 (7 lines)
- `src/playsim/p_maputl.cpp` **FBlockThingsIterator::FBlockThingsIterator** 864–872 (9 lines)
- `src/playsim/p_maputl.cpp` **FBlockThingsIterator::FBlockThingsIterator** 874–884 (11 lines)
- `src/playsim/p_maputl.cpp` **FBlockThingsIterator::FBlockThingsIterator** 886–894 (9 lines)
- `src/playsim/p_maputl.cpp` **FBlockThingsIterator::ClearHash** 902–907 (6 lines)
- `src/playsim/p_maputl.cpp` **FBlockThingsIterator::StartBlock** 915–928 (14 lines)
- `src/playsim/p_maputl.cpp` **FBlockThingsIterator::SwitchBlock** 936–941 (6 lines)
- `src/playsim/p_maputl.cpp` **FBlockThingsIterator::Next** 949–1026 (78 lines)

**Credited native LOC:** 261 (tally `nativeLoc` 261).

## Managed

`VmPMapUtlMultiBlockLinesIterator.cs`, `VmPMapUtlBlockThingsIterator.cs`
Managed LOC (tally): 329.
Conversion commit: `815141a02c6c6adc7ce875826200571dd5f7447a`.

## Padding review (2026-10-21)

| Managed | Native (`p_maputl.cpp`) |
|---------|-------------------------|
| `VmPMapUtlMultiBlockLinesIterator` | `FMultiBlockLinesIterator` 693–856 |
| `VmPMapUtlBlockThingsIterator` ctors/`init`/`ClearHash`/`StartBlock`/`SwitchBlock`/`Next` | `FBlockThingsIterator` 864–1026 |

### Out-of-body uncredit (683)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **264** |
| **Out-of-body removed** | **3** |
| **Honest** | **261** |


**Post-review ratio:** 329/261 managed/native ≈ 126% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapUtlMultiBlockLinesIteratorTests.Next_ReturnsLineFromBlockIterator`
- `VmPMapUtlMultiBlockLinesIteratorTests.GoUp_StartsUpperPortalGroup`
- `VmPMapUtlMultiBlockLinesIteratorTests.GoDown_StartsLowerPortalGroup`
- `VmPMapUtlMultiBlockLinesIteratorTests.Next_AdvancesPortalGroupWhenBlockIteratorExhausted`
- `VmPMapUtlMultiBlockLinesIteratorTests.Reset_ReinitializesPortalWalk`
- `VmPMapUtlBlockThingsIteratorTests.Next_ReturnsSingleBlockActorWithoutHashing`
- `VmPMapUtlBlockThingsIteratorTests.Next_SkipsDuplicateActorUsingHash`
- `VmPMapUtlBlockThingsIteratorTests.Next_CenterOnlyFiltersToBlockInterior`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 683 --to 683` (0 hard / 0 soft).

## Seams

`CollectConnectedGroups`, `FBlockLinesIterator`, blockmap storage, `PrefetchL3`, and `FBlockNode` release remain native.

## Credit rationale

Portal-group and single-group line/thing block iterators after chunk 680.
