# Chunk 724 — Whole static `P_XYMovement` (`p_mobj.cpp` 2421–2926).

## Scope

Whole static `P_XYMovement` (`p_mobj.cpp` 2421–2926).

## Native ranges

- `src/playsim/p_mobj.cpp` **P_XYMovement** 2421–2926 (506 lines)

**Credited native LOC:** 506 (tally `nativeLoc` 506).

## Managed

`VmPMobjXYMovement.cs`
Managed LOC (tally): 557.
Conversion commit: `54f8ace51b052b92fdfa5f7fd428076ad26317db`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `XYMovement` (wind, carry scroll, step loop, slide, missile explode, friction) | `P_XYMovement` 2421–2926 |
| `SubstepsForMove` | step sizing 2535–2561 |
| `HandleSkullflyStop` | zero-move skullfly 2502–2520 |
| `HandleMissileBlocked` | missile blocked 2705–2766 |


**Post-review ratio:** 557/506 managed/native ≈ 110% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjXYMovementTests.XYMovement_ZeroMove_ClearsSkullfly`
- `VmPMobjXYMovementTests.XYMovement_MissileBlocked_Explodes`
- `VmPMobjXYMovementTests.XYMovement_ScrollAboveCarryStop_AddsScrollMoveFlag`
- `VmPMobjXYMovementTests.SubstepsForMove_SplitsFastMotion`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 724 --to 724` (0 hard / 0 soft).

## Seams

`P_TryMove`, `P_SlideMove`, `P_CheckSlopeWalk`, `P_BounceActor`, `P_BounceWall`, `P_ReflectOffActor`, `P_ExplodeMissile`, `P_ProjectileHitPlane`, `P_GetFriction`, and `PlayIdle` are injected.

## Credit rationale

Single-function oversize exception (~506 native LOC): horizontal movement, slide, missile explode, friction.
