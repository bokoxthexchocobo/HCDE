# Chunk 1227 — Directory lump scan, backup-name skip, and the cached entry reader from file_directory.cpp (1-194).

## Scope

Directory lump scan, backup-name skip, and the cached entry reader from file_directory.cpp (1-194).

## Native ranges

- `src/common/filesystem/source/file_directory.cpp` **FDirectory::AddDirectory** 1–194 (194 lines)

**Credited native LOC:** 194 (tally `nativeLoc` 194).

## Managed

`VmDirectory1227Slice.cs`
Managed LOC (tally): 271.
Conversion commit: `e16f66850d84d314bd9577e7ef0f645b3f2979fe`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Full path and trailing slash (**65–71**) | yes |
| Directory scan and backup skip (**79–141**) | yes |
| Open always succeeds (**149–155**) | yes |
| Cached reader join (**163–178**) | yes |
| CheckDir keeps the object (**186–192**) | yes |
| Band hull (**1–194**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A backup leaf is skipped after the fold, while a folder named `.orig` still contributes its file. The base path stays on a failed rescan. The cached reader uses the position from before the name sort.

| Managed (`VmDirectory1227Slice.cs`) | Native (`file_directory.cpp`) |
|-------------------------------------|-------------------------------|
| Scan, backup skip, and cached reader | **1–194** |
| Band hull (exclusive slice) | **1–194** |

### Chunk **1227** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **194** |
| **Net headline** | **+194** vs post-**1226** |


**Post-review ratio:** 271/194 managed/native ≈ 140% (src file LOC at HEAD; see tally).

## Tests

- `VmDirectory1227SliceTests.Add_SkipsBackupsAndKeepsTheRelativePath`
- `VmDirectory1227SliceTests.Read_JoinsTheBaseAfterTheNameSort`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1227 --to 1227` (0 hard / 0 soft).

## Seams

The stored directory name is the full path with a trailing slash. nosubdir is kept and is not passed into the scan. The base path is the prefix of the first scanned entry, taken from the last match of the relative path, and a miss keeps the whole path. Directories set that base and are not added. Backup names are skipped after a C-locale fold when the lowered leaf contains .orig, .bak, or .cache. The filter sees that lowered leaf and the original full path. A null filter or a null callback accepts the file. Files longer than 0x7fffffff are ignored. The relative path is stored unchanged while the lump name is the normalized relative path. Flags are full path, the resource id is -1, the method is stored, the namespace is global, and the position is the accepted index. Open still returns true when the scan fails, and a failed rescan does not drop the entries or the base already stored. GetEntryReader joins the base to the relative path selected by that position. Only reader type 2 consumes the file bytes. An entry outside the accepted count returns an empty reader. The third reader argument is ignored.

## Credit rationale

UNCredited file_directory.cpp 1-194 (+194). The file ends at 194. A full tally line scan shows no overlap.
