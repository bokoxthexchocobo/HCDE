# Chunk 728 — Whole `AActor::Tick` (`p_mobj.cpp` 4523–5075).

## Scope

Whole `AActor::Tick` (`p_mobj.cpp` 4523–5075).

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::Tick** 4523–5075 (553 lines)

**Credited native LOC:** 553 (tally `nativeLoc` 553).

## Managed

`VmPMobjActorTick.cs`
Managed LOC (tally): 614.
Conversion commit: `3cfb58993ddf81aa690f5d953133b36e01659141`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `Tick` | `AActor::Tick` 4523–5075 |
| `TickNoInteraction` | MF5_NOINTERACTION 4587–4637 |
| `AccumulateSectorCarry` / `AddPlayerScrollSpecial` | scroll carry 4787–4924 |
| `ApplySteepSlopePush` | steep floor push 4926–4963 |
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


**Post-review ratio:** 614/553 managed/native ≈ 111% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjActorTickTests.Tick_NullState_MarksDestroyed`
- `VmPMobjActorTickTests.Tick_Freezetics_DecrementsAndReturns`
- `VmPMobjActorTickTests.Tick_NoInteraction_ClearsScrollFlag`
- `VmPMobjActorTickTests.AverageSectorScroll_DividesWhenMultipleSectors`
- `VmPMobjActorTickTests.AccumulateSectorCarry_AveragesTwoScrollContributions`
- `VmPMobjActorTickTests.SteepSlopePush_AddsPlaneNormalToVelocity`
- `VmPMobjActorTickTests.OnMobj_StepUp_SetsActorZToBlockerTop`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 728 --to 728` (0 hard / 0 soft).

## Seams

`TickBehaviors`, inventory `DoEffect`, scroll carry, `P_XYMovement`, `P_CheckOnmobj`, `P_ZMovement`, `P_BounceActor`, `P_ExplodeMissile`, and sector actions are injected.

## Credit rationale

Single-function oversize exception (~553 native LOC): thinker tick through XY/Z movement.
