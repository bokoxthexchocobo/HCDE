# Chunk 780 — `player_t` log/subtitle/FOV/color/pain-flash/pitch helpers and VM wrappers (`p_user.cpp` 840–1146). Excludes `SetSkin` 971–986 and chunk777 band 1148+.

## Scope

`player_t` log/subtitle/FOV/color/pain-flash/pitch helpers and VM wrappers (`p_user.cpp` 840–1146). Excludes `SetSkin` 971–986 and chunk777 band 1148+.

## Native ranges

- `src/playsim/p_user.cpp` **player_t::SetLogNumber** 840–876 (37 lines)
- `src/playsim/p_user.cpp` **DEFINE_ACTION_FUNCTION(_PlayerInfo, SetLogNumber)** 878–884 (7 lines)
- `src/playsim/p_user.cpp` **player_t::SetLogText** 886–895 (10 lines)
- `src/playsim/p_user.cpp` **DEFINE_ACTION_FUNCTION(_PlayerInfo, SetLogText)** 897–903 (7 lines)
- `src/playsim/p_user.cpp` **player_t::SetSubtitle** 905–920 (16 lines)
- `src/playsim/p_user.cpp` **DEFINE_ACTION_FUNCTION(_PlayerInfo, SetSubtitleNumber)** 922–929 (8 lines)
- `src/playsim/p_user.cpp` **player_t::GetSpawnClass** 931–935 (5 lines)
- `src/playsim/p_user.cpp` **player_t::SetFOV** 938–961 (24 lines)
- `src/playsim/p_user.cpp` **DEFINE_ACTION_FUNCTION(_PlayerInfo, SetFOV)** 963–969 (7 lines)
- `src/playsim/p_user.cpp` **EnumColorSets** 997–1000 (4 lines)
- `src/playsim/p_user.cpp` **EnumColorSets** 1002–1020 (19 lines)
- `src/playsim/p_user.cpp` **DEFINE_ACTION_FUNCTION(FPlayerClass, EnumColorSets)** 1022–1028 (7 lines)
- `src/playsim/p_user.cpp` **GetColorSet** 1035–1047 (13 lines)
- `src/playsim/p_user.cpp` **DEFINE_ACTION_FUNCTION(FPlayerClass, GetColorSetName)** 1049–1055 (7 lines)
- `src/playsim/p_user.cpp` **GetPainFlash** 1062–1077 (16 lines)
- `src/playsim/p_user.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(APlayerPawn, GetPainFlashForType)** 1079–1084 (6 lines)
- `src/playsim/p_user.cpp` **GetSoftPitch** 1099–1104 (6 lines)
- `src/playsim/p_user.cpp` **player_t::SendPitchLimits** 1106–1139 (34 lines)
- `src/playsim/p_user.cpp` **DEFINE_ACTION_FUNCTION(_PlayerInfo, SendPitchLimits)** 1141–1146 (6 lines)

**Credited native LOC:** 239 (tally `nativeLoc` 239).

## Managed

`VmPUserLogFovPitch.cs`
Managed LOC (tally): 241.
Conversion commit: `f74c20ab4b5ba9742ed4549352307fa475c0cf89`.

## Padding review (2026-10-21)

| Managed | Native (`p_user.cpp`) |
|---------|-------------------|
| `SetLogNumber` / `SetLogNumberAction` | **840–876**, DEFINE **878–884** |
| `SetLogText` / `SetLogTextAction` | **886–895**, DEFINE **897–903** |
| `SetSubtitle` / `SetSubtitleNumberAction` | **905–920**, DEFINE **922–929** |
| `GetSpawnClass` | **931–935** |
| `PlanSetFov` / `SetFovAction` | **938–961**, DEFINE **963–969** |
| `EnumColorSets` / `EnumColorSetsAction` | **997–1020**, DEFINE **1022–1028** |
| `GetColorSet` / `GetColorSetNameAction` | **1035–1047**, DEFINE **1049–1055** |
| `GetPainFlash` / `GetPainFlashForTypeAction` | **1062–1077**, DEFINE **1079–1084** |
| `GetSoftPitch` | **1099–1104** |
| `PlanSendPitchLimits` / `ApplyPitchLimits` / `SendPitchLimitsAction` | **1106–1139**, DEFINE **1141–1146** |
| `FovWritePlan` / `PitchLimitPlan` | `SetFOV` / `SendPitchLimits` plan records **938–961**, **1106–1139** |

### Out-of-body uncredit (780)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **240** |
| **Out-of-body removed** | **1** |
| **Honest** | **239** |


**Post-review ratio:** 241/239 managed/native ≈ 101% (src file LOC at HEAD; see tally).

## Tests

- `VmPUserLogFovPitchTests.SetLogNumber_UsesStringTableLabel`
- `VmPUserLogFovPitchTests.SetSubtitle_SkipsShareware`
- `VmPUserLogFovPitchTests.PlanSetFov_ClampAndRespectDmflags`
- `VmPUserLogFovPitchTests.EnumColorSets_SortsAndSkipsDeleted`
- `VmPUserLogFovPitchTests.GetPainFlash_MatchesTypeAndClass`
- `VmPUserLogFovPitchTests.SendPitchLimits_AppliesLocalClamp`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 780 --to 780` (0 hard / 0 soft).

## Seams

`GStrings`, `fileSystem`, `cvar_set`, `Net_Write*`, `S_GetMSLength`, `V_IsHardwareRenderer`, `GC`, and `PClass` lookups remain native via injectable seams.

## Credit rationale

Faithful whole-function ports for player messaging and view-limit bands between player copy/GC (chunk778) and weapon/resurrect utilities (chunk777).
