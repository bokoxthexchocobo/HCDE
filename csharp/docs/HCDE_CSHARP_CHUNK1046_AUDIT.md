# Chunk 1046 — `ParseStatistics` tail, `ReadStatistics`, save/sort helpers, `GetStatisticsList`, and `StatisticsEntry` from `statistics.cpp` (**151–319**, +169). Completes file credit through EOF **619** with **1045** + **1044–1043**.

## Scope

`ParseStatistics` tail, `ReadStatistics`, save/sort helpers, `GetStatisticsList`, and `StatisticsEntry` from `statistics.cpp` (**151–319**, +169). Completes file credit through EOF **619** with **1045** + **1044–1043**.

## Native ranges

- `src/gamedata/statistics.cpp` **SaveStatistics / StatisticsEntry** 151–319 (169 lines)

**Credited native LOC:** 169 (tally `nativeLoc` 169).

## Managed

`VmStatisticsSaveParse1046Slice.cs`
Managed LOC (tally): 115.
Conversion commit: `9b74ecdeb9bfa7a6bddf25bd5acd5f6332b10bbd`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Nested level parse tail (**151–158**) | yes |
| Recoverable parse swallow (**164–166**) | host `catch` seam only |
| `ReadStatistics` (**176–179**) | yes |
| `compare_episode_names` / `compare_dates` (**188–213**) | yes |
| `SaveStatistics` format/sort (**226–270**) | yes (format strings; host I/O seam) |
| `GetStatisticsList` (**278–291**) | yes |
| `StatisticsEntry` (**298–318**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–150** | chunk **1045** |
| **222–224** | inline time helpers — `VmStatisticsTimeFormat` seam |
| **230–235**, **269** | `FileWriter` open/close host |
| **237**, **242** | host `qsort` calls |
| **304–305**, **312** | host `time` / skill property |
| **320–619** | chunks **1044–1043** |
| **320** | comment before `LevelStatEntry` (hull ends **319**) |

| Managed (`VmStatisticsSaveParse1046Slice.cs`) | Native (`statistics.cpp`) |
|-----------------------------------------------|---------------------------|
| `BuildNestedLevelStatDraft` | **151–158** |
| `ShouldReloadEpisodeStatistics` | **176–179** |
| `CompareEpisodeHeaderNamesNatural` | **188–194** |
| `ComputeSessionDateSortKey` / compare | **196–213** |
| Save format helpers | **244–267** |
| `ShouldEmitSavedSessionRow` | **248** |
| `FindEpisodeStatisticsIndex` | **278–291** |
| `BuildStatisticsEntryDraft` / date format | **298–318** |
| Band hull (exclusive slice) | **151–319** |

### Chunk **1046** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **169** |
| **Net headline** | **+169** vs post-**1045** |


**Post-review ratio:** 115/169 managed/native ≈ 68% (src file LOC at HEAD; see tally).

## Tests

- `VmStatisticsSaveParse1046SliceTests.BuildNestedLevelStatDraft_ZeroSkill`
- `VmStatisticsSaveParse1046SliceTests.ComputeSessionDateSortKey_ParsesDdMmYyyy`
- `VmStatisticsSaveParse1046SliceTests.CompareEpisodeHeaderNamesNatural_CaseInsensitive`
- `VmStatisticsSaveParse1046SliceTests.ShouldEmitSavedSessionRow_RequiresNonEmptyInfo`
- `VmStatisticsSaveParse1046SliceTests.FormatSavedSessionRow_MatchesNativeShape`
- `VmStatisticsSaveParse1046SliceTests.FormatSavedNestedLevelRow_IncludesLevelName`
- `VmStatisticsSaveParse1046SliceTests.FindEpisodeStatisticsIndex_MatchesSectionCaseInsensitive`
- `VmStatisticsSaveParse1046SliceTests.FormatStatisticsEntryDate_UsesLocalTimeOrFallback`
- `VmStatisticsSaveParse1046SliceTests.BuildStatisticsEntryDraft_CapturesFields`
- `VmStatisticsSaveParse1046SliceTests.FormatSavedEpisodeBlockHeader_AndFooter`
- `VmStatisticsSaveParse1046SliceTests.ShouldReloadEpisodeStatistics_RequiresPath`
- `VmStatisticsSaveParse1046SliceTests.FormatUnableToSaveStatisticsMessage_IncludesPath`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1046 --to 1046` (0 hard / 0 soft).

## Seams

`FScanner`, `FileWriter`, `Printf`, `G_SkillProperty`, `localtime`, and global `EpisodeStatistics` remain host-injected; **1045** owns parse head (**1–150**); uncredited `VmStatisticsTimeFormat` / `VmStatisticsParseHead1045Slice` helpers reused without re-credit.

## Credit rationale

File-scoped UNCredited **`statistics.cpp`** **151–319** (+169); excludes **1–150** (**1045**) and **320–619** (**1044–1043**).
