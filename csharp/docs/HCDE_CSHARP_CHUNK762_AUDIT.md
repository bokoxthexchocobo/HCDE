# Chunk 762 — `FThinkerCollection::Link`, `RunThinkers`, `DestroyAllThinkers`, `CleanUpTravellers`, `OnLoad` (`dthinker.cpp` 71–366; excludes `SerializeThinkers` 374–443).

## Scope

`FThinkerCollection::Link`, `RunThinkers`, `DestroyAllThinkers`, `CleanUpTravellers`, `OnLoad` (`dthinker.cpp` 71–366; excludes `SerializeThinkers` 374–443).

## Native ranges

- `src/playsim/dthinker.cpp` **FThinkerCollection::Link** 71–84 (14 lines)
- `src/playsim/dthinker.cpp` **FThinkerCollection::RunThinkers** 92–200 (109 lines)
- `src/playsim/dthinker.cpp` **FThinkerCollection::DestroyAllThinkers** 315–336 (22 lines)
- `src/playsim/dthinker.cpp` **FThinkerCollection::CleanUpTravellers** 344–352 (9 lines)
- `src/playsim/dthinker.cpp` **FThinkerCollection::OnLoad** 360–366 (7 lines)

**Credited native LOC:** 161 (tally `nativeLoc` 161).

## Managed

`VmDThinkerCollectionEarly.cs`
Managed LOC (tally): 215.
Conversion commit: `7c13df40da3bb7488e18c6d4293b5369c16eb440`.

## Padding review (2026-10-21)

| Managed | Native (`dthinker.cpp`) |
|---------|-------------------------|
| `Link` | `FThinkerCollection::Link` **71–84** |
| `RunThinkers` / `RunThinkersNonProfile` / `RunThinkersProfile` / `CompareProfileEntries` | `FThinkerCollection::RunThinkers` **92–200** |
| `DestroyAllThinkers` | `FThinkerCollection::DestroyAllThinkers` **315–336** |
| `CleanUpTravellers` | `FThinkerCollection::CleanUpTravellers` **344–352** |
| `OnLoad` | `FThinkerCollection::OnLoad` **360–366** |


**Post-review ratio:** 215/161 managed/native ≈ 134% (src file LOC at HEAD; see tally).

## Tests

- `VmDThinkerCollectionEarlyTests.Link_RoutesJustSpawnedToFreshList`
- `VmDThinkerCollectionEarlyTests.Link_ClearsJustSpawnedWhenNotTravelling`
- `VmDThinkerCollectionEarlyTests.RunThinkers_TicksFreshUntilStable`
- `VmDThinkerCollectionEarlyTests.CompareProfileEntries_SortsByTotalTimeDescending`
- `VmDThinkerCollectionEarlyTests.DestroyAllThinkers_SkipsTravellingAndStatic`
- `VmDThinkerCollectionEarlyTests.CleanUpTravellers_DestroyListAndStripFresh`
- `VmDThinkerCollectionEarlyTests.OnLoad_InvokesBothListKinds`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 762 --to 762` (0 hard / 0 soft).

## Seams

`FThinkerList::TickThinkers`, `ProfileThinkers`, `DoDestroyThinkers`, `RemoveTravellers`, list `OnLoad`, GC, and VM stack cleanup are injected.

## Credit rationale

Faithful orchestration for thinker linking, tick passes, teardown, traveller cleanup, and post-load hooks before save serialization.
