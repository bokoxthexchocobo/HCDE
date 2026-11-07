# Chunk 1406 — copystring, FileExists, RecursiveFileExists, FileReadable, DirExists, and DirEntryExists from cmdlib.cpp (92-241).

## Scope

copystring, FileExists, RecursiveFileExists, FileReadable, DirExists, and DirEntryExists from cmdlib.cpp (92-241).

## Native ranges

- `src/common/utility/cmdlib.cpp` **copystring** 92–241 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmCmdlib1406Slice.cs`
Managed LOC (tally): 137.
Conversion commit: `229cce652139e62c8a9b831c818f3413cf7ecaab`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| copystring (**101–116**) | yes |
| FileExists (**135–140**) | yes |
| RecursiveFileExists (**151–172**) | yes |
| FileReadable (**182–190**) | yes |
| DirExists (**200–205**) | yes |
| DirEntryExists (**215–232**) | yes |
| Band hull (**92–241**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| FixPathSeperator | ends at line 91, already credited |
| GetFileInfo | starts at line 242, outside this hole |
| WideString, stat, _wstat64, access, _waccess, ScanDirectory | callees |

copystring of a null pointer allocates a one-byte empty string. Any other string is copied through the first zero and keeps that terminator. FileExists is a successful stat whose mode lacks S_IFDIR (0x4000). DirExists is a successful stat whose mode has that bit. A null or empty path returns false before stat and leaves the directory flag false. After stat, the directory flag is written from the mode even when stat failed. A null directory pointer skips that write. FileReadable compares access mode 4 (R_OK, and the Windows mode) with zero. The Windows branch passes the string through WideString first, and a null pointer becomes an empty wide string. The POSIX branch passes the original pointer. RecursiveFileExists checks path/file first. A miss scans the path with match "*", nosubdir false, and readhidden false. It returns the first non-hidden, non-system directory whose child file exists. The search does not call ScanDirectory again.

| Managed (`VmCmdlib1406Slice.cs`) | Native (`cmdlib.cpp`) |
|-----------------------------------|---------------------|
| String copy and path probes | **92–241** |
| Band hull (exclusive slice) | **92–241** |

### Chunk **1406** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **150** |
| **Net headline** | **+150** vs the prior chunk |


**Post-review ratio:** 137/150 managed/native ≈ 91% (src file LOC at HEAD; see tally).

## Tests

- `VmCmdlib1406SliceTests.CopyStringKeepsTheTerminatorAndProbesReadTheModeAfterStat`
- `VmCmdlib1406SliceTests.RecursiveSearchChecksTheBaseThenVisibleDirectories`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1406 --to 1406` (0 hard / 0 soft).

## Seams

WideString, stat, access, and ScanDirectory stay callees. GetFileInfo starts at line 242.

## Credit rationale

UNCredited cmdlib.cpp 92-241 (+150). After chunk 1405 this is the next whole-function hole of at least 150 lines. codegen.cpp 3778-3933 and p_user.cpp 2395-2544 stay uncredited because the whole functions left in each gap are under 150 lines. The band is shorter than 400 lines and is not sliced.
