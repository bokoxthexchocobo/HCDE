# Chunk 1200 — SuperFastHash and its case-insensitive twin from superfasthash.cpp (1–153).

## Scope

SuperFastHash and its case-insensitive twin from superfasthash.cpp (1–153).

## Native ranges

- `src/common/thirdparty/superfasthash.cpp` **SuperFastHash** 1–153 (153 lines)

**Credited native LOC:** 153 (tally `nativeLoc` 153).

## Managed

`VmSuperFastHash1200Slice.cs`
Managed LOC (tally): 80.
Conversion commit: `ad9fb552d5d109ea01803363761f433a680f2a7a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| get16bits (**39–48**, **100–102**) | yes |
| SuperFastHash (**50–96**) | yes |
| SuperFastHashI (**104–150**) | yes |
| Band hull (**1–153**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

Both hashes return 0 for a null pointer. The case-sensitive hash also returns 0 for length 0. The insensitive hash returns 0 for length 0 or below. The main loop consumes four bytes at a time: add the first pair, xor the second pair shifted left 11, xor the hash shifted left 16, then add the hash shifted right 11. A remainder of 3, 2, or 1 uses the matching tail. The final avalanche is the same six shifts. Insensitive loads and the one-byte tail fold A-Z to a-z before those steps.

| Managed (`VmSuperFastHash1200Slice.cs`) | Native (`superfasthash.cpp`) |
|------------------------------------------|------------------------------|
| SuperFastHash | **1–153** |
| Band hull (exclusive slice) | **1–153** |

### Chunk **1200** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **153** |
| **Net headline** | **+153** vs post-**1199** |


**Post-review ratio:** 80/153 managed/native ≈ 52% (src file LOC at HEAD; see tally).

## Tests

- `VmSuperFastHash1200SliceTests.Hash_MatchesNativeLengthsAndSignedTail`
- `VmSuperFastHash1200SliceTests.HashI_FoldsAsciiAndKeepsHighBytes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1200 --to 1200` (0 hard / 0 soft).

## Seams

Sixteen-bit loads use the portable little-endian byte pair, which is the path this build takes. A one-byte remainder is a signed char. The insensitive hash folds A-Z with C-locale tolower, including tolower of a signed -1 staying -1. A null pointer or a non-positive insensitive length returns 0.

## Credit rationale

UNCredited superfasthash.cpp 1–153 (+153). The file ends at 153. A full tally line scan shows no overlap.
