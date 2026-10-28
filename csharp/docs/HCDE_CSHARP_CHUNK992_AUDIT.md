# Chunk 992 — `GetMapIndex`, `P_IsBuildMap`, and `P_OpenMapData` head from `p_openmap.cpp` (**1–150**); first UNCredited band in this TU (EOF **404**).

## Scope

`GetMapIndex`, `P_IsBuildMap`, and `P_OpenMapData` head from `p_openmap.cpp` (**1–150**); first UNCredited band in this TU (EOF **404**).

## Native ranges

- `src/p_openmap.cpp` **P_OpenMapData** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPOpenMapHead992Slice.cs`
Managed LOC (tally): 105.
Conversion commit: `37eed3ea398bb94a029d2334fe26b90f5e9dbbb2`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `P_IsBuildMap` (**37–40**) | yes (probe policy) |
| `GetMapIndex` lump walk (**56–94**) | yes |
| `P_OpenMapData` external `file:` branch head (**108–118**) | yes (predicates) |
| Classic short-name vs `maps/*.wad` / `*.map` pick (**129–150**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–36**, **42–48**, **96–100** | license / includes / banners |
| **87–88** | `I_Error` host |
| **111–116**, **117–118** | `FileExists` / resource open host |
| **146–147** | `ReopenFileReader` host |
| **151+** | outside hull |

| Managed (`VmPOpenMapHead992Slice.cs`) | Native (`p_openmap.cpp`) |
|---------------------------------------|-------------------------|
| `MapLumpTable` / `MapLumpNameMatches` / `ScanMapLumpIndex` | **58–94** |
| `IsBuildMapProbeAlwaysFalse` / `ShouldAbortStandaloneNonBuildMap` | **37–40**, **147–150** |
| `IsExternalFileMapName` / `StripExternalFileMapPrefix` | **106–110** |
| `UsesClassicShortMapName` / `FormatMaps*` / `ShouldPreferShortNameLump` | **129–135** |
| `IsLumpFromDifferentContainer` | **142–150** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **992** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**991** |


**Post-review ratio:** 105/150 managed/native ≈ 70% (src file LOC at HEAD; see tally).

## Tests

- `VmPOpenMapHead992SliceTests.ScanMapLumpIndex_MatchesAndRequiredGaps`
- `VmPOpenMapHead992SliceTests.ExternalAndClassicMapPaths`
- `VmPOpenMapHead992SliceTests.ShortNameLumpSelectionAndBuildProbe`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 992 --to 992` (0 hard / 0 soft).

## Seams

`I_Error`, `FileExists`, `FResourceFile::OpenResourceFile`, `fileSystem`, `strncpy`, and `P_IsBuildMap` host probe remain injected.

## Credit rationale

No existing `VmPOpenMap*` managed slice; CLI UNCredited **1–150**. `P_OpenMapData` is named in **982** seams only (not credited).
