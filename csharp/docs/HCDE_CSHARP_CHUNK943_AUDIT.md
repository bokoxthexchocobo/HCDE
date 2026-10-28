# Chunk 943 — `ACSF_SoundVolume` through `ACSF_CheckFont` from `p_acs.cpp` (**6150–6299**). Retarget from **6000–6149** (overlaps recognition **66**/**76**); outside deferred **7366–7509**.

## Scope

`ACSF_SoundVolume` through `ACSF_CheckFont` from `p_acs.cpp` (**6150–6299**). Retarget from **6000–6149** (overlaps recognition **66**/**76**); outside deferred **7366–7509**.

## Native ranges

- `src/playsim/p_acs.cpp` **ACSF_SoundVolume** 6150–6299 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsCallFuncSoundString943Slice.cs`
Managed LOC (tally): 80.
Conversion commit: `fd7c1bbf4375dc5fe0411aa199dfb78aa829447c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_acs.cpp`) |
|---------|----------------------|
| chunk **943** hull | **6150–6299** |
| `SoundVolumeUsesActivator` | `ACSF_SoundVolume` **6157–6169** |
| `StringCompareSameIndex` | `strcmp`/`stricmp` fast path **6180–6182** |
| `NullLookupToEmpty` | invalid strings **6188–6189** |
| `StringCompareWithOptionalLength` | **6191–6199** |
| `TrimSubstringLength` | `StrLeft`/`StrRight` **6215–6217** |
| `StrMidPosClamped` / `StrMidLengthClamped` | `StrMid` **6236–6243** |
| `GetWeaponReturnsNone` | **6249–6252** |
| `SpawnDecalUsesActivator` | **6273–6278** |
| `SpawnDecalZOffset` / `SpawnDecalDistance` | flags **6270–6271** |
| `SpawnDecalTemplateMissing` | **6265–6266** |
| `CheckFontFound` | `CheckFont` **6298** |
| `MinArgCountMet` | `MIN_ARG_COUNT` throughout |
| `ActivatorRequiredForSpawn` / `AccumulateSpawnDecalCount` | **6273–6288** |
| `ResolveEmptyStringReturn` | empty `StrMid`/`StrLeft` **6208–6210** |

### Chunk **943** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**942** |


**Post-review ratio:** 80/150 managed/native ≈ 53% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsCallFuncSoundString943SliceTests.SoundVolumeUsesActivator_WhenTidZero`
- `VmPAcsCallFuncSoundString943SliceTests.StringCompareSameIndex_ReturnsZeroWithoutLookup`
- `VmPAcsCallFuncSoundString943SliceTests.TrimSubstringLength_ClampsToOldLen`
- `VmPAcsCallFuncSoundString943SliceTests.StrMidPosClamped_ReturnsNegativeWhenPastEnd`
- `VmPAcsCallFuncSoundString943SliceTests.SpawnDecalDistance_DefaultsToSixtyFour`
- `VmPAcsCallFuncSoundString943SliceTests.GetWeaponReturnsNone_WhenActivatorMissing`
- `VmPAcsCallFuncSoundString943SliceTests.NullLookupToEmpty_UsesEmptyString`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 943 --to 943` (0 hard / 0 soft).

## Seams

`S_ChangeActorSoundVolume`, `LookupString`, `GlobalACSStrings`, `DoSpawnDecal`, `V_GetFont`, and `DecalLibrary` remain host seams.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmPAcsCallFuncSoundString943Slice.cs` (`AcsCallFunctions` handlers are not matrix credit).
