# Chunk 1023 — `DLevelScript::CallFunction` ACSF named-script through `ACSF_PlaySound`/`PlayActorSound` heads from `p_acs.cpp` (**5965–6114**). Follows chunk **1022** (**4728–4877**); stops before chunk **943** (**6150+**).

## Scope

`DLevelScript::CallFunction` ACSF named-script through `ACSF_PlaySound`/`PlayActorSound` heads from `p_acs.cpp` (**5965–6114**). Follows chunk **1022** (**4728–4877**); stops before chunk **943** (**6150+**).

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::CallFunction mid band** 5965–6114 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsCallfuncGameplay1023Slice.cs`
Managed LOC (tally): 109.
Conversion commit: `aeae10a859a80cbd4136525b5ba59214326f5f06`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `ACSF_ACS_NamedExecute*` arg packing (**5954–5963**) | yes (`NamedExecuteScriptIndex` / optional args) |
| `ACSF_UniqueTID` / `ACSF_IsTIDUsed` (**5967–5972**) | yes |
| `ACSF_Sqrt` / `FixedSqrt` / `VectorLength` (**5974–5984**) | yes |
| `ACSF_SetHUDClipRect` / `SetHUDWrapWidth` (**5986–5997**) | yes |
| `ACSF_GetCVarString` … `SetUserCVarString` min-arg gates (**5999–6035**) | yes |
| `ACSF_LineAttack` FH flag map + activator TID (**6038–6077**) | yes |
| `ACSF_PlaySound` / `PlayActorSound` default args (**6079–6104**) | yes |

| Managed (`VmPAcsCallfuncGameplay1023Slice.cs`) | Native (`p_acs.cpp`) |
|------------------------------------------------|---------------------|
| `NamedExecuteScriptIndex` / `NamedExecuteOptionalArg` | **5956–5960** |
| `UniqueTidStartArg` / `UniqueTidMinInUseArg` | **5968** policy |
| `IsTidUsedMeetsMinArgs` | **5971** |
| `IntegerSqrtRoundDown` | **5976** |
| `FixedSqrtFromAcs` / `VectorLengthFixed` | **5980**, **5984** |
| `BuildHudClipRect` / `HudWrapWidth` | **5987–5996** |
| `SetCvarMeetsMinArgs` / user CVar gates | **6000–6035** |
| `LineAttackFhFlags` / `LineAttackUsesActivator` | **6050–6054** |
| `BuildPlaySoundArgs` / `PlaySoundUsesActivator` | **6099–6103** |
| Band hull (exclusive slice) | **5965–6114** |

### Chunk **1023** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1022** |


**Post-review ratio:** 109/150 managed/native ≈ 73% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsCallfuncGameplay1023SliceTests.NamedExecuteScriptIndex_NegatesValidNameIndex`
- `VmPAcsCallfuncGameplay1023SliceTests.UniqueTidStartArg_DefaultsWhenEmpty`
- `VmPAcsCallfuncGameplay1023SliceTests.IntegerSqrtRoundDown_MatchesNativeRoundDown`
- `VmPAcsCallfuncGameplay1023SliceTests.FixedSqrtAndVectorLength_Use65536Scale`
- `VmPAcsCallfuncGameplay1023SliceTests.BuildHudClipRect_DefaultsHandleAspectTrue`
- `VmPAcsCallfuncGameplay1023SliceTests.LineAttackFhFlags_MapsNativeBits`
- `VmPAcsCallfuncGameplay1023SliceTests.BuildPlaySoundArgs_DefaultChannelAndVolume`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1023 --to 1023` (0 hard / 0 soft).

## Seams

`P_ExecuteSpecial`, `DoGetCVar`/`SetCVar`, `HCDE_ResolveACSPlayer`, `P_LineAttack`, `S_FindSound`/`S_PlaySound`, and `GetActorSound` remain host seams.

## Credit rationale

Primary **1023** target `p_map.cpp` **6161–6310** rejected (`VmPMapRadiusDamage` **6130–6480** off-tally duplicate). Alternate **4878–5027** rejected (tally overlap chunk **10**/**58**/**54**; no 150-line conversion-clean hull). First UNCredited **150**-line `CallFunction` band before **943** with dedicated slice (recognition **66**/**76** slivers are zero-credit markers only).
