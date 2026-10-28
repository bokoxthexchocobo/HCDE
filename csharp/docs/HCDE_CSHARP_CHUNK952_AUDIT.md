# Chunk 952 — `DumpScriptStatus` loop tail, `ACSProfileInfo`, profile arrangement/clear/sort helpers, and `ShowProfileData` head from `p_acs.cpp` (**11100–11249**). Follows **951** (**≤11099**); avoids **10050–10199** credited corridor.

## Scope

`DumpScriptStatus` loop tail, `ACSProfileInfo`, profile arrangement/clear/sort helpers, and `ShowProfileData` head from `p_acs.cpp` (**11100–11249**). Follows **951** (**≤11099**); avoids **10050–10199** credited corridor.

## Native ranges

- `src/playsim/p_acs.cpp` **DACSThinker::DumpScriptStatus** 11100–11249 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsAcsProfile952Slice.cs`
Managed LOC (tally): 87.
Conversion commit: `919540dab2ed0e8fd03adeb59ff6f3b5e8e71c7c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `DumpScriptStatus` while body (**11100–11103**) | yes |
| `ACSProfileInfo` ctor/reset/add (**11108–11133**) | yes |
| `ArrangeScriptProfiles` / `ArrangeFunctionProfiles` (**11135–11169**) | yes |
| `ClearProfiles` (**11171–11177**) | yes |
| `sort_by_*` comparators (**11179–11221**) | yes |
| `ShowProfileData` head (**11223–11249**) | yes |

| Managed (`VmPAcsAcsProfile952Slice.cs`) | Native (`p_acs.cpp`) |
|------------------------------------------------|---------------------|
| `DumpScriptStatusLoopContinues` / `FormatDumpScriptStatusLine` | **11099–11102** |
| `CreateResetProfileStats` / `ApplyProfileRun` / `ResetProfileStats` | **11113–11133** |
| `ShouldCollectFunctionProfile` | **11161–11166** |
| `CompareProfile*` / `AverageInstrPerRun` | **11179–11221** |
| `ShowProfileDataShouldReturn` / `ResolveProfileDisplayLimit` | **11228–11248** |
| `ProfileTypeLabel` / `ProfileListingTitle` | **11226–11247** |
| `ShouldSkipUnrunProfile` / `ProfileRowWithinLimit` | **11252–11257** (predicate logic; band ends before row loop body) |

### Chunk **952** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**951** |


**Post-review ratio:** 87/150 managed/native ≈ 58% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsAcsProfile952SliceTests.FormatDumpScriptStatusLine_UsesStateName`
- `VmPAcsAcsProfile952SliceTests.CreateResetProfileStats_UsesUintMaxMin`
- `VmPAcsAcsProfile952SliceTests.ApplyProfileRun_UpdatesMinMaxAndTotals`
- `VmPAcsAcsProfile952SliceTests.ShouldCollectFunctionProfile_OnlyWhenNotImported`
- `VmPAcsAcsProfile952SliceTests.CompareProfileTotalInstr_SortsDescending`
- `VmPAcsAcsProfile952SliceTests.AverageInstrPerRun_ZeroWhenNoRuns`
- `VmPAcsAcsProfile952SliceTests.ResolveProfileDisplayLimit_AllWhenNonPositive`
- `VmPAcsAcsProfile952SliceTests.ShouldSkipUnrunProfile_WhenNumRunsZero`
- `VmPAcsAcsProfile952SliceTests.ProfileListingTitle_UsesTopPrefixWhenLimited`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 952 --to 952` (0 hard / 0 soft).

## Seams

`Printf`, `ScriptPresentation`, `StaticModules`, `qsort`, and profile row formatting past the band remain host seams.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmPAcsAcsProfile952Slice.cs` (ACS VM execution is not matrix credit).
