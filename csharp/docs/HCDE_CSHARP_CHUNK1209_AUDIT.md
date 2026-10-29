# Chunk 1209 — MD5 init, update, final, and the transform from md5.cpp (1–236).

## Scope

MD5 init, update, final, and the transform from md5.cpp (1–236).

## Native ranges

- `src/common/thirdparty/md5.cpp` **MD5Context::Update** 1–236 (236 lines)

**Credited native LOC:** 236 (tally `nativeLoc` 236).

## Managed

`VmMd51209Slice.cs`
Managed LOC (tally): 227.
Conversion commit: `16b33ec7ad2b2a7095a7afa2e296e5ea28727fd5`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| byteSwap (**24–36**) | yes |
| MD5Context::Init (**42–51**) | yes |
| MD5Context::Update (**57–94**) | yes |
| MD5Context::Final (**100–130**) | yes |
| F1 through F4 and MD5STEP (**136–144**) | yes |
| MD5Transform (**151–233**) | yes |
| Band hull (**1–236**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

Init loads the four MD5 constants and clears the byte count. Update adds the length with a carry into the high word, fills the open 64-byte block, and transforms every full block. Final appends the 0x80 pad, an extra block when the pad does not fit, and the bit length, then writes the digest in little-endian bytes and zeroes the context. A later Final without Init therefore does not repeat the empty digest. A null buffer with a positive length is the native read. A zero length updates nothing.

| Managed (`VmMd51209Slice.cs`) | Native (`md5.cpp`) |
|-------------------------------|--------------------|
| MD5 | **1–236** |
| Band hull (exclusive slice) | **1–236** |

### Chunk **1209** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **236** |
| **Net headline** | **+236** vs post-**1208** |


**Post-review ratio:** 227/236 managed/native ≈ 96% (src file LOC at HEAD; see tally).

## Tests

- `VmMd51209SliceTests.Final_MatchesTheRfcVectorsAndClearsTheContext`
- `VmMd51209SliceTests.Update_ConcatenatesChunksAndAZeroLengthCall`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1209 --to 1209` (0 hard / 0 soft).

## Seams

byteSwap is compiled out on this little-endian host. Words are stored little endian. Final wipes the context.

## Credit rationale

UNCredited md5.cpp 1–236 (+236). The file ends at 236. A full tally line scan shows no overlap.
