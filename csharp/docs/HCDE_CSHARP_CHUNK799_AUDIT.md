# Chunk 799 — `FLevelLocals::CreateFloor` (`mapthinkers/a_floor.cpp` 281–538).

## Scope

`FLevelLocals::CreateFloor` (`mapthinkers/a_floor.cpp` 281–538).

## Native ranges

- `src/playsim/mapthinkers/a_floor.cpp` **FLevelLocals::CreateFloor** 281–538 (258 lines)

**Credited native LOC:** 258 (tally `nativeLoc` 258).

## Managed

`VmAFloorCreate.cs`
Managed LOC (tally): 281.
Conversion commit: ``.

## Padding review (2026-10-21)

| Managed | Native (`a_floor.cpp`) |
|---------|----------------------|
| `CreateFloor` | `FLevelLocals::CreateFloor` **281–538** |


**Post-review ratio:** 281/258 managed/native ≈ 109% (src file LOC at HEAD; see tally).

## Tests

- `VmAFloorCreateTests.CreateFloor_RaiseByValue_SetsUpDirection`
- `VmAFloorCreateTests.CreateFloor_LowerByValue_SetsDownDirection`
- `VmAFloorCreateTests.CreateFloor_SkipsWhenPlaneMoving`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 799 --to 799` (0 hard / 0 soft).

## Seams

Surrounding-height finders, model sectors, instant-floor compat, and thinker spawn are injected on `CreateFloorContext`.

## Credit rationale

Floor mover setup paired with tick/elevator chunk798.
