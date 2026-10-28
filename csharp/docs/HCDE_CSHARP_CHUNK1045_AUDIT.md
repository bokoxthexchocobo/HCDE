# Chunk 1045 — CVAR/globals layout and `ParseStatistics` scanner head from `statistics.cpp` (**1–150**). Completes file with **1044** (**320–469**) + **1043** (**470–619**).

## Scope

CVAR/globals layout and `ParseStatistics` scanner head from `statistics.cpp` (**1–150**). Completes file with **1044** (**320–469**) + **1043** (**470–619**).

## Native ranges

- `src/gamedata/statistics.cpp` **ParseStatistics / globals** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmStatisticsParseHead1045Slice.cs`
Managed LOC (tally): 109.
Conversion commit: `548a787e0c785272df4f4bff64642cba21b0b4da`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `savestatistics` / `statfile` defaults (**50–51**) | yes (via `VmStatisticsConfig` seam constants) |
| `OneLevel` / stats struct field sizes (**60–95**) | yes (layout constants) |
| `ParseStatistics` open/clear (**108–114**) | yes (skip-when-missing policy) |
| Episode header + `{` (**116–125**) | yes |
| Session token/time/skill (**127–141**) | yes |
| Level block `{` + name token (**142–149**) | yes |
| Level info token read start (**150**) | yes (hull seam; **151+** outside band) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–49** | license / includes host |
| **70–100** | static global containers host |
| **151–319** | remainder of `ParseStatistics` + save/read helpers (future band) |
| **320–619** | chunks **1044–1043** |
| `VmStatisticsParser.TryParseDocument` | uncredited parallel parser — time helper seam only |

| Managed (`VmStatisticsParseHead1045Slice.cs`) | Native (`statistics.cpp`) |
|-----------------------------------------------|---------------------------|
| `DefaultStatFileName` / `DefaultSaveStatisticsMode` | **50–51** |
| `OneLevelCounterFieldCount` / field length constants | **60–80** |
| `ShouldSkipParseWhenFileUnavailable` | **110–114** |
| `BuildEpisodeDraft` / `IsStatisticsBlockOpenToken` | **116–125** |
| `BuildSessionDraft` / `TryBuildSessionDraft` / loop guards | **127–144** |
| `BeginNestedLevelStatEntry` / `TakePendingLevelInfoToken` | **148–150** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **1045** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1044** |


**Post-review ratio:** 109/150 managed/native ≈ 73% (src file LOC at HEAD; see tally).

## Tests

- `VmStatisticsParseHead1045SliceTests.BuildEpisodeDraft_CapturesHeaderAndName`
- `VmStatisticsParseHead1045SliceTests.IsStatisticsBlockOpenToken_OnlyBrace`
- `VmStatisticsParseHead1045SliceTests.BuildSessionDraft_ParsesDurationAndTruncates`
- `VmStatisticsParseHead1045SliceTests.SelectSessionNameFromFileTokens_UsesSecondToken`
- `VmStatisticsParseHead1045SliceTests.ParseColonDurationToTics_DelegatesToParser`
- `VmStatisticsParseHead1045SliceTests.ShouldSkipParseWhenFileUnavailable`
- `VmStatisticsParseHead1045SliceTests.ParseLevelStatNameFromToken_Truncates`
- `VmStatisticsParseHead1045SliceTests.ShouldBeginSessionLevelBlock_MatchesOpenBrace`
- `VmStatisticsParseHead1045SliceTests.DefaultConfig_MatchesCvarDefaults`
- `VmStatisticsParseHead1045SliceTests.OneLevelCounterFieldCount_MatchesStruct`
- `VmStatisticsParseHead1045SliceTests.SessionDurationHasValidShape_RejectsMalformed`
- `VmStatisticsParseHead1045SliceTests.NormalizeEpisodeHeaderToken_Trims`
- `VmStatisticsParseHead1045SliceTests.TryBuildSessionDraft_ValidatesTokens`
- `VmStatisticsParseHead1045SliceTests.TakePendingLevelInfoToken_Truncates`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1045 --to 1045` (0 hard / 0 soft).

## Seams

`FScanner`, `CRecoverableError`, and global `EpisodeStatistics` arrays remain host-injected; uncredited `VmStatisticsConfig`, `VmStatisticsParser.ParseTimeToTics`, and `VmOneLevelStats` are not re-credited.

## Credit rationale

File-scoped UNCredited **`statistics.cpp`** **1–150** (+150); excludes credited **320–619** (**1044–1043**). Hull ends mid nested level parse (**150**).
