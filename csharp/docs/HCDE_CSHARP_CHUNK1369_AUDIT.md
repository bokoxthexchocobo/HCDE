# Chunk 1369 — D_AddWildFile, D_AddConfigFiles, D_AddDirectory, BaseFileSearch, and D_FileNotFound from findfile.cpp (165-391).

## Scope

D_AddWildFile, D_AddConfigFiles, D_AddDirectory, BaseFileSearch, and D_FileNotFound from findfile.cpp (165-391).

## Native ranges

- `src/common/utility/findfile.cpp` **FindFileSearch** 165–391 (227 lines)

**Credited native LOC:** 227 (tally `nativeLoc` 227).

## Managed

`VmFindFile1369Slice.cs`
Managed LOC (tally): 338.
Conversion commit: `db9af0527226e86a49091abc6903ebfb42696532`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Blank line after the wildcard filter (**165**) | yes |
| `D_AddWildFile` (**166–209**) | yes |
| `D_AddConfigFiles` banner and body (**211–239**) | yes |
| `D_AddDirectory` banner and body (**241–264**) | yes |
| `BaseFileSearch` banner, static buffer, and body (**266–369**) | yes |
| `D_FileNotFound` banner and body (**371–391**) | yes |
| Band hull (**165–391**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| findfile.cpp **119–132** and **141–164** | already chunk 137 |
| findfile.cpp **55–117** | `D_AddFile` stat and directory walk, outside this band |
| `ScanDirectory`, `DirEntryExists`, `NicePath`, `ExpandEnvVars`, `RecursiveFileExists` | helpers outside this band stay injected |

A null or empty wild value returns without a search. A `BaseFileSearch` hit adds that path and does not scan. A miss extracts the directory and base, runs the chunk 137 wildcard filter, and scans `'.'` when the directory is empty. Every scan hit is added, including directories. An empty scan reports `optional wad` or `wad`.

`D_AddConfigFiles` expands only `Path` values and restores the config cursor around each wild add, because the search resets the section to `FileSearch.Directories`. A null config or a missing section does nothing.

`D_AddDirectory` uses `*.wad` when the pattern is null or empty, passes `nosubdir`, and skips directories.

`BaseFileSearch` returns null for a null or empty name. A non-empty program directory is tried first and a trailing slash is not doubled. An empty program directory throws before the existence check. A direct hit is returned as that same string. Config keys are `Path`, `RecursivePath`, and `PathList` in file order, compared without case. `Path` and `PathList` join a non-empty `NicePath` with one slash. `RecursivePath` strips one trailing slash, including a directory that is only `/`, then returns a non-empty recursive hit without another existence check. `PathList` expands once and drops empty `:` tokens. A non-null extension is appended only when the last component has no dot, then the search runs again with a null extension. `*.wad` is appended whole, so `doom` becomes `doom*.wad`.

`D_FileNotFound` always prints `type not found: file`. It calls `I_FatalError` only when `i_exit_on_not_found` contains the requirement bit. The suggested cvar is the current value minus that bit.

| Managed (`VmFindFile1369Slice.cs`) | Native (`findfile.cpp`) |
|------------------------------------|------------------------|
| Wild add, config files, directory, search, and missing-file report | **165–391** |
| Band hull (exclusive slice) | **165–391** |

### Chunk **1369** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **227** |
| **Net headline** | **+227** vs the prior chunk |


**Post-review ratio:** 338/227 managed/native ≈ 149% (src file LOC at HEAD; see tally).

## Tests

- `VmFindFile1369SliceTests.WildConfigAndMissing`
- `VmFindFile1369SliceTests.SearchAndDirectory`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1369 --to 1369` (0 hard / 0 soft).

## Seams

D_AddFile, ScanDirectory, DirEntryExists, NicePath, ExpandEnvVars, and RecursiveFileExists stay injected. The normalize/dedupe and wildcard filter at 119-164 stay chunk 137. The stat walk in D_AddFile stays outside this band.

## Credit rationale

UNCredited findfile.cpp 165-391 (+227). A full tally line scan, including recognition ranges, shows no overlap. This is the largest remaining hole of at most 400 lines after m_png.cpp 655-886. It is not a tie. Gaps longer than 400 lines stay whole and are not sliced.
