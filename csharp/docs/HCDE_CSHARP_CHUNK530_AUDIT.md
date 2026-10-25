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
Managed LOC (tally): 101.
Conversion commit: `9b3429f7a6ba0e4bf351c45d9a9cbea839f89f94`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-21):** Credits count-flag/raise/orientation bands **2767–3028** plus `SetViewAngleNative`/`SetViewPitchNative` **3038–3086** (`p_actionfunctions.cpp`).

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `CheckFlagResult` | `CheckFlag` **2789–2790** |
| `PlanCountFlags` / `ApplyKillCount` / `CountsAsKill` | `A_ChangeCountFlags` **2801–2824** |
| `ChildUsesMaster` | `A_RaiseChildren` **2860** |
| `SiblingSharesMaster` | `A_RaiseSiblings` **2885–2887** |
| `RefireProbabilityFails` / `RefireTargetInvalid` / `RefireContinuesFiring` | `A_MonsterRefire` **2946–2956** |
| `SetAngleOnRef` / `SetPitchOnRef` / `SetRollOnRef` | **2975–3025** |
| `SetViewAngleOnRef` / `SetViewPitchOnRef` | **3038–3086** |
| `RefNull` | pointer null guards **2975**, **2998**, **3021** |

### Out-of-body uncredit (530)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **199** |
| **Out-of-body removed** | **2** |
| **Honest** | **197** |


**Post-review ratio:** 101/197 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmActionRaiseAndCountFlagsTests.CheckFlagResult_RequiresOwner`
- `VmActionRaiseAndCountFlagsTests.ApplyKillCount_DecrementsWhenCountedAlive`
- `VmActionRaiseAndCountFlagsTests.SiblingSharesMaster_ExcludesSelf`
- `VmActionRaiseAndCountFlagsTests.RefireProbabilityFails_WhenRollLow`
- `VmActionRaiseAndCountFlagsTests.RefireTargetInvalid_WhenNoSight`
- `VmActionRaiseAndCountFlagsTests.SetViewAngleOnRef_WhenPresent`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 530 --to 530` (0 hard / 0 soft).

## Seams

`ModActorFlag`, `CheckActorFlag`, `P_Thing_Raise`, `P_CheckSight`, and `SetAngle`/`SetPitch`/`SetRoll` are not hosted.

## Credit rationale

Credits count-flag level totals, raise pointer iteration policy, monster refire gating, and orientation setter null-guards from `p_actionfunctions.cpp` (2767–3028).
