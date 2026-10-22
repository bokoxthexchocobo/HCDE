# Chunk 740 — Seeker/tracer steering plus CheckIfSeen (`p_actionfunctions.cpp` 742–892, 1854–1878).

## Scope

Seeker/tracer steering plus CheckIfSeen (`p_actionfunctions.cpp` 742–892, 1854–1878).

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_SeekerMissile)** 742–781 (40 lines)
- `src/playsim/p_actionfunctions.cpp` **DoTracer2** 791–837 (47 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_Tracer2)** 839–845 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_Tracer)** 854–887 (34 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, CheckIfSeen)** 1854–1878 (25 lines)

**Credited native LOC:** 153 (tally `nativeLoc` 153).

## Managed

`VmPActionSeekerTracer.cs`
Managed LOC (tally): 166.
Conversion commit: `b23b14cc4aa89988ad59ab88b615c671f0257cb6`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `SeekerMissile` | `DEFINE_ACTION_FUNCTION(AActor, A_SeekerMissile)` 742–781 |
| `DoTracer2` | static `DoTracer2` 791–837 |
| (native macro only) | `DEFINE_ACTION_FUNCTION(AActor, A_Tracer2)` 839–845 |
| `TracerAction` | `DEFINE_ACTION_FUNCTION(AActor, A_Tracer)` 854–887 |
| `CheckIfSeen` | `DEFINE_ACTION_FUNCTION(AActor, CheckIfSeen)` 1854–1878 |


**Post-review ratio:** 166/153 managed/native ≈ 108% (src file LOC at HEAD; see tally).

## Tests

- `VmPActionSeekerTracerTests.SeekerMissile_StickyPlayerTracerWhenNoLook`
- `VmPActionSeekerTracerTests.SeekerMissile_LookClearsTracerOnMiss`
- `VmPActionSeekerTracerTests.DoTracer2_TurnsTowardTracer`
- `VmPActionSeekerTracerTests.DoTracer2_SkipsDeadTracer`
- `VmPActionSeekerTracerTests.TracerAction_SkipsOffTic`
- `VmPActionSeekerTracerTests.CheckIfSeen_ReturnsFalseWhenAnyPlayerSees`
- `VmPActionSeekerTracerTests.CheckIfSeen_ReturnsTrueWhenUnseen`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 740 --to 740` (0 hard / 0 soft).

## Seams

`P_RoughMonsterSearch`, `P_SeekerMissile`, puff/smoke spawns, `P_CheckSight`, and player iteration are injected.

## Credit rationale

Base seeker/tracer band was under 150 native; `A_Tracer` and `CheckIfSeen` are whole-function padding without floor stuffing.
