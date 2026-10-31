# Chunk 1212 — DOS zip time and the local, central, and end-of-directory headers from writezip.cpp (1-230).

## Scope

DOS zip time and the local, central, and end-of-directory headers from writezip.cpp (1-230).

## Native ranges

- `src/common/utility/writezip.cpp` **WriteZip** 1–230 (230 lines)

**Credited native LOC:** 230 (tally `nativeLoc` 230).

## Managed

`VmWriteZip1212Slice.cs`
Managed LOC (tally): 239.
Conversion commit: `ebf4320ff592ef642699721a57823cacef32fcb8`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| time_to_dos (**46-59**) | yes |
| AppendToZip (**73-116**) | yes |
| AppendCentralDirectory (**128-172**) | yes |
| WriteZip (**174-230**) | yes |
| Band hull (**1-230**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A null calendar or a year before 1980 stores a zero DOS stamp. Otherwise the time is hour*2048 + minute*32 + second/2 and the date is (year-80)*512 + (month+1)*32 + day. Implode methods 1000 through 1006 become method 6 with the low flags. Deflate stores method 8 and flags 2. Methods at or above 1337 fail the entry. The local header writes the DOS time into ModDate and the DOS date into ModTime. The central header writes them the other way around. A short write or a rejected method deletes the file. A failed open returns false and does not delete. The end record counts the entries and sizes only the central directory.

| Managed (`VmWriteZip1212Slice.cs`) | Native (`writezip.cpp`) |
|------------------------------------|------------------------|
| Zip writer | **1-230** |
| Band hull (exclusive slice) | **1-230** |

### Chunk **1212** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **230** |
| **Net headline** | **+230** vs post-**1211** |


**Post-review ratio:** 239/230 managed/native ≈ 104% (src file LOC at HEAD; see tally).

## Tests

- `VmWriteZip1212SliceTests.Write_StoresLocalCentralAndEndHeaders`
- `VmWriteZip1212SliceTests.Write_RemovesTheFileWhenAHeaderIsRejected`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1212 --to 1212` (0 hard / 0 soft).

## Seams

FileWriter::Open, Tell, Write, and RemoveFile are host callbacks. time and localtime are the injected calendar. This host is little endian, so LittleShort and LittleLong do not swap. Header sizes are the packed 30, 46, and 22 byte structs. Names are raw bytes.

## Credit rationale

UNCredited writezip.cpp 1-230 (+230). The file ends at 230. A full tally line scan shows no overlap.
