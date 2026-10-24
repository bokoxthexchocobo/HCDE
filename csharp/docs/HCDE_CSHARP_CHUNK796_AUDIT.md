# Chunk 796 — `FLevelLocals::CreateCeiling` (`mapthinkers/a_ceiling.cpp` 250–479).

## Scope

`FLevelLocals::CreateCeiling` (`mapthinkers/a_ceiling.cpp` 250–479).

## Native ranges

- `src/playsim/mapthinkers/a_ceiling.cpp` **FLevelLocals::CreateCeiling** 250–479 (230 lines)

**Credited native LOC:** 230 (tally `nativeLoc` 230).

## Managed

`VmACeilingCreate.cs`
Managed LOC (tally): 318.
Conversion commit: `9c4eb555c8e1217da58b0b398e5cf454604460ae`.

## Padding review (2026-10-21)

| Managed | Native (`a_ceiling.cpp`) |
|---------|-------------------------|
| `CreateCeiling` | `FLevelLocals::CreateCeiling` **250–479** |


**Post-review ratio:** 318/230 managed/native ≈ 138% (src file LOC at HEAD; see tally).

## Tests

- `VmACeilingCreateTests.CreateCeiling_RaiseByValue_SetsUpDirection`
- `VmACeilingCreateTests.CreateCeiling_LowerByValue_SetsDownDirection`
- `VmACeilingCreateTests.CreateCeiling_SkipsWhenPlaneMoving`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 796 --to 796` (0 hard / 0 soft).

## Seams

Surrounding-height finders, model sectors, `CreateThinker`, `StopInterpolation`, and `PlayCeilingSound` are injected on `CreateCeilingContext`.

## Credit rationale

Ceiling mover setup before tick/EV chunk797.

## Manual review (2026-10-22)

- Single credited function **`CreateCeiling` (250–479)**; does **not** include `DCeiling::Construct` (**223–242**), `DEFINE_ACTION_FUNCTION` (**481–495**), or `IMPLEMENT_CLASS` (**39**).
- **`CreateCeiling`** body mirrors native type `switch`, texture-change block, instant-move `StopInterpolation`, and `PlayCeilingSound` tail.
- **`VmACeilingTickEv`** reuses `CeilingSector` / thinker fields — not a second implementation of create logic.
- Injected finders (`FindHighestFloorPoint`, model sector lookup, etc.) match native free-function calls; no stub `=> true` gates.
- No `Plan*` or bridge wrappers in `VmACeilingCreate.cs`.
