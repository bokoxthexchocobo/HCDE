# Chunk 1201 — Blood RFF probe, directory decrypt, and 8.3 names from file_rff.cpp (1–162).

## Scope

Blood RFF probe, directory decrypt, and 8.3 names from file_rff.cpp (1–162).

## Native ranges

- `src/common/filesystem/source/file_rff.cpp` **CheckRFF** 1–162 (162 lines)

**Credited native LOC:** 162 (tally `nativeLoc` 162).

## Managed

`VmFileRff1201Slice.cs`
Managed LOC (tally): 124.
Conversion commit: `ee8bb8e8e539ab98e303612df09a2db0e0e403ba`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| RFFInfo and RFFLump (**38–58**) | yes |
| BloodCrypt (**66–74**) | yes |
| OpenRFF (**83–134**) | yes |
| CheckRFF (**142–159**) | yes |
| Band hull (**1–162**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

CheckRFF accepts a file of at least 16 bytes whose first four bytes are RFF and 0x1A, then rewinds. OpenRFF reads the little-endian lump count and directory offset, loads that many 48-byte entries, and decrypts them with BloodCrypt using the directory offset as the key. Each entry copies position, size, and index. Flag bit 0x10 selects the RFF crypt method and the compressed flag. The other entries stay stored. The 8-byte name is terminated, then a dot and the three extension bytes are appended. NormalizeFileName receives that string. GenerateHash runs after the loop. OpenRFF returns success.

| Managed (`VmFileRff1201Slice.cs`) | Native (`file_rff.cpp`) |
|-----------------------------------|------------------------|
| RFF container | **1–162** |
| Band hull (exclusive slice) | **1–162** |

### Chunk **1201** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **162** |
| **Net headline** | **+162** vs post-**1200** |


**Post-review ratio:** 124/162 managed/native ≈ 77% (src file LOC at HEAD; see tally).

## Tests

- `VmFileRff1201SliceTests.BloodCrypt_XorsWithLowKeyAndHalfIndex`
- `VmFileRff1201SliceTests.Check_DecryptsDirectoryAndBuildsNames`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1201 --to 1201` (0 hard / 0 soft).

## Seams

NormalizeFileName is a host callback. The default keeps the assembled name. GenerateHash is counted once after the directory walk. LittleLong is the little-endian load used on this build. A short directory read fills the missing bytes with zero before BloodCrypt; the native buffer would be uninitialized.

## Credit rationale

UNCredited file_rff.cpp 1–162 (+162). The file ends at 162. A full tally line scan shows no overlap.
