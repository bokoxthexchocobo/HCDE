# Chunk 1366 — The 7z stream, archive object, and F7ZFile::Open through the file count from file_7z.cpp (1-235).

## Scope

The 7z stream, archive object, and F7ZFile::Open through the file count from file_7z.cpp (1-235).

## Native ranges

- `src/common/filesystem/source/file_7z.cpp` **File7z** 1–235 (235 lines)

**Credited native LOC:** 235 (tally `nativeLoc` 235).

## Managed

`VmFile7z1366Slice.cs`
Managed LOC (tally): 239.
Conversion commit: `019e3b45002995f978ce07ab3fcb6db3e67d60c0`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Banner, includes, and the stream (**1–96**) | yes |
| Archive construct, destroy, open, and extract (**98–153**) | yes |
| `F7ZFile` construct and `Open` through the file count (**155–235**) | yes |
| Band hull (**1–235**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| file_7z.cpp **236–280** | recognition chunk 108, the directory loop and the first-file extract probe |
| file_7z.cpp **282–365** | hash, `Read`, `GetEntryReader`, and `Check7Z` |
| `SzArEx_*` and `CrcGenerateTable` | library bodies stay injected |

`Read` stores a negative count as size 0 and returns `SZ_ERROR_READ` (8). A zero count is success and still stores 0. A positive count, including one larger than the request, is stored and returns `SZ_OK`. An unknown seek origin returns 1 and does not seek or tell, so the position stays put. That 1 is the same number as `SZ_ERROR_DATA`, and the source returns the literal. Set, cur, and end are 0, 1, and 2. After a real seek, `Tell` replaces the position even when the seek result is an error. The returned code is the seek result truncated to 32 bits.

The archive constructor calls `CrcGenerateTable` only while `g_CrcTable[1]` is 0. A hook that leaves the cell at 0 runs again on the next archive. A nonzero cell skips the call. Then it seeks to 0 with `SeekSet`, builds the look vtable with lookahead 0, attaches the real stream, and sets look position and size to 0. The look buffer is the 16384-byte stream buffer. `BlockIndex` starts at `0xFFFFFFFF` and the out buffer is null.

`Extract` copies only when the result is `SZ_OK`. The copy is `OutBuffer` at the returned offset for the returned size. A null out buffer on that path throws `OutBuffer` and does not write the destination. Any other result leaves the destination unchanged.

Destroy frees a non-null out buffer and then the database. A throw from the out-buffer free does not free the database. A null out buffer skips that free.

`F7ZFile::Open` builds the archive, sets `skipped` to 0, then opens the database. Failure destroys the archive, clears it, and prints at error level 1. Unsupported uses the file name and `Decoder does not support this archive`. Memory and CRC use their fixed sentences and do not print the name. Every other code, including 1 and a negative value, prints `error #N`. A null name on the unsupported path throws after the archive is destroyed. Success allocates `NumFiles`, stores that count in `NumLumps`, and sets `j` to 0. It does not walk directories. A second open that fails does not destroy the archive the first call left behind.

| Managed (`VmFile7z1366Slice.cs`) | Native (`file_7z.cpp`) |
|----------------------------------|------------------------|
| Stream, archive, and open through the file count | **1–235** |
| Band hull (exclusive slice) | **1–235** |

### Chunk **1366** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **235** |
| **Net headline** | **+235** vs the prior chunk |


**Post-review ratio:** 239/235 managed/native ≈ 102% (src file LOC at HEAD; see tally).

## Tests

- `VmFile7z1366SliceTests.StreamReadSeekAndArchive`
- `VmFile7z1366SliceTests.OpenThroughFileCount`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1366 --to 1366` (0 hard / 0 soft).

## Seams

CrcGenerateTable, LookToRead2_CreateVTable, SzArEx_Open, SzArEx_Extract, and the alloc hooks stay injected. Recognition chunk 108 owns the directory loop at 236-280. Check7Z stays later in the file.

## Credit rationale

UNCredited file_7z.cpp 1-235 (+235). A full tally line scan, including recognition ranges, shows no overlap. Recognition chunk 108 owns 236-280, so the open prefix is 1-235 rather than a longer run. This is the largest remaining hole of at most 400 lines. It is not a tie. cmdlib.cpp 738-970 is 233. Gaps longer than 400 lines stay whole and are not sliced.
