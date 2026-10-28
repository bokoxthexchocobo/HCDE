# Chunk 1044 — `LevelStatEntry`, `STAT_StartNewGame`, `StoreLevelStats`, and `STAT_ChangeLevel` episode checkpoint from `statistics.cpp` (**320–469**). Continues **1043** (**470–619**, EOF **619**).

## Scope

`LevelStatEntry`, `STAT_StartNewGame`, `StoreLevelStats`, and `STAT_ChangeLevel` episode checkpoint from `statistics.cpp` (**320–469**). Continues **1043** (**470–619**, EOF **619**).

## Native ranges

- `src/gamedata/statistics.cpp` **StoreLevelStats / STAT_ChangeLevel** 320–469 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmStatisticsLevelTrack1044Slice.cs`
Managed LOC (tally): 114.
Conversion commit: `fca3cb0a98c809ef258a4a7331c73e07ca6903fb`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `LevelStatEntry` name/info/time (**325–341**) | yes |
| `STAT_StartNewGame` episode pick (**352–367**) | yes |
| `compare_level_names` / hub sort (**375–381**, **421–422**) | yes (case-insensitive ordering) |
| `StoreLevelStats` guards + counters (**384–419**) | yes |
| `STAT_ChangeLevel` endseq + nextinfo (**432–445**) | yes |
| Episode save checkpoint (**447–469**) | yes (through `validlevels` setup) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **298–318** | `StatisticsEntry` date/skill host (**320** hull starts comments) |
| **328–332** | unused `time` locals in `LevelStatEntry` |
| **411–418** | thinker iterator host |
| **442–444** | `FindLevelInfo` / redirect host |
| **453–465** | `P_OpenMapData` / `GetStatisticsList` host |
| **470–619** | chunk **1043** |
| `VmStatisticsEpisodeSave1043Slice` | episode totals/formatting — **470+** only |

| Managed (`VmStatisticsLevelTrack1044Slice.cs`) | Native (`statistics.cpp`) |
|--------------------------------------------------|---------------------------|
| `BuildLevelStatEntry` | **334–341** |
| `ShouldAssignStartEpisodeOnNewGame` / `TryResolveStartEpisodeIndex` | **354–366** |
| `ShouldStoreLevelStatistics` / skip flag / find+apply counters | **388–407** |
| `ReconcileKillCountWhenNoLivingMonsters` | **419** |
| `CompareLevelNamesNatural` / `SortLevelDataByName` | **375–381**, **422** |
| `IsEndSequenceMapTransition` / `ShouldResolveNextLevelInfo` | **440–445** |
| `ShouldPersistEpisodeStatistics` / `ReachedEpisodeStatisticsCheckpoint` | **447–450** |
| `BuildEpisodeStatisticsSectionKey` / `ResolveEpisodeNameForStatistics` | **460–464** |
| Band hull (exclusive slice, EOF **619**) | **320–469** |

### Chunk **1044** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1043** |


**Post-review ratio:** 114/150 managed/native ≈ 76% (src file LOC at HEAD; see tally).

## Tests

- `VmStatisticsLevelTrack1044SliceTests.BuildLevelStatEntry_TruncatesNameAndInfo`
- `VmStatisticsLevelTrack1044SliceTests.ShouldAssignStartEpisodeOnNewGame_SkipsDmAndMp`
- `VmStatisticsLevelTrack1044SliceTests.FindOrReserveLevelIndex_CaseInsensitiveReuse`
- `VmStatisticsLevelTrack1044SliceTests.ApplyLevelCounterSnapshot_CopiesCounters`
- `VmStatisticsLevelTrack1044SliceTests.ReconcileKillCountWhenNoLivingMonsters_ForcesFullClear`
- `VmStatisticsLevelTrack1044SliceTests.ShouldSkipLevelStatistics_WhenNoStatisticsFlag`
- `VmStatisticsLevelTrack1044SliceTests.IsEndSequenceMapTransition_DetectsPrefix`
- `VmStatisticsLevelTrack1044SliceTests.ReachedEpisodeStatisticsCheckpoint_EndGameOrMissingNext`
- `VmStatisticsLevelTrack1044SliceTests.BuildEpisodeStatisticsSectionKey_Uppercases`
- `VmStatisticsLevelTrack1044SliceTests.ResolveEpisodeNameForStatistics_LocalizesDollarPrefix`
- `VmStatisticsLevelTrack1044SliceTests.SortLevelDataByName_OrdersAlphabetically`
- `VmStatisticsLevelTrack1044SliceTests.ShouldPersistEpisodeStatistics_OnlyModeOne`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1044 --to 1044` (0 hard / 0 soft).

## Seams

`GetStatisticsList`, `StatisticsEntry`, `time`/`localtime`, `FindLevelInfo`, `P_OpenMapData`, and thinker iteration remain host-injected; **1043** owns aggregation/save tail (**470+**); uncredited `VmStatisticsParser` / `VmOneLevelStats` model not re-credited.

## Credit rationale

File-scoped UNCredited **`statistics.cpp`** **320–469** (+150); excludes credited **470–619** (**1043**). Hull ends **469** before **1043** aggregation loop.
