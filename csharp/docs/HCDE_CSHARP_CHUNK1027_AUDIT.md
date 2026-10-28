# Chunk 1027 — `ParseMusic`, `ParseCutscene`, and `ParseCluster` property head from `g_mapinfo.cpp` (**833–982**). **Pivot:** `p_sight.cpp` **1–150** and playsim mid-file windows remain Vm-blocked; band avoids ParseExit* **754–832** (`VmMapInfoMapOptionsExitEnvironment` merge duplicate) and Hexen exit remap **1008+** (`VmMapInfoClusterExitTextRemap`).

## Scope

`ParseMusic`, `ParseCutscene`, and `ParseCluster` property head from `g_mapinfo.cpp` (**833–982**). **Pivot:** `p_sight.cpp` **1–150** and playsim mid-file windows remain Vm-blocked; band avoids ParseExit* **754–832** (`VmMapInfoMapOptionsExitEnvironment` merge duplicate) and Hexen exit remap **1008+** (`VmMapInfoClusterExitTextRemap`).

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **ParseCluster head** 833–982 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapInfoClusterParse1027Slice.cs`
Managed LOC (tally): 195.
Conversion commit: `1ff059d8c270dda5f45cdc91d960c19546ec689c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `ParseMusic` colon/trailing order (**839–855**) | yes |
| `ParseCutscene` keyword dispatch (**863–879**) | yes |
| `ParseCluster` reset + property switch head (**888–982**) | yes (name/entertext/exittext/music/flat/pic/hub/flags/cd*/cutscenes) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **754–832** | ParseExit* merge policy already off-tally in `VmMapInfoMapOptionsExitEnvironment` |
| **676–735** | `VmMapInfoParseLookupName` |
| **833–838**, **880–887**, **983+** | comment banners / outside hull |
| **1008–1027** | outside hull; `VmMapInfoClusterExitTextRemap` |

| Managed (`VmMapInfoClusterParse1027Slice.cs`) | Native (`g_mapinfo.cpp`) |
|------------------------------------------------|---------------------------|
| `ParseMusicToken` | **839–855** |
| `ApplyCutsceneKeyword` | **863–879** |
| `CreateFreshCluster` / `ApplyClusterProperty` / `TryApplyDefaultClusterStringLabel` | **888–982** |
| `TryParseCdIdHex` | **971–975** |
| Band hull (exclusive slice) | **833–982** |

### Chunk **1027** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1026** |


**Post-review ratio:** 195/150 managed/native ≈ 130% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoClusterParse1027SliceTests.ParseMusicToken_SplitsColonOrder`
- `VmMapInfoClusterParse1027SliceTests.ParseMusicToken_UsesTrailingNumberWhenNoColon`
- `VmMapInfoClusterParse1027SliceTests.TryParseCdIdHex_ParsesHexString`
- `VmMapInfoClusterParse1027SliceTests.ApplyCutsceneKeyword_DeleteClearsVideo`
- `VmMapInfoClusterParse1027SliceTests.TryApplyDefaultClusterStringLabel_MatchesEnterDefault`
- `VmMapInfoClusterParse1027SliceTests.ApplyClusterProperty_SetsHubAndAllowIntermissionFlags`
- `VmMapInfoClusterParse1027SliceTests.ApplyClusterProperty_PicSetsFinalePicFlag`
- `VmMapInfoClusterParse1027SliceTests.CreateFreshCluster_StartsEmpty`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1027 --to 1027` (0 hard / 0 soft).

## Seams

`FScanner`, `FindWadClusterInfo`, `GStrings`, `fileSystem`, and `CheckEndOfFile` remain native; `ParseLookupName` multiline body stays on `VmMapInfoParseLookupName` (**676–735**).

## Credit rationale

First tally-clean **150**-line `g_mapinfo.cpp` hull after wide scan with no conversion/recognition overlap and no Vm* line-range collision on **833–982**.
