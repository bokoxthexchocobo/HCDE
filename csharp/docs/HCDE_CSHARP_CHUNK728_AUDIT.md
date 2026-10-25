# Chunk 728 — Whole `AActor::Tick` (`p_mobj.cpp` 4523–5075).

## Scope

Whole `AActor::Tick` (`p_mobj.cpp` 4523–5075).

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::Tick** 4523–5075 (553 lines)

**Credited native LOC:** 553 (tally `nativeLoc` 553).

## Managed

`VmPMobjActorTick.cs`
Managed LOC (tally): 343.
Conversion commit: `86f8ce3a9aa4eae3299f549536de0d8c47780daf`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `Tick` | `AActor::Tick` 4523–5075 |
| `TickNoInteraction` | MF5_NOINTERACTION 4587–4637 |
| `HandleRocketGrenadeTrail` | smoke trails 4680–4711 |
| `ApplyVerticalFriction` / `PulseVisibility` | 4719–4778 |
| `AverageSectorScroll` | scroll average 4913–4922 |
| `HandleVerticalMotion` / `HandleOnMobj` | Z/onmobj 5006–5075 |
| `MissileNeedsMinVelocity` | 4970–4972 |

### Out-of-body uncredit (728)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **553** |
| **Out-of-body removed** | **0** |
| **Honest** | **553** |


**Post-review ratio:** 343/553 managed/native ≈ 62% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjActorTickTests.Tick_NullState_MarksDestroyed`
- `VmPMobjActorTickTests.Tick_Freezetics_DecrementsAndReturns`
- `VmPMobjActorTickTests.Tick_NoInteraction_ClearsScrollFlag`
- `VmPMobjActorTickTests.AverageSectorScroll_DividesWhenMultipleSectors`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 728 --to 728` (0 hard / 0 soft).

## Seams

`TickBehaviors`, inventory `DoEffect`, scroll carry, `P_XYMovement`, `P_CheckOnmobj`, `P_ZMovement`, `P_BounceActor`, `P_ExplodeMissile`, and sector actions are injected.

## Credit rationale

Single-function oversize exception (~553 native LOC): thinker tick through XY/Z movement.
