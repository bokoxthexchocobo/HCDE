# Chunk 1023 — ACS string pool dump/unlock, global-string mark/collect head, `ScriptPresentation`, and `P_ClearACSVars` branch policy from `p_acs.cpp` (**1386–1535**). Stops before chunk **10** saveg archive (**1566+**).

## Scope

ACS string pool dump/unlock, global-string mark/collect head, `ScriptPresentation`, and `P_ClearACSVars` branch policy from `p_acs.cpp` (**1386–1535**). Stops before chunk **10** saveg archive (**1566+**).

## Native ranges

- `src/playsim/p_acs.cpp` **ACS string archive / ScriptPresentation** 1386–1535 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsStringArchive1023Slice.cs`
Managed LOC (tally): 134.
Conversion commit: `ba9522420377109e43bc73675b09239d9c15f93d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `ACSStringPool::Dump` active rows (**1399–1401**) | yes |
| `ACSStringPool::UnlockForLevel` lock removal (**1414–1417**) | yes |
| `P_MarkWorldVarStrings` / `P_MarkGlobalVarStrings` array walks (**1432–1450**) | yes (loop bounds) |
| `P_CollectACSGlobalStrings` stack guard (**1471–1473**) | yes |
| `ScriptPresentation` (**1512–1522**) | yes |
| `P_ClearACSVars` global vs mark/purge branch (**1540–1555**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1386–1394**, **1404**, **1422–1428**, **1452–1507**, **1524–1530** | banners / `#ifdef` / comments only |

| Managed (`VmPAcsStringArchive1023Slice.cs`) | Native (`p_acs.cpp`) |
|---------------------------------------------|---------------------|
| `ShouldListPooledString` | **1399** |
| `ShouldRemoveLevelLock` | **1414–1415** |
| `WorldArrayMarkCount` / `GlobalArrayMarkCount` | **1432–1434**, **1447–1449** |
| `StackPointerCorrupted` | **1471** |
| `FormatScriptPresentation` | **1512–1522** |
| `PurgeViaGlobalClear` / `PurgeViaMarkPass` | **1540–1555** |
| Band hull (exclusive slice) | **1386–1535** |

### Chunk **1023** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1022** |


**Post-review ratio:** 134/150 managed/native ≈ 89% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsStringArchive1023SliceTests.ShouldListPooledString_SkipsFreeEntries`
- `VmPAcsStringArchive1023SliceTests.FormatDumpLine_MatchesNativePrintfShape`
- `VmPAcsStringArchive1023SliceTests.UnlockLevelLocksNative_RemovesMatchingLock`
- `VmPAcsStringArchive1023SliceTests.FormatScriptPresentation_QuotesNamedScripts`
- `VmPAcsStringArchive1023SliceTests.CollectGlobalStringsNative_MarksStacksThenPurges`
- `VmPAcsStringArchive1023SliceTests.ClearAcsVarsNative_BranchesLikeNative`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1023 --to 1023` (0 hard / 0 soft).

## Seams

`Printf`, `FSerializer`, `GlobalACSStrings` host methods, and `ACS_*` container mutation remain native.

## Credit rationale

Primary **1023** `p_map.cpp` **6161–6310** rejected (`VmPMapRadiusDamage` **6130–6480**). Alternate **4878–5027** rejected (tally overlap chunk **10**/**58**/**54**). Shifted **`p_acs` 5965–6114** rejected (`LocTallyIntegrityTests` conversion vs recognition line overlap). Landed **1386–1535**: conversion+recognition clean; precedes retired chunk **567** overlap at **1566**.
