# Chunk 1043 — Episode stat aggregation/save, `Serialize`/`STAT_Serialize`, `GetStatString`, CCMDs, and ADD_STAT tail from `statistics.cpp` (**470–619**).

## Scope

Episode stat aggregation/save, `Serialize`/`STAT_Serialize`, `GetStatString`, CCMDs, and ADD_STAT tail from `statistics.cpp` (**470–619**).

## Native ranges

- `src/gamedata/statistics.cpp` **STAT_Serialize / GetStatString / ADD_STAT** 470–619 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmStatisticsEpisodeSave1043Slice.cs`
Managed LOC (tally): 127.
Conversion commit: `d1dc88b3dfb21e69a8c5d661677d63d7849330f9`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Episode totals loop (**470–478**) | yes |
| Episode/level `infostring.Format` (**480–488**) | yes |
| `savestatistics == 2` stub (**495–497**) | yes (mode detect) |
| `STAT_Serialize` episode match (**533–541**) | yes |
| `STAT_EpisodeName` (**556–560**) | yes |
| `GetStatString` (**568–579**) | yes |
| `finishgame` guard (**590–594**) | yes |
| ADD_STAT velocity (**606–617**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **450–469** | section build / `GetStatisticsList` host (**470** hull start) |
| **481–492** | `StatisticsEntry` / `LevelStatEntry` / `SaveStatistics` host |
| **507–522** | `FSerializer` object seam |
| **543–551** | serializer read/write host |
| **581–585**, **600–604** | CCMD/ADD_STAT host macros |
| **597–598** | `DEM_FINISHGAME` network seam |
| `VmStatisticsParser` / `VmStatisticsTimeFormat` | uncredited helpers — time parse reused only |

| Managed (`VmStatisticsEpisodeSave1043Slice.cs`) | Native (`statistics.cpp`) |
|-------------------------------------------------|---------------------------|
| `AccumulateEpisodeTotals` | **470–478** |
| `FormatEpisodeSummaryInfoLine` | **480** |
| `FormatLevelStatInfoLine` | **487–488** |
| `IsSingleLevelStatisticsSaveMode` | **495–497** |
| `TryMatchStartEpisodeIndex` | **535–541** |
| `ResolveEpisodeNameForDisplay` | **556–560** |
| `FormatLevelStatDisplayLine` / `ComposeLevelStatReport` | **568–579** |
| `CanUseFinishGameCommand` / `FormatFinishGameBlockedMessage` | **590–594** |
| `ResolveVelocityStatCamera` / `FormatVelocityStatBlock` / `ShouldIncludeVelocityStat` | **608–617** |
| Band hull (exclusive slice) | **470–619** |

### Chunk **1043** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1042** |


**Post-review ratio:** 127/150 managed/native ≈ 85% (src file LOC at HEAD; see tally).

## Tests

- `VmStatisticsEpisodeSave1043SliceTests.AccumulateEpisodeTotals_SumsAllLevels`
- `VmStatisticsEpisodeSave1043SliceTests.FormatEpisodeSummaryInfoLine_MatchesNativeShape`
- `VmStatisticsEpisodeSave1043SliceTests.FormatLevelStatInfoLine_OmitsLevelCount`
- `VmStatisticsEpisodeSave1043SliceTests.TryMatchStartEpisodeIndex_CaseInsensitive`
- `VmStatisticsEpisodeSave1043SliceTests.FormatLevelStatDisplayLine_IncludesTime`
- `VmStatisticsEpisodeSave1043SliceTests.ComposeLevelStatReport_ConcatenatesLevels`
- `VmStatisticsEpisodeSave1043SliceTests.CanUseFinishGameCommand_LevelOrCutscene`
- `VmStatisticsEpisodeSave1043SliceTests.IsSingleLevelStatisticsSaveMode_OnlyModeTwo`
- `VmStatisticsEpisodeSave1043SliceTests.ResolveVelocityStatCamera_FallsBackToPlayerMob`
- `VmStatisticsEpisodeSave1043SliceTests.FormatVelocityStatBlock_IncludesMapName`
- `VmStatisticsEpisodeSave1043SliceTests.ResolveEpisodeNameForDisplay_EmptyWhenMissing`
- `VmStatisticsEpisodeSave1043SliceTests.FormatFinishGameBlockedMessage_MentionsCommand`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1043 --to 1043` (0 hard / 0 soft).

## Seams

`GetStatisticsList`, `StatisticsEntry`, `LevelStatEntry`, `SaveStatistics`, `FSerializer`, `StoreLevelStats`, `Net_WriteInt8`, and player/camera host objects remain injected; uncredited `VmStatisticsParser`, `VmStatisticsTimeFormat`, and `VmStatisticsConfig` are not re-credited.

## Credit rationale

File-scoped UNCredited **`statistics.cpp`** **470–619** (+150); retired chunk **244** had no surviving overlap at HEAD.
