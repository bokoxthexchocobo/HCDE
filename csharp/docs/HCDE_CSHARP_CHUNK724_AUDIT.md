# Chunk 724 — Whole static `P_XYMovement` (`p_mobj.cpp` 2421–2926). Accounting: `csharp/docs/HCDE_CSHARP_CHUNK724_728_ACCOUNTING.md`.

## Scope

Whole static `P_XYMovement` (`p_mobj.cpp` 2421–2926). Accounting: `csharp/docs/HCDE_CSHARP_CHUNK724_728_ACCOUNTING.md`.

## Native ranges

- `src/playsim/p_mobj.cpp` **P_XYMovement** 2421–2926 (506 lines)

**Credited native LOC:** 506 (tally `nativeLoc` 506).

## Managed

`VmPMobjXYMovement.cs`
Managed LOC (tally): 557.
Conversion commit: `7e71de589cad8e0d479db7467adb5bfc7c6c98e7`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `XYMovement` (wind, carry scroll, step loop, slide, missile explode, friction) | `P_XYMovement` 2421–2926 |
| `SubstepsForMove` | step sizing 2535–2561 |
| `HandleSkullflyStop` | zero-move skullfly 2502–2520 |
| `HandleMissileBlocked` | missile blocked 2705–2766 |

### Out-of-body uncredit (724)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **506** |
| **Out-of-body removed** | **0** |
| **Honest** | **506** |


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
