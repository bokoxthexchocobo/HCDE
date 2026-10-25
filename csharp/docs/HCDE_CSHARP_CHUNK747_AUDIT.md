# Chunk 747 — Fake-floor triggers, player land hooks, CallFallAndSink, TID hash (`p_mobj.cpp`).

## Scope

Fake-floor triggers, player land hooks, CallFallAndSink, TID hash (`p_mobj.cpp`).

## Native ranges

- `src/playsim/p_mobj.cpp` **P_CheckFakeFloorTriggers** 3214–3275 (62 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, CheckFakeFloorTriggers)** 3277–3284 (8 lines)
- `src/playsim/p_mobj.cpp` **PlayerLandedMakeRumble** 3292–3299 (8 lines)
- `src/playsim/p_mobj.cpp` **PlayerDiedMakeRumble** 3301–3308 (8 lines)
- `src/playsim/p_mobj.cpp` **PlayerLandedMakeGruntSound** 3316–3323 (8 lines)
- `src/playsim/p_mobj.cpp` **PlayerSquatView** 3325–3332 (8 lines)
- `src/playsim/p_mobj.cpp` **PlayerLandedOnThing** 3334–3347 (14 lines)
- `src/playsim/p_mobj.cpp` **AActor::CallFallAndSink** 3443–3454 (12 lines)
- `src/playsim/p_mobj.cpp` **AActor::AddToHash** 3559–3582 (24 lines)
- `src/playsim/p_mobj.cpp` **AActor::RemoveFromHash** 3589–3602 (14 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetTID** 3604–3618 (15 lines)

**Credited native LOC:** 181 (tally `nativeLoc` 181).

## Managed

`VmPMobjFakeFloorLand.cs`
Managed LOC (tally): 130.
Conversion commit: `f47aacf5ab7745f02ca088313401b880aab6fe39`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `CheckFakeFloorTriggers` | `P_CheckFakeFloorTriggers` 3214–3275 |
| `CheckFakeFloorTriggers` | `DEFINE_ACTION_FUNCTION(AActor, CheckFakeFloorTriggers)` 3277–3284 |
| `PlayerLandedOnThing` | `PlayerLandedMakeRumble` 3292–3299 |
| `PlayerLandedOnThing` | `PlayerDiedMakeRumble` 3301–3308 |
| `PlayerLandedOnThing` | `PlayerLandedMakeGruntSound` 3316–3323 |
| `PlayerLandedOnThing` | `PlayerSquatView` 3325–3332 |
| `PlayerLandedOnThing` | `PlayerLandedOnThing` 3334–3347 |
| `CallFallAndSink` | `AActor::CallFallAndSink` 3443–3454 |
| `AddToHash` | `AActor::AddToHash` 3559–3582 |
| `RemoveFromHash` | `AActor::RemoveFromHash` 3589–3602 |
| `SetTid` | `AActor::SetTID` 3604–3618 |

### Out-of-body uncredit (747)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **181** |
| **Out-of-body removed** | **0** |
| **Honest** | **181** |


**Post-review ratio:** 130/181 managed/native ≈ 72% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjFakeFloorLandTests.CheckFakeFloorTriggers_SkipsWhenPredicting`
- `VmPMobjFakeFloorLandTests.CheckFakeFloorTriggers_FeetHitFakeFloor`
- `VmPMobjFakeFloorLandTests.CheckFakeFloorTriggers_EyesCrossSurface`
- `VmPMobjFakeFloorLandTests.CheckFakeFloorTriggers_EyesDiveBelowSurface`
- `VmPMobjFakeFloorLandTests.CallFallAndSink_NativeFallback`
- `VmPMobjFakeFloorLandTests.AddToHash_ZeroTidClearsLinks`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 747 --to 747` (0 hard / 0 soft).

## Seams

`TriggerSectorActions`, rumble/grunt virtuals, `P_FallingDamage`, `VMCall`, and `TIDHash` are injected.

## Credit rationale

Whole p_mobj helpers; excludes FallAndSink body (chunk735).
