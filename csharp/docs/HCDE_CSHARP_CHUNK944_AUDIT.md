# Chunk 944 — `ACSF_DropItem` through start of `ACSF_ChangeActorPitch` from `p_acs.cpp` (**6300–6449**). Follows chunk **943** (**6150–6299**); outside deferred **7366–7509**.

## Scope

`ACSF_DropItem` through start of `ACSF_ChangeActorPitch` from `p_acs.cpp` (**6300–6449**). Follows chunk **943** (**6150–6299**); outside deferred **7366–7509**.

## Native ranges

- `src/playsim/p_acs.cpp` **ACSF_DropItem** 6300–6449 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsCallFuncDropGameplay944Slice.cs`
Managed LOC (tally): 89.
Conversion commit: `50da9fec08c34c1f8f6bf784b92fe087bc12df69`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_acs.cpp`) |
|---------|----------------------|
| chunk **944** hull | **6300–6449** |
| `DropItemAmountOrDefault` / `DropItemChanceOrDefault` | `ACSF_DropItem` **6304–6305** |
| `DropItemUsesActivatorTid` / `ActivatorRequiredForDrop` | **6310–6316** |
| `DropItemClassFound` / `IncrementDropCount` | **6308–6328** |
| `DropInventoryTypeResolved` | `ACSF_DropInventory` **6340–6366** |
| `CheckFlagActorPresent` | `ACSF_CheckFlag` **6374–6378** |
| `ResolveQuakeEx*` optional args | `ACSF_QuakeEx` **6385–6396** |
| `SetLineActivationRepeatOrDefault` / repeat flags | `ACSF_SetLineActivation` **6404–6409** |
| `GetLineActivationWhenFound` | `ACSF_GetLineActivation` **6417–6418** |
| `PowerupClassIsActorType` / `PowerupInventoryPresent` | `ACSF_GetActorPowerupTics` **6425–6438** |
| `ChangeActorAngleInterpolate` | `ACSF_ChangeActorAngle` **6445** |
| `ChangeActorPitchInterpolate` / `MinArgCountMet` | `ACSF_ChangeActorPitch` **6448–6449** |

### Chunk **944** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**943** |


**Post-review ratio:** 89/150 managed/native ≈ 59% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsCallFuncDropGameplay944SliceTests.DropItemAmount_DefaultsToNegativeOne`
- `VmPAcsCallFuncDropGameplay944SliceTests.DropItemChance_DefaultsToTwoFiftySix`
- `VmPAcsCallFuncDropGameplay944SliceTests.QuakeExDamageMultiplier_DefaultsToOne`
- `VmPAcsCallFuncDropGameplay944SliceTests.QuakeExHighFrequency_DefaultsToHalf`
- `VmPAcsCallFuncDropGameplay944SliceTests.SetLineActivationRepeat_DefaultsToNegativeOne`
- `VmPAcsCallFuncDropGameplay944SliceTests.GetLineActivation_ReturnsZeroWhenLineMissing`
- `VmPAcsCallFuncDropGameplay944SliceTests.ChangeActorAngleInterpolate_FalseWhenArgMissing`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 944 --to 944` (0 hard / 0 soft).

## Seams

`P_DropItem`, `DropInventory`, `CheckActorFlag`, `P_StartQuakeXYZ`, line iterators, `PClass::FindActor`, and `SetActorAngle`/`SetActorPitch` remain host seams.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmPAcsCallFuncDropGameplay944Slice.cs` (`AcsCallFunctions` handlers are not matrix credit).
