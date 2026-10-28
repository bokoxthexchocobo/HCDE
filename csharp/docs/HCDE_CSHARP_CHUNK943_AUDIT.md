# Chunk 943 — `ACSF_GetCVarString` through `ACSF_StopSound` from `p_acs.cpp` (**6000–6149**). Replaces aborted Thing_Hate **943**; outside deferred **7366–7509**.

## Scope

`ACSF_GetCVarString` through `ACSF_StopSound` from `p_acs.cpp` (**6000–6149**). Replaces aborted Thing_Hate **943**; outside deferred **7366–7509**.

## Native ranges

- `src/playsim/p_acs.cpp` **ACSF_GetCVarString** 6000–6149 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsCallFuncCvarHud943Slice.cs`
Managed LOC (tally): 76.
Conversion commit: `64ea0c7e0bfd63a95e221d867900a86290afb818`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_acs.cpp`) |
|---------|----------------------|
| chunk **943** hull | **6000–6149** |
| `MinArgCountMet` | `MIN_ARG_COUNT` gates **6000+** |
| `GetUserCVarAbortsWhenPlayerUnresolved` / `SelectGetUserCVarResult` | **6015–6017**, **6024–6026** |
| `SetCVarUsesStringOperand` | `SetCVar` vs `SetCVarString` **6005**, **6009** |
| `UserCVarNumericRead` / `UserCVarStringRead` | **6011–6027** |
| `SetUserCVarRequiresThreeArgs` | **6029–6035** |
| `MapLineAttackFlags` | `FHF_*` → `LAF_*` **6050–6052** |
| `LineAttackUsesActivator` | tid **6054–6056** |
| `ShouldAssignPuffTid` | puff TID **6057–6060**, **6070–6073** |
| `PlaySoundNeedsLookup` | `PlaySound` vs `PlayActorSound` **6086–6094** |
| `ResolvePlaySoundSpot` | tid **6105–6108** |
| `ComposePlayChannel` / `PlaySoundChannelMask` | channel flags **6119–6121** |
| `StopSoundUsesActivator` | **6133–6135** |
| `DefaultStopChannel` | **6131** |
| `HasValidSoundId` | **6116** |

### Chunk **943** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**942** |


**Post-review ratio:** 76/150 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsCallFuncCvarHud943SliceTests.MinArgCountMet_RequiresAtLeastN`
- `VmPAcsCallFuncCvarHud943SliceTests.MapLineAttackFlags_TranslatesFhfBits`
- `VmPAcsCallFuncCvarHud943SliceTests.LineAttackUsesActivator_WhenTidZero`
- `VmPAcsCallFuncCvarHud943SliceTests.ComposePlayChannel_AddsLocalAndLoopFlags`
- `VmPAcsCallFuncCvarHud943SliceTests.ResolvePlaySoundSpot_ReturnsActivatorForTidZero`
- `VmPAcsCallFuncCvarHud943SliceTests.DefaultStopChannel_FallsBackToBody`
- `VmPAcsCallFuncCvarHud943SliceTests.SetUserCVarRequiresThreeArgs_MatchesNative`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 943 --to 943` (0 hard / 0 soft).

## Seams

`DoGetCVar`, `SetCVar`, `SetUserCVar`, `P_LineAttack`, `S_PlaySound`, `S_StopSound`, and `HCDE_ResolveACSPlayer` remain host seams.

## Credit rationale

Honest **150**-line UNCredited band; exclusive `VmPAcsCallFuncCvarHud943Slice.cs` (`AcsModCvarService` / `AcsCallFunctionsExtended` are live helpers, not matrix credit for these lines).
