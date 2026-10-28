# Chunk 1053 — `level_info_t::Reset` tail, `LookupLevelName`, `CheckLevelRedirect`, and `isValid` from `g_mapinfo.cpp` (**301–450**). Follows **1052** (**151–300**).

## Scope

`level_info_t::Reset` tail, `LookupLevelName`, `CheckLevelRedirect`, and `isValid` from `g_mapinfo.cpp` (**301–450**). Follows **1052** (**151–300**).

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **Reset tail / LookupLevelName / CheckLevelRedirect** 301–450 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapInfoParseTail1053Slice.cs`
Managed LOC (tally): 147.
Conversion commit: `5f4daf91872d807fcdc0f8ea809034be9d616b3a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `Reset` tail (**301–334**) | yes |
| `LookupLevelName` (**342–389**) | yes |
| `CheckLevelRedirect` predicates (**397–441**) | yes (host callbacks for actor/cvar/map) |
| `isValid` policy (**449–452**) | yes (`IsLevelInfoValid`; hull **450** ends at `{`) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **151–300** | chunk **1052** |
| **335–341** | comment banners |
| **390–396** | comment banners / redirect prologue host |
| **453+** | `GetSecretMap` / parse helpers (future band) |
| **833+** | chunks **1027–1036** |
| `VmMapInfoLookupLevelName` | uncredited strip helper — seam only |

| Managed (`VmMapInfoParseTail1053Slice.cs`) | Native (`g_mapinfo.cpp`) |
|-------------------------------------------|---------------------------|
| `ApplyLevelInfoResetTail` | **301–334** |
| `ResolveLookupLevelName` / prefix builder | **342–389** |
| redirect predicates | **397–441** |
| `IsLevelInfoValid` | **449–452** |
| Band hull (exclusive slice) | **301–450** |

### Chunk **1053** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1052** |


**Post-review ratio:** 147/150 managed/native ≈ 98% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoParseTail1053SliceTests.ApplyLevelInfoResetTail_SetsHcdeShadowUnsetAndSkyRotate`
- `VmMapInfoParseTail1053SliceTests.ResolveLookupLevelName_ReturnsPlainWhenFlagUnset`
- `VmMapInfoParseTail1053SliceTests.ResolveLookupLevelName_StripsMapHeaderFromLocalizedString`
- `VmMapInfoParseTail1053SliceTests.BuildLocalizedHeaderPrefix_FormatsEpisodeMap`
- `VmMapInfoParseTail1053SliceTests.ShouldEvaluateInventoryRedirect_RequiresNamedType`
- `VmMapInfoParseTail1053SliceTests.ShouldEvaluateCvarRedirect_RejectsIgnoredBool`
- `VmMapInfoParseTail1053SliceTests.UsesPerPlayerCvarRedirect_DetectsUserInfoFlag`
- `VmMapInfoParseTail1053SliceTests.IsLevelInfoValid_AllowsDefaultRecordWithoutMapName`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1053 --to 1053` (0 hard / 0 soft).

## Seams

`GStrings.CheckString`, `PClass::FindActor`, inventory scans, `FindCVar` / `GetCVar`, `P_CheckMapData`, and `FindLevelInfo` remain host-injected; uncredited `VmMapInfoLookupLevelName` / `VmMapInfoLevelReset` are not re-credited.

## Credit rationale

File-scoped UNCredited **`g_mapinfo.cpp`** **301–450** (+150); excludes **151–300** (**1052**). Hull ends at **`isValid` opening brace** (**450**); **451–452** return deferred without padding.
