# Chunk 1124 — initFromColors, white set, linedef color indexes, standard/overlay CVAR tables, and am_restorecolors from `am_map.cpp` (**451–600**); follows **1123**.

## Scope

initFromColors, white set, linedef color indexes, standard/overlay CVAR tables, and am_restorecolors from `am_map.cpp` (**451–600**); follows **1123**.

## Native ranges

- `src/am_map.cpp` **Automap colorset tables and restore** 451–600 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapColorsetTables1124Slice.cs`
Managed LOC (tally): 145.
Conversion commit: `b390abcbc935939e6e99d4035ec7996b4db1fc67`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `initFromColors` unused marker and zero fill (**453–470**) | yes |
| `setWhite` / index validity (**472–489**) | yes |
| `AUTOMAP_LINE_COLORS` (**498–514**) | yes |
| `cv_standard` / `cv_overlay` (**522–576**) | yes |
| `am_restorecolors` (**578–593**) | yes |
| Doom background triplet (**599–600**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **451** | closing brace of `initFromCVars` |
| **492–497**, **516–521**, **594–598** | section comments and `NOT_USED` macro site covered by the unused-triplet test |
| **601+** | remaining Doom/Strife palettes outside hull |

| Managed (`VmAmMapColorsetTables1124Slice.cs`) | Native (`am_map.cpp`) |
|------------------------------------------------|---------------------|
| Triplets, line colors, CVAR restore | **451–600** |
| Band hull (exclusive slice) | **451–600** |

### Chunk **1124** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1123** |


**Post-review ratio:** 145/150 managed/native ≈ 97% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapColorsetTables1124SliceTests.InitFromColorTriplets_MarksUnusedAndClearsFillAlpha`
- `VmAmMapColorsetTables1124SliceTests.SetWhite_BlacksBackgroundAndWhitesTheRest`
- `VmAmMapColorsetTables1124SliceTests.LineStyleColorIndex_MapsPortalAndDefault`
- `VmAmMapColorsetTables1124SliceTests.RestoreColorDefaults_ResetsBothTablesAndBothAlphas`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1124 --to 1124` (0 hard / 0 soft).

## Seams

`ResetToDefault` stays a host callback. Doom palette bytes after the background triplet begin at **601**.

## Credit rationale

UNCredited **`am_map.cpp`** **451–600** (+150), inside the 3594-line file, with no conversion or recognition overlap.
