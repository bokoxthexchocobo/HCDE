# Chunk 1213 — EMAPINFO, EDF, and ExtraData detection from hcde_eternity_compat.cpp (1-176).

## Scope

EMAPINFO, EDF, and ExtraData detection from hcde_eternity_compat.cpp (1-176).

## Native ranges

- `src/common/engine/hcde_eternity_compat.cpp` **HCDE_EternityCompat_DetectLoadedResources** 1–176 (176 lines)

**Credited native LOC:** 176 (tally `nativeLoc` 176).

## Managed

`VmEternityCompat1213Slice.cs`
Managed LOC (tally): 177.
Conversion commit: `c565dec396dd3f62a674993ac8973bb8383233dd`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| LowerCopy (**35-49**) | yes |
| BaseName (**51-55**) | yes |
| EndsWith (**57-61**) | yes |
| HasPathSegment (**63-67**) | yes |
| IsUserLoadedLump (**69-72**) | yes |
| RememberSample (**74-86**) | yes |
| RecordEMapInfo (**88-93**) | yes |
| RecordEdf (**95-100**) | yes |
| RecordExtraData (**102-107**) | yes |
| ReportDetectedResources (**109-128**) | yes |
| HCDE_EternityCompat_DetectLoadedResources (**131-166**) | yes |
| HCDE_EternityCompat_IsActive (**168-171**) | yes |
| HCDE_EternityCompat_GetFlags (**173-176**) | yes |
| Band hull (**1-176**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A user lump is one whose container is greater than the IWAD index. An empty full name falls back to the short name. The three family checks are separate, so one lump can raise more than one flag. Samples store the raw full path, ignore duplicates, and stop at eight. No flags means no report. IsActive(0) stays true.

| Managed (`VmEternityCompat1213Slice.cs`) | Native (`hcde_eternity_compat.cpp`) |
|------------------------------------------|--------------------------------------|
| Resource scan | **1-176** |
| Band hull (exclusive slice) | **1-176** |

### Chunk **1213** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **176** |
| **Net headline** | **+176** vs post-**1212** |


**Post-review ratio:** 177/176 managed/native ≈ 101% (src file LOC at HEAD; see tally).

## Tests

- `VmEternityCompat1213SliceTests.Detect_ClassifiesEmapinfoEdfAndExtradata`
- `VmEternityCompat1213SliceTests.Detect_CapsSamplesAndSkipsIwadLumps`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1213 --to 1213` (0 hard / 0 soft).

## Seams

GetFileContainer, GetMaxIwadNum, GetFileShortName, GetFileFullName, and GetFileFullPath are the lump list. Printf is the report callback. ASCII A-Z folds to lower case after backslashes become slashes, matching the C locale.

## Credit rationale

UNCredited hcde_eternity_compat.cpp 1-176 (+176). The file ends at 176. A full tally line scan shows no overlap.
