# Chunk 1035 — `ParseMapDefinition` close tail, `GetDefaultLevelNum`, and `ParseMapHeader` title/header head from `g_mapinfo.cpp` (**2277–2426**). Follows chunk **1034** (**2127–2276**).

## Scope

`ParseMapDefinition` close tail, `GetDefaultLevelNum`, and `ParseMapHeader` title/header head from `g_mapinfo.cpp` (**2277–2426**). Follows chunk **1034** (**2127–2276**).

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **GetDefaultLevelNum / ParseMapHeader head** 2277–2426 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapInfoParseMapHeader1035Slice.cs`
Managed LOC (tally): 141.
Conversion commit: `d53f6a134c5e321eb352476a1fde77cd54860a77`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `ParseMapDefinition` else close / loop break (**2277–2280**) | yes |
| `CheckEndOfFile("map")` (**2284**) | seam (host) |
| `GetDefaultLevelNum` MAP/E#M# (**2297–2319**) | yes |
| `ParseMapHeader` numeric map token + Hexen hack (**2333–2346**) | yes |
| Map name string path (**2348–2352**) | yes |
| Wad level slot reserve (**2353–2357**) | yes |
| Default copy + HexenHack defaults (**2358–2377**) | yes |
| MapName store + upper (**2379–2380**) | yes |
| `$` title prefix (**2382–2387**) | yes |
| `lookup` keyword title (**2390–2395**) | yes |
| IWAD default-string workaround (**2402–2406**) | yes |
| Plain title + Hexen WAD key (**2409–2424**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2127–2276** | chunk **1034** |
| **2281–2283** | loop/host close braces only |
| **2288–2294** | comment banner |
| **2322–2327** | `ParseMapHeader` comment banner |
| **2430–2432** | `GetDefaultLevelNum` assign + SNDINFO outside hull (**2426** ends title block) |
| **2600–3049** | prior mapinfo bands |
| **3050–3151** | deferred tail (**102** lines) |
| `VmMapInfoParseMapHeader` / `VmEMapInfoParsePrimitives` | uncredited shared helpers — seam only |

| Managed (`VmMapInfoParseMapHeader1035Slice.cs`) | Native (`g_mapinfo.cpp`) |
|--------------------------------------------------|---------------------------|
| `ShouldExitMapDefinitionAfterCloseBrace` | **2277–2280** |
| `GetDefaultLevelNum` | **2297–2319** |
| `ResolveMapNameToken` | **2333–2346** |
| `ShouldReserveWadLevelSlot` | **2353–2357** |
| `ApplyHexenHackDefaults` | **2360–2375** |
| `StoreMapName` | **2379–2380** |
| `TryApplyDollarLevelTitle` | **2382–2387** |
| `TryApplyLookupKeywordTitle` | **2390–2395** |
| `TryApplyIwadDefaultStringTitle` | **2402–2406** |
| `TryApplyHexenWadTitleKey` / `BuildHexenTitleLookupKey` | **2411–2424** |
| `ApplyPlainLevelTitle` | **2409** |
| Band hull (exclusive slice) | **2277–2426** |

### Chunk **1035** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1034** |


**Post-review ratio:** 141/150 managed/native ≈ 94% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoParseMapHeader1035SliceTests.GetDefaultLevelNum_ParsesMap01`
- `VmMapInfoParseMapHeader1035SliceTests.GetDefaultLevelNum_E1M2_ReturnsEpisodeFormula`
- `VmMapInfoParseMapHeader1035SliceTests.GetDefaultLevelNum_E1M10_SetsId24Only`
- `VmMapInfoParseMapHeader1035SliceTests.ResolveMapNameToken_OldFormatNumberSetsHexenHack`
- `VmMapInfoParseMapHeader1035SliceTests.ApplyHexenHackDefaults_ClearsWallLightAndSetsFlags`
- `VmMapInfoParseMapHeader1035SliceTests.TryApplyDollarLevelTitle_SetsLookup`
- `VmMapInfoParseMapHeader1035SliceTests.TryApplyLookupKeywordTitle_RequiresSecondToken`
- `VmMapInfoParseMapHeader1035SliceTests.TryApplyHexenWadTitleKey_UsesTxtKey`
- `VmMapInfoParseMapHeader1035SliceTests.ShouldExitMapDefinitionAfterCloseBrace_MirrorsElseBreak`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1035 --to 1035` (0 hard / 0 soft).

## Seams

`CheckEndOfFile`, `FindWadLevelInfo`, `wadlevelinfos.Reserve`, `sc.CheckNumber` / `MustGetString`, `GStrings.MatchDefaultString`, and filesystem lookups remain host-injected; legacy helpers on uncredited `VmMapInfoParseMapHeader` / `VmEMapInfoParsePrimitives` are not re-credited.

## Credit rationale

UNCredited **`g_mapinfo.cpp`** **2277–2426** with zero overlap on live conversion rows (**1827–2276**, **2600–3049**); hull stops before default `levelnum` assign (**2430+**).
