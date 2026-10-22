# Chunk 688 — p_trace portal entry + `Setup3DFloors`

## Native credit (237 LOC — not floor-fitted)

Whole functions only, chosen before measuring:

| Symbol | Lines | LOC |
|--------|-------|-----|
| `GetPortalTransition` | 99–126 | 28 |
| `isLiquid` | 128–133 | 6 (used by setup; not added to hit 150) |
| `Trace` | 141–197 | 57 |
| `EnterSectorPortal` | 206–231 | 26 |
| `EnterLinePortal` | 239–271 | 33 |
| `Setup3DFloors` | 279–365 | 87 |

**Sum: 237** (above 150 floor without trimming).

## Pre-450 `combat-trace-ordering`

Intercept band narrowed to **366–479** so **279–365** is not double-counted with this chunk. Combat row still only credits `CombatTrace.cs` (no portal managed files).

## Managed

- `VmPTracePortalEntry.cs`
- `VmPTraceSetup3DFloors.cs`

## Verified

`dotnet test csharp/HCDE.sln`; `reaudit_chunks.py audit --from 688 --to 688` (0/0).
