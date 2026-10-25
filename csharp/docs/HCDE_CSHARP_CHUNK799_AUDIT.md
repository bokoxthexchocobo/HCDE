# Chunk 799 — `FLevelLocals::CreateFloor` (`mapthinkers/a_floor.cpp` 281–514). Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

`FLevelLocals::CreateFloor` (`mapthinkers/a_floor.cpp` 281–514). Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `0b1f92ef5d0aa82913b97a7933e1fa1355f17b80`.

## Padding review (2026-10-21)

| Managed | Native (`a_floor.cpp`) |
|---------|----------------------|
| `CreateFloor` | `FLevelLocals::CreateFloor` **281–514** |

### Out-of-body uncredit (799)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **234** |
| **Out-of-body removed** | **234** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 799 --to 799` (0 hard / 0 soft).

## Seams

Height finders, model sectors, instant-floor compat, thinker spawn, and `SetFloorChangeType` are injected.

## Credit rationale

Whole-function floor spawn switch after DFloor tick band (chunk798).
