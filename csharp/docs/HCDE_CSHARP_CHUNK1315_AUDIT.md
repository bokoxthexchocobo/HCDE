# Chunk 1315 — CPUID leaves, the brand-string spaces, and the feature dump from x86.cpp (1-218).

## Scope

CPUID leaves, the brand-string spaces, and the feature dump from x86.cpp (1-218).

## Native ranges

- `src/common/utility/x86.cpp` **CheckCPUID** 1–218 (218 lines)

**Credited native LOC:** 218 (tally `nativeLoc` 218).

## Managed

`VmCpuId1315Slice.cs`
Managed LOC (tally): 265.
Conversion commit: `45d2f70c902f47561fc79143053e18a7d7e9fff4`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Leaf walk and vendor id (**32–152**) | yes |
| Brand spaces and the feature dump (**154–216**) | yes |
| Band hull (**1–218**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| The CPUID instruction | the host returns the four registers |

The non-x86 path clears the record and sets the line size to 64. The x86 path does that too, then reads leaf 0, leaf 1, and the extended maximum as an unsigned value. AuthenticAMD is ebx, edx, ecx in that order. Family 15 adds the extended family before the model check, so a non-zero extended family skips the extended model. Family 6 still adds it. AMD applies the extended family and the extended model in the same check. CLFLUSH replaces the line size with ebx bits 15:8 shifted by 5, and a later AMD L1 leaf overwrites that whole word. Leaf 7 is read only when the signed maximum id is at least 7. The brand walk skips leading spaces and collapses a space that follows a space. An empty vendor byte returns an empty dump after that walk. The brief flag does not change the text. Feature names stay in the header bit order, and a vendor id with no trailing zero continues into the brand bytes.

| Managed (`VmCpuId1315Slice.cs`) | Native (`x86.cpp`) |
|----------------------------------|--------------------|
| CPUID leaves and the feature dump | **1–218** |
| Band hull (exclusive slice) | **1–218** |

### Chunk **1315** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **218** |
| **Net headline** | **+218** vs post-**1314** |


**Post-review ratio:** 265/218 managed/native ≈ 122% (src file LOC at HEAD; see tally).

## Tests

- `VmCpuId1315SliceTests.Check_DecodesIntelLeavesAndKeepsFamily15ModelFlat`
- `VmCpuId1315SliceTests.Dump_CondensesTheBrandAndPrintsAmdFamily`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1315 --to 1315` (0 hard / 0 soft).

## Seams

CPUID is a host leaf callback. The non-x86 path is the other half of the same file.

## Credit rationale

UNCredited x86.cpp 1-218 (+218). A full tally line scan shows no overlap. The file ends at line 218.
