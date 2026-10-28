# Chunk 1036 — `ParseMapHeader` tail (`levelnum`, Hexen `$map` music) and `ParseEpisodeInfo` head from `g_mapinfo.cpp` (**2427–2576**). Follows chunk **1035** (**2277–2426**).

## Scope

`ParseMapHeader` tail (`levelnum`, Hexen `$map` music) and `ParseEpisodeInfo` head from `g_mapinfo.cpp` (**2427–2576**). Follows chunk **1035** (**2277–2426**).

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **ParseMapHeader tail / ParseEpisodeInfo head** 2427–2576 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapInfoParseMapHeader1036Slice.cs`
Managed LOC (tally): 128.
Conversion commit: `818ae0dead4c29e3b9cccb26af0659d231cd252a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Title block close braces (**2427–2428**) | seam (structure only) |
| Default `levelnum` + `id24_levelnum` (**2430–2432**) | yes |
| HexenMusic `$map` default music (**2434–2440**) | yes |
| `return levelinfo` (**2442**) | seam (host) |
| `ParseEpisodeInfo` map + teaser (**2473–2483**) | yes |
| Episode body keywords (**2489–2530**) | yes |
| Unknown episode property / break (**2532–2541**) | yes |
| `CheckEndOfFile("episode")` (**2543**) | seam (host) |
| Extended Heretic gate (**2545–2549**) | yes |
| Optional map existence gate (**2551–2558**) | yes |
| Find episode by map (**2562–2568**) | yes |
| Remove branch (**2570–2574**) | yes |
| Add/replace branch head (**2575–2576**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1827–2426** | chunks **1033–1035** (title/tokens — not re-credited) |
| **2446–2457** | `ParseEpisodeInfo` comment banner |
| **2577–2591** | episode field assign tail outside hull (**2576** ends on `else {`) |
| **2600–3049** | prior credited mapinfo bands |
| **3050–3151** | deferred (**102** lines) |
| `VmMapInfoEpisodeParser` | uncredited shared catalog — seam only |

| Managed (`VmMapInfoParseMapHeader1036Slice.cs`) | Native (`g_mapinfo.cpp`) |
|--------------------------------------------------|---------------------------|
| `AssignLevelNumFromMapName` | **2430–2432** |
| `TryAssignHexenMapMusic` | **2437–2440** |
| `ApplySharewareTeaserMap` | **2476–2482** |
| `ClassifyEpisodeBodyKeyword` | **2489–2530** |
| `ShouldBreakEpisodeBodyLoop` / `ShouldEmitUnknownEpisodeProperty` | **2532–2541** |
| `ShouldIgnoreExtendedEpisode` | **2545–2549** |
| `ShouldSkipOptionalEpisode` | **2551–2558** |
| `FindEpisodeMapIndex` | **2562–2568** |
| `ShouldRemoveEpisodeEntry` / `ApplyEpisodeRemoveAt` | **2570–2574** |
| `ShouldReserveNewEpisodeSlot` / `MergeEpisodeDraft` | **2575–2576** |
| `NormalizeEpisodeShortcut` / `SynthesizeEpisodeNameFromPic` | **2520–2521**, **2511** |
| Band hull (exclusive slice) | **2427–2576** |

### Chunk **1036** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1035** |


**Post-review ratio:** 128/150 managed/native ≈ 85% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoParseMapHeader1036SliceTests.AssignLevelNumFromMapName_UsesDefaultLevelNum`
- `VmMapInfoParseMapHeader1036SliceTests.TryAssignHexenMapMusic_LookupByLevelNum`
- `VmMapInfoParseMapHeader1036SliceTests.ApplySharewareTeaserMap_OnlyWhenShareware`
- `VmMapInfoParseMapHeader1036SliceTests.ShouldIgnoreExtendedEpisode_WhenMenuHackMissing`
- `VmMapInfoParseMapHeader1036SliceTests.ShouldSkipOptionalEpisode_WhenMapMissing`
- `VmMapInfoParseMapHeader1036SliceTests.FindEpisodeMapIndex_IsCaseInsensitive`
- `VmMapInfoParseMapHeader1036SliceTests.SynthesizeEpisodeNameFromPic_PrefixesDollar`
- `VmMapInfoParseMapHeader1036SliceTests.MergeEpisodeDraft_ReplacesExistingMap`
- `VmMapInfoParseMapHeader1036SliceTests.ClassifyEpisodeBodyKeyword_RecognizesRemove`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1036 --to 1036` (0 hard / 0 soft).

## Seams

`HexenMusic` map, `P_CheckMapData`, `ParseCutscene`, `ParseOpenBrace` / `ParseCloseBrace`, `CheckEndOfFile`, and `AllEpisodes` storage remain host-injected; uncredited `VmMapInfoEpisodeParser` is a parallel helper — not re-credited.

## Credit rationale

UNCredited **`g_mapinfo.cpp`** **2427–2576**; abuts **1035** at title close (**2427–2428**) without re-porting title logic; no overlap with **2600–3049** or deferred **3050–3151**.
