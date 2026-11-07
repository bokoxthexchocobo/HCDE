# Chunk 1395 — VersionInfo assignment and FScanner open paths from sc_man.cpp (1-170).

## Scope

VersionInfo assignment and FScanner open paths from sc_man.cpp (1-170).

## Native ranges

- `src/common/engine/sc_man.cpp` **VersionInfo::operator=** 1–170 (170 lines)

**Credited native LOC:** 170 (tally `nativeLoc` 170).

## Managed

`VmScanner1395Slice.cs`
Managed LOC (tally): 198.
Conversion commit: `76711a2d62e8253584e6b4b77df8c221872385cf`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| VersionInfo assignment (**60–83**) | yes |
| FScanner symbol constructor (**91–94**) | yes |
| FScanner lump constructor (**102–106**) | yes |
| Open (**114–122**) | yes |
| OpenFile (**132–147**) | yes |
| OpenMem (**158–161**) | yes |
| Band hull (**1–170**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| OpenString | lines 171-178, already chunk 124 |
| OpenLumpNum | starts at line 188 |
| PrepareScript | starts at line 211 |
| Close | starts at line 261 |
| strtoull | callee; a failed conversion leaves the end pointer at the start |

VersionInfo stores major and minor as uint16 and revision as uint32. Each parsed field is clamped to 65535, cast through int16, then stored in that unsigned field. A trailing junk character sets major to 65535 and leaves the fields that were not parsed. Open looks up the full name and errors when the lump is missing. OpenFile closes, rejects a failed read or an empty buffer with a positive length, then copies the file length and prepares. OpenMem copies size characters and calls OpenString.

| Managed (`VmScanner1395Slice.cs`) | Native (`sc_man.cpp`) |
|------------------------------------|----------------------|
| Version assignment and scanner open | **1–170** |
| Band hull (exclusive slice) | **1–170** |

### Chunk **1395** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **170** |
| **Net headline** | **+170** vs the prior chunk |


**Post-review ratio:** 198/170 managed/native ≈ 116% (src file LOC at HEAD; see tally).

## Tests

- `VmScanner1395SliceTests.AssignVersionMatchesStrtoullThenTheUnsignedStore`
- `VmScanner1395SliceTests.OpenPathsCallTheLumpFileAndStringCallees`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1395 --to 1395` (0 hard / 0 soft).

## Seams

OpenString starts at line 171 and is already chunk 124. OpenLumpNum starts at line 188. PrepareScript starts at line 211. Close starts at line 261. strtoull and FString are callees.

## Credit rationale

UNCredited sc_man.cpp 1-170 (+170). After chunk 1394 the largest hole length is 170. a_quake.cpp 165-334 is also 170 and sorts later by path. The band is shorter than 400 lines and is not sliced.
