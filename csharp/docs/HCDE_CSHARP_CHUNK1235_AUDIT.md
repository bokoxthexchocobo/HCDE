# Chunk 1235 — Quit, exit, logfile, directory and wad listing, md5sum, and printlocalized from c_enginecmds.cpp (1-303).

## Scope

Quit, exit, logfile, directory and wad listing, md5sum, and printlocalized from c_enginecmds.cpp (1-303).

## Native ranges

- `src/common/console/c_enginecmds.cpp` **execLogfile** 1–303 (303 lines)

**Credited native LOC:** 303 (tally `nativeLoc` 303).

## Managed

`VmEngineCmds1235Slice.cs`
Managed LOC (tally): 352.
Conversion commit: `87e2751abe0af4c6681b7d658b968c7d0fd73967`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Quit and exit (**52–62**) | yes |
| String-table print (**69–85**) | yes |
| Exec stop on the first miss (**87–100**) | yes |
| Logfile open and append bit (**102–130**) | yes |
| Error and fatal usage (**132–154**) | yes |
| Crashout fault, store not performed (**165–170**) | yes |
| Directory wildcard split (**173–213**) | yes |
| Wad directory (**223–243**) | yes |
| md5sum 8192-byte reads (**253–284**) | yes |
| printlocalized language id (**286–303**) | yes |
| Band hull (**1–303**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A third logfile argument turns append on even when the text is 0. A path with no wildcard scans that path with pattern star. Only a forward slash splits the path on this build. wdir with any argc other than 2 lists every lump. A two-byte language tag hits the string bounds check and does not fall through to GetString. crashout records the null store and returns.

| Managed (`VmEngineCmds1235Slice.cs`) | Native (`c_enginecmds.cpp`) |
|---------------------------------------|------------------------------|
| Commands through printlocalized | **1–303** |
| Band hull (exclusive slice) | **1–303** |

### Chunk **1235** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **303** |
| **Net headline** | **+303** vs post-**1234** |


**Post-review ratio:** 352/303 managed/native ≈ 116% (src file LOC at HEAD; see tally).

## Tests

- `VmEngineCmds1235SliceTests.Quit_PrintsAndStopsTheLog`
- `VmEngineCmds1235SliceTests.Dir_ListsWildcardsAndHashesFiles`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1235 --to 1235` (0 hard / 0 soft).

## Seams

Quit and exit call ConsoleEndoom and throw CExitEvent(0). print uses CheckString and reports an unknown name; a missing GetString returns the name when developer is 0. exec stops at the first file that fails. logfile closes an open log before opening the next one. The append flag is true whenever a third argument is present, including the text 0. fopen failure prints Could not start log. error and error_fatal use only argv[1]. crashout is compiled on this Linux build; the null store is logged as a fault and is not performed. dir uses NicePath for an argument and I_GetCWD otherwise. On this host a backslash is not a separator. A base name containing * or ? keeps that pattern and uses the directory portion; otherwise the pattern is * and the scan path is the whole path. Directories print at level 200 and files at level 2. wdir lists every lump unless argc is exactly 2, and a missing wad returns before the list. md5sum prints the usage and still walks the file arguments. Each file is read in 8192-byte chunks into the chunk 1209 MD5. printlocalized lowercases A-Z only. A tag of length 2 throws on index 2. A longer tag builds MAKE_ID from the first three bytes in little-endian order with a zero fourth byte. A missing language string is not passed to printf.

## Credit rationale

UNCredited c_enginecmds.cpp 1-303 (+303). The file ends at 303. A full tally line scan shows no overlap.
