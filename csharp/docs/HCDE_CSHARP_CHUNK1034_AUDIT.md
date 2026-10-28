# Chunk 1034 — `ParseMapDefinition` flag-handler switch body and registered-option / unknown-property fallback from `g_mapinfo.cpp` (**2127–2276**). Follows chunk **1033** (**1977–2126**).

## Scope

`ParseMapDefinition` flag-handler switch body and registered-option / unknown-property fallback from `g_mapinfo.cpp` (**2127–2276**). Follows chunk **1033** (**1977–2126**).

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **ParseMapDefinition flag switch / option fallback** 2127–2276 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapInfoParseMapDefinition1034Slice.cs`
Managed LOC (tally): 148.
Conversion commit: `a4195118fb9e3111fbf5286db666f6cf77cf9f80`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `MITYPE_EATNEXT` arm (**2128–2131**) | yes (host seam; slice documents flow) |
| `MITYPE_IGNORE` (**2133–2134**) | yes |
| `MITYPE_SETFLAG` / `CLRFLAG` / `SCFLAGS` (**2136–2157**) | yes |
| `MITYPE_SETFLAG2` / `CLRFLAG2` / `SCFLAGS2` (**2159–2180**) | yes |
| `MITYPE_SETFLAG3` / `CLRFLAG3` / `SCFLAGS3` (**2182–2203**) | yes |
| `MITYPE_CLRCOMPATFLAG` (**2205–2210**) | yes |
| `MITYPE_COMPATFLAG` assign + apply (**2212–2241**) | yes |
| `default` assert arm (**2243–2246**) | seam (unreachable) |
| `MapInfoOptions` scan + new-format gate (**2253–2267**) | yes |
| Unknown property / close-brace break (**2269–2276**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1977–2126** | chunk **1033** |
| **2123–2125** | retired **280** option-dispatch seam |
| **2121–2126** | match / switch head (**1033**) |
| **2277–2285** | `CheckEndOfFile` / function close outside hull |
| `VmMapInfoFlagApply` | shared helpers — seam only, no duplicate native lines |
| **3050–3151** | deferred tail (**102** lines) |

| Managed (`VmMapInfoParseMapDefinition1034Slice.cs`) | Native (`g_mapinfo.cpp`) |
|-----------------------------------------------------|---------------------------|
| `HandlerTypeIsIgnore` | **2133–2134** |
| `DispatchMapFlagHandler` (SET/CLR/SC flag arms) | **2136–2203** |
| `ApplyClearFlag3` / `ApplyScFlags3` | **2196–2203** |
| `ApplyClearCompat` via `DispatchMapFlagHandler` | **2205–2210** |
| `ResolveCompatSetValue` / `ApplyCompatHandler` | **2214–2240** |
| `ShouldFatalOnOldFormatOption` / `TryDispatchRegisteredOption` | **2257–2267** |
| `ShouldBreakMapBodyLoop` / `ShouldEmitUnknownPropertyMessage` | **2271–2276** |
| Band hull (exclusive slice) | **2127–2276** |

### Chunk **1034** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1033** |


**Post-review ratio:** 148/150 managed/native ≈ 99% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoParseMapDefinition1034SliceTests.DispatchSetFlag_ClearsBitWhenAssignZero`
- `VmMapInfoParseMapDefinition1034SliceTests.DispatchScFlags2_MergesHandlerMasks`
- `VmMapInfoParseMapDefinition1034SliceTests.ApplyClearFlag3_AndScFlags3_MirrorNativeArms`
- `VmMapInfoParseMapDefinition1034SliceTests.ResolveCompatSetValue_OldFormatUsesCheckNumber`
- `VmMapInfoParseMapDefinition1034SliceTests.ApplyCompatHandler_ClearsWhenSetZero`
- `VmMapInfoParseMapDefinition1034SliceTests.TryDispatchRegisteredOption_RejectsOldFormatOnlyOption`
- `VmMapInfoParseMapDefinition1034SliceTests.TryDispatchRegisteredOption_MatchesKnownToken`
- `VmMapInfoParseMapDefinition1034SliceTests.ShouldBreakMapBodyLoop_WhenCloseBraceSeen`
- `VmMapInfoParseMapDefinition1034SliceTests.HandlerTypeIsIgnore_SkipsMutation`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1034 --to 1034` (0 hard / 0 soft).

## Seams

`ParseAssign`, `CheckAssign`, `sc.MustGet*`, `ParseCloseBrace`, `SkipToNext`, and `MapInfoOptions` handler invocation remain host-injected; flag bit merges delegate to uncredited `VmMapInfoFlagApply`.

## Credit rationale

UNCredited **`g_mapinfo.cpp`** **2127–2276** with zero conversion overlap; excludes **1977–2126** (**1033**) and retired **2123–2125** dispatch seam (no re-credit).
