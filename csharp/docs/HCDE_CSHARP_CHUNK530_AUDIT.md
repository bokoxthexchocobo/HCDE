# Chunk 530 — A_ChangeFlag, CheckFlag, A_ChangeCountFlags, raise family, A_SetAngle/Pitch/Roll/ViewAngle/ViewPitch

## Scope

A_ChangeFlag, CheckFlag, A_ChangeCountFlags, raise family, A_SetAngle/Pitch/Roll/ViewAngle/ViewPitch

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **AActor::A_ChangeFlag** 2767–2775 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::CheckFlag** 2783–2791 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_ChangeCountFlags** 2794–2826 (33 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_RaiseMaster** 2833–2843 (11 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_RaiseChildren** 2850–2866 (17 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_RaiseSiblings** 2873–2892 (20 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_RaiseSelf** 2899–2904 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::RaiseActor** 2912–2918 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::CanRaise** 2925–2929 (5 lines)
- `src/playsim/p_actionfunctions.cpp` **SetViewAngleNative** 3038–3045 (8 lines)
- `src/playsim/p_actionfunctions.cpp` **SetViewAngleNative** 3047–3057 (11 lines)
- `src/playsim/p_actionfunctions.cpp` **SetViewPitchNative** 3067–3074 (8 lines)
- `src/playsim/p_actionfunctions.cpp` **SetViewPitchNative** 3076–3086 (11 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_SetAngle** 2968–2981 (14 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_SetPitch** 2991–3004 (14 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::A_SetRoll** 3015–3028 (14 lines)

**Credited native LOC:** 197 (tally `nativeLoc` 197).

## Managed

`VmActionRaiseAndCountFlags.cs`
Managed LOC (tally): 210.
Conversion commit: `56c32560db2b44133c90e71500088ab1495cc302`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-21):** Credits count-flag/raise/orientation bands **2767–3028** plus `SetViewAngleNative`/`SetViewPitchNative` **3038–3086** (`p_actionfunctions.cpp`).

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `RunChangeCountFlags` / `PlanChangeCountFlags` / `ApplyKillCount` / `CountsAsKill` | `A_ChangeCountFlags` **2794–2825** |
| `TryRaiseMaster` / `RaiseChildrenOnLevel` / `RaiseSiblingsOnLevel` / `RaiseSelf` | `A_RaiseMaster` **2833–2843**; `A_RaiseChildren` **2850–2865**; `A_RaiseSiblings` **2873–2891**; `A_RaiseSelf` **2899–2904** |
| `DecideMonsterRefire` / refire helpers | `A_MonsterRefire` **2938–2957** |
| `TrySetAngle` / `TrySetPitch` / `TrySetRoll` / `TrySetViewAngle` / `TrySetViewPitch` | `A_SetAngle` **2968–2981**; `A_SetPitch` **2991–3004**; `A_SetRoll` **3015–3028**; `SetViewAngleNative` **3038–3086** |
| `CheckFlagResult` | `CheckFlag` **2789–2790** |
| `RefNull` | pointer null guards **2838**, **2881**, **2975**, **2998**, **3021**, **3041**, **3070** |

### Out-of-body uncredit (530)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **199** |
| **Out-of-body removed** | **2** |
| **Honest** | **197** |


**Post-review ratio:** 210/197 managed/native ≈ 107% (src file LOC at HEAD; see tally).

## Tests

- `VmActionRaiseAndCountFlagsTests.CheckFlagResult_RequiresOwner`
- `VmActionRaiseAndCountFlagsTests.ApplyKillCount_DecrementsWhenCountedAlive`
- `VmActionRaiseAndCountFlagsTests.RunChangeCountFlags_UpdatesLevelTotals`
- `VmActionRaiseAndCountFlagsTests.RaiseChildrenOnLevel_RaisesMatchingMaster`
- `VmActionRaiseAndCountFlagsTests.DecideMonsterRefire_JumpsWhenSightBlocked`
- `VmActionRaiseAndCountFlagsTests.SiblingSharesMaster_ExcludesSelf`
- `VmActionRaiseAndCountFlagsTests.RefireProbabilityFails_WhenRollLow`
- `VmActionRaiseAndCountFlagsTests.RefireTargetInvalid_WhenNoSight`
- `VmActionRaiseAndCountFlagsTests.TrySetViewAngle_InvokesSetterWhenRefPresent`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 530 --to 530` (0 hard / 0 soft).

## Seams

`ModActorFlag`, `CheckActorFlag`, `P_Thing_Raise`, `P_CheckSight`, and `SetAngle`/`SetPitch`/`SetRoll` are not hosted.

## Credit rationale

Credits count-flag level totals, raise pointer iteration policy, monster refire gating, and orientation setter null-guards from `p_actionfunctions.cpp` (2767–3028).
