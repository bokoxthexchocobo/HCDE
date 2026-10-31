# Chunk 1325 — The central directory, the compression methods, and the local header skip from file_zip.cpp (1-388).

## Scope

The central directory, the compression methods, and the local header skip from file_zip.cpp (1-388).

## Native ranges

- `src/common/filesystem/source/file_zip.cpp` **FZipFile::Open** 1–388 (388 lines)

**Credited native LOC:** 388 (tally `nativeLoc` 388).

## Managed

`VmZipFile1325Slice.cs`
Managed LOC (tally): 573.
Conversion commit: `6fe5874cea01bf380318c3f03733baf303fad18f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| End-of-directory scan (**47–91**) | yes |
| Open, multipart reject, and the directory load (**120–213**) | yes |
| Names, methods, encryption, and zip64 sizes (**215–305**) | yes |
| Raw read, local-header skip, and CheckZip (**321–388**) | yes |
| Band hull (**1–388**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| The unused prefix scan locals | they are written and never read |
| Hash buckets and lump filter | callees in resourcefile.cpp |

The scan walks backward from the end in 1024-byte steps, at most 65535 bytes, and the Quake loop visits size minus 4 down through 0. PK 5 6 records the first end record. PK 6 6 wins immediately and marks zip64. A directory at offset 0 is corrupt. Multipart disk numbers fail. Entries that end in `/` with a zero 32-bit size are skipped, as are unknown methods and the encrypted flag. A zip64 extra block of type 1 and at least 24 data bytes replaces the sizes and the local offset. A size above `0x7fffffff` warns, counts a skip, and still keeps the entry, so a later entry can fall off `NumLumps`. Implode flags 0, 2, 4, and 6 become methods 1000, 1002, 1004, and 1006. Names are lowercased, backslashes become slashes, and invalid UTF-8 bytes at or above 0x80 become `@`. GetRawData of an empty or out-of-range entry returns a stored buffer with no bytes. Otherwise it skips the local name and extra, clears the need-start flag, and reads the compressed bytes. CheckZip requires `PK\x03\x04` and destroys the archive when open fails.

| Managed (`VmZipFile1325Slice.cs`) | Native (`file_zip.cpp`) |
|------------------------------------|-------------------------|
| Directory scan, open, and the raw read | **1–388** |
| Band hull (exclusive slice) | **1–388** |

### Chunk **1325** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **388** |
| **Net headline** | **+388** vs post-**1324** |


**Post-review ratio:** 573/388 managed/native ≈ 148% (src file LOC at HEAD; see tally).

## Tests

- `VmZipFile1325SliceTests.Directory_SkipsFoldersAndReadsPastTheLocalHeader`
- `VmZipFile1325SliceTests.Zip64_AppliesSizesAndKeepsAnOversizeSlot`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1325 --to 1325` (0 hard / 0 soft).

## Seams

LittleShort and LittleLong are identity on this little-endian host. NormalizeFileName lowercases and rewrites slashes in resourcefile.cpp. GenerateHash and PostProcessArchive run after a successful open. This slice keeps those calls and the directory walk that feeds them.

## Credit rationale

UNCredited file_zip.cpp 1-388 (+388). A full tally line scan shows no overlap. The file ends at line 388.
