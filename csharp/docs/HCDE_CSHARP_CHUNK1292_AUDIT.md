# Chunk 1292 — Theme clamp, archive search, and the noto font list from widgetresourcedata.cpp (1-158).

## Scope

Theme clamp, archive search, and the noto font list from widgetresourcedata.cpp (1-158).

## Native ranges

- `src/widgets/widgetresourcedata.cpp` **InitWidgetResources** 1–158 (158 lines)

**Credited native LOC:** 158 (tally `nativeLoc` 158).

## Managed

`VmWidgetResources1292Slice.cs`
Managed LOC (tally): 209.
Conversion commit: `5df9803fa5b0230413a0e3f7b8c170bb6756acb0`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Theme clamp (**30–34**) | yes |
| Open and theme choice (**38–78**) | yes |
| Close (**80–83**) | yes |
| Archive search (**85–102**) | yes |
| Disk file (**104–115**) | yes |
| Font list (**117–153**) | yes |
| Widget data (**155–158**) | yes |
| Band hull (**1–158**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Archive bytes | host lumps |
| System theme | host flag word |
| Extra files | host list after -file |
| Theme colors | landed theme slice |

The theme cvar clamps below 0 to 0 and above 2 to 2. Dark is selected when the cvar is 1, or when it is 0 and the system theme has the dark bit. Contrast is the high-contrast bit. The required archive is opened first and a missing file is fatal. Each extra file is optional. The theme pointer is set before the two theme.txt reads, so a missing theme file still counts as installed and a later init skips those reads. A root lookup checks only the first archive. Any other lookup starts at the last archive and walks down. The first hit returns a copy of the lump. A missing name is fatal. A disk file that does not open returns an empty buffer. The notosans name matches with ASCII case folding and loads six fonts. The traditional Chinese line stays commented out. Any other name loads ui/font plus that name plus .ttf. Close deletes the archives and leaves the pointer set. The line edit top inset is 6.

| Managed (`VmWidgetResources1292Slice.cs`) | Native (`widgetresourcedata.cpp`) |
|-------------------------------------------|-----------------------------------|
| Open, search, and fonts | **1–158** |
| Band hull (exclusive slice) | **1–158** |

### Chunk **1292** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **158** |
| **Net headline** | **+158** vs post-**1291** |


**Post-review ratio:** 209/158 managed/native ≈ 132% (src file LOC at HEAD; see tally).

## Tests

- `VmWidgetResources1292SliceTests.Setup_SearchesFromTheEndAndKeepsTheFirstArchive`
- `VmWidgetResources1292SliceTests.Retry_SkipsThemeAfterAMissingFileAndLeavesThePointer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1292 --to 1292` (0 hard / 0 soft).

## Seams

Archive bytes, disk bytes, the system theme, and the extra file list are host inputs. Lump names match as stored. Theme colors are the landed theme slice.

## Credit rationale

UNCredited widgetresourcedata.cpp 1-158 (+158). A full tally line scan shows no overlap. The file ends at line 158.
