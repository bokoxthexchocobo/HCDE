# Chunk 799 — `FLevelLocals::CreateFloor` (`mapthinkers/a_floor.cpp` 281–514).

## Scope

`FLevelLocals::CreateFloor` (`mapthinkers/a_floor.cpp` 281–514).

## Native ranges

- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::CreateFloor** 281–514 (234 lines)

**Credited native LOC:** 234 (tally `nativeLoc` 234).

## Managed

`VmAFloorCreate.cs`
Managed LOC (tally): 281.
Conversion commit: `298b5f92f1a6ad00679cd167eef9f8d3158b0bae`.

## Padding review (2026-10-21)

| Managed | Native (`a_floor.cpp`) |
|---------|----------------------|
| `CreateFloor` | `FLevelLocals::CreateFloor` **281–514** |


**Post-review ratio:** 281/234 managed/native ≈ 120% (src file LOC at HEAD; see tally).

## Tests

- `VmAFloorCreateTests.CreateFloor_RaiseByValue_SetsUpDirection`
- `VmAFloorCreateTests.CreateFloor_LowerByValue_SetsDownDirection`
- `VmAFloorCreateTests.CreateFloor_SkipsWhenPlaneMoving`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 799 --to 799` (0 hard / 0 soft).

## Seams

Height finders, model sectors, instant-floor compat, thinker spawn, and `SetFloorChangeType` are injected.

## Credit rationale

Whole-function floor spawn switch after DFloor tick band (chunk798).
