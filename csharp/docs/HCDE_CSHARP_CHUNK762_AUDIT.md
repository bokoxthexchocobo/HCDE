# Chunk 762 — `FThinkerCollection::Link`, `RunThinkers`, `DestroyAllThinkers`, `CleanUpTravellers`, `OnLoad` (`dthinker.cpp` 71–366; excludes `SerializeThinkers` 374–443). Retired (2026-10-24): `--band` hull overlaps existing credit (dthinker-list-client-chunk763 src/playsim/dthinker.cpp 210-223).

## Scope

`FThinkerCollection::Link`, `RunThinkers`, `DestroyAllThinkers`, `CleanUpTravellers`, `OnLoad` (`dthinker.cpp` 71–366; excludes `SerializeThinkers` 374–443). Retired (2026-10-24): `--band` hull overlaps existing credit (dthinker-list-client-chunk763 src/playsim/dthinker.cpp 210-223).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `f60b85d27b939486d1af8511e88ed5e60d4f49ed`.

## Padding review (2026-10-21)

| Managed | Native (`dthinker.cpp`) |
|---------|-------------------------|
| `Link` | `FThinkerCollection::Link` **71–84** |
| `RunThinkers` / `RunThinkersNonProfile` / `RunThinkersProfile` / `CompareProfileEntries` | `FThinkerCollection::RunThinkers` **92–200** |
| `DestroyAllThinkers` | `FThinkerCollection::DestroyAllThinkers` **315–336** |
| `CleanUpTravellers` | `FThinkerCollection::CleanUpTravellers` **344–352** |
| `OnLoad` | `FThinkerCollection::OnLoad` **360–366** |

### Out-of-body uncredit (762)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **161** |
| **Out-of-body removed** | **0** |
| **Honest** | **161** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 762 --to 762` (0 hard / 0 soft).

## Seams

`FThinkerList::TickThinkers`, `ProfileThinkers`, `DoDestroyThinkers`, `RemoveTravellers`, list `OnLoad`, GC, and VM stack cleanup are injected.

## Credit rationale

Faithful orchestration for thinker linking, tick passes, teardown, traveller cleanup, and post-load hooks before save serialization.
