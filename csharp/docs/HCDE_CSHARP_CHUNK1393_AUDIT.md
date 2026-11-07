# Chunk 1393 — FormatGUID, myasctime, CreatePath, RemoveFile, and RemoveDir from cmdlib.cpp (467-645).

## Scope

FormatGUID, myasctime, CreatePath, RemoveFile, and RemoveDir from cmdlib.cpp (467-645).

## Native ranges

- `src/common/utility/cmdlib.cpp` **FormatGUID** 467–645 (179 lines)

**Credited native LOC:** 179 (tally `nativeLoc` 179).

## Managed

`VmCmdlib1393Slice.cs`
Managed LOC (tally): 180.
Conversion commit: `af20d1207d4d10eb44db3e5e1d5837831908cba4`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| FormatGUID (**468–484**) | yes |
| myasctime (**486–511**) | yes |
| CreatePath, both platform branches (**513–615**) | yes |
| RemoveFile and RemoveDir (**617–635**) | yes |
| strbin banner before the function (**637–645**) | yes |
| Band hull (**467–645**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| strbin | starts at line 646, outside this hole |
| DirEntryExists and mkdir | callees; the POSIX branch passes mode 0755 (493) |
| remove and rmdir | callees; the Windows branch records a wide path |
| time and localtime | callees; a null local time returns Unknown with a newline |

FormatGUID writes lowercase hex into a buffer of the given size and keeps one byte for the terminator. myasctime stores `%F %T` in the static buffer, or `Unknown\n` when local time is missing. The POSIX CreatePath walks each slash, creates a missing entry, and stops when mkdir returns -1. A path of `/` returns immediately. The Windows CreatePath appends a slash when the path does not already end in a separator, strips trailing separators, and creates each parent. The inner Windows `_mkdir` branch is compiled out. RemoveFile ignores the syscall result. RemoveDir returns it.

| Managed (`VmCmdlib1393Slice.cs`) | Native (`cmdlib.cpp`) |
|----------------------------------|------------------------|
| GUID, timestamp, path create, and removal | **467–645** |
| Band hull (exclusive slice) | **467–645** |

### Chunk **1393** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **179** |
| **Net headline** | **+179** vs the prior chunk |


**Post-review ratio:** 180/179 managed/native ≈ 101% (src file LOC at HEAD; see tally).

## Tests

- `VmCmdlib1393SliceTests.FormatGuidWritesLowerHexAndAscTimeUsesTheStaticBuffer`
- `VmCmdlib1393SliceTests.CreatePathBuildsParentsAndRemoveUsesThePlatformCall`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1393 --to 1393` (0 hard / 0 soft).

## Seams

strbin starts at line 646 and stays outside this band. DirEntryExists, mkdir, remove, and rmdir are callees. time and localtime are callees.

## Credit rationale

UNCredited cmdlib.cpp 467-645 (+179). After chunk 1392 this is the unique largest hole. i_net.cpp 1639-1811 is 173. The band is shorter than 400 lines and is not sliced.
