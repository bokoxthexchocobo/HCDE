# Chunk 1214 — Alpha-patch and palette IMGZ checksum lists from md5check.cpp (1-260).

## Scope

Alpha-patch and palette IMGZ checksum lists from md5check.cpp (1-260).

## Native ranges

- `src/common/textures/formats/md5check.cpp` **checkPatchForAlpha** 1–260 (260 lines)

**Credited native LOC:** 260 (tally `nativeLoc` 260).

## Managed

`VmMd5Check1214Slice.cs`
Managed LOC (tally): 261.
Conversion commit: `2e93f9abffcf898ea65302fb21c7d8962f9d46f8`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| alphapatches (**41-171**) | yes |
| palimgzs (**175-180**) | yes |
| makeMD5 (**188-199**) | yes |
| checkPatchForAlpha (**207-227**) | yes |
| checkIMGZPalette (**235-260**) | yes |
| Band hull (**1-260**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

Patches longer than 10164 bytes return false before a digest. A listed length is hashed once and compared with every row of that length. IMGZ seeks to the start and reads the reported length only after a listed length matches. The sentinel row is the end of each native array and is not a listed length.

| Managed (`VmMd5Check1214Slice.cs`) | Native (`md5check.cpp`) |
|------------------------------------|-------------------------|
| Checksum lists | **1-260** |
| Band hull (exclusive slice) | **1-260** |

### Chunk **1214** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **260** |
| **Net headline** | **+260** vs post-**1213** |


**Post-review ratio:** 261/260 managed/native ≈ 100% (src file LOC at HEAD; see tally).

## Tests

- `VmMd5Check1214SliceTests.MakeMd5_PrintsLowerHexAndTheAlphaListRejectsOtherBytes`
- `VmMd5Check1214SliceTests.CheckImgzPalette_HashesFromTheStartOnlyWhenTheLengthIsListed`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1214 --to 1214` (0 hard / 0 soft).

## Seams

MD5 is the little-endian digest from chunk 1209. IMGZ Seek and Read are the file reader. A short read leaves the unread tail zero. Digests are 32 lowercase hex characters.

## Credit rationale

UNCredited md5check.cpp 1-260 (+260). The file ends at 260. A full tally line scan shows no overlap.
