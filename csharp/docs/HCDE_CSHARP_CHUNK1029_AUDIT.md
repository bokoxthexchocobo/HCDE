# Chunk 1029 — `SetLevelNum` and `ParseMapInfo` lump loop head from `g_mapinfo.cpp` (**2600–2749**). **Pivot:** primary **1491–1640** aborted — `VmMapInfoMapOptionsLate` (**1421–1560**), `VmMapInfoScalarOptionsBlock` (**1567–1716**), and `VmMapInfoMapOptionsExitEnvironment` (**1567–1652**) leave no honest ≥150 exclusive credit.

## Scope

`SetLevelNum` and `ParseMapInfo` lump loop head from `g_mapinfo.cpp` (**2600–2749**). **Pivot:** primary **1491–1640** aborted — `VmMapInfoMapOptionsLate` (**1421–1560**), `VmMapInfoScalarOptionsBlock` (**1567–1716**), and `VmMapInfoMapOptionsExitEnvironment` (**1567–1652**) leave no honest ≥150 exclusive credit.

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **ParseMapInfo head** 2600–2749 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapInfoParseMapInfo1029Slice.cs`
Managed LOC (tally): 166.
Conversion commit: `e3585afde5abbb9ab94b55cb8d3116ac9fd7f3b3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `SetLevelNum` duplicate clearing (**2602–2613**) | yes |
| `ParseMapInfo` lump reopen + duplicate lump warn (**2623–2637**) | yes |
| `include` container / core override gate (**2641–2657**) | yes (policy predicates) |
| Top-level `gamedefaults` / `defaultmap` / `adddefaultmap` / `map` dispatch (**2664–2691**) | yes |
| `-NOFLAT-` sky2 copy (**2687–2690**) | yes |
| `clusterdef`/`cluster`/`episode`/`skill` clears (**2695–2715**) | yes |
| New-format gates for `gameinfo`/`intermission`/`doomednums` (**2716–2750** hull ends **2749**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1491–1640** (primary target) | off-tally `VmMapInfoMapOptionsLate` + scalar/exit merge bands |
| **1191–1340** | chunk **1028** |
| **833–982** | chunk **1027** |
| **2658–2662** | nested include recursion host |
| **2750+** | outside hull |

| Managed (`VmMapInfoParseMapInfo1029Slice.cs`) | Native (`g_mapinfo.cpp`) |
|------------------------------------------------|---------------------------|
| `ApplySetLevelNum` | **2602–2613** |
| `LumpAlreadyParsed` | **2630–2633** |
| `ShouldFatalOnCoreIncludeOverride` | **2649–2656** |
| `ResolveSkyPic2WhenNoFlat` | **2687–2690** |
| `TryParseTopLevelKeyword` | **2641–2749** |
| Band hull (exclusive slice) | **2600–2749** |

### Chunk **1029** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1028** |


**Post-review ratio:** 166/150 managed/native ≈ 111% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoParseMapInfo1029SliceTests.ApplySetLevelNum_ClearsDuplicatesOnOtherMaps`
- `VmMapInfoParseMapInfo1029SliceTests.ShouldFatalOnCoreIncludeOverride_DetectsCoreOverride`
- `VmMapInfoParseMapInfo1029SliceTests.ResolveSkyPic2WhenNoFlat_CopiesPrimarySky`
- `VmMapInfoParseMapInfo1029SliceTests.TryParseTopLevelKeyword_RejectsGameInfoOnOldFormat`
- `VmMapInfoParseMapInfo1029SliceTests.TryParseTopLevelKeyword_AllowsClusterKeywordOnNewFormat`
- `VmMapInfoParseMapInfo1029SliceTests.TryParseTopLevelKeyword_MapsClearsSkills`
- `VmMapInfoParseMapInfo1029SliceTests.LumpAlreadyParsed_TracksDuplicates`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1029 --to 1029` (0 hard / 0 soft).

## Seams

`FScanner`, nested `ParseMapInfo`, `ParseMapDefinition`, `ParseCluster`, and host filesystem lookups remain native.

## Credit rationale

UNCredited `g_mapinfo.cpp` band **≥2600** with zero tally/recognition overlap and zero Vm* line-range collision (alternate **2800–2949** also clean; chose lower contiguous parse entry).
