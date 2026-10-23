# Chunk 880 — Deferred (2026-10-23): `p_linkedsectors.cpp` remainder after **174** is non-contiguous and no single band ≥150 lines; renaming tally row to bypass `--band` was rejected.

## Scope

Deferred (2026-10-23): `p_linkedsectors.cpp` remainder after **174** is non-contiguous and no single band ≥150 lines; renaming tally row to bypass `--band` was rejected.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `289b24a4748aa842d15bd69c2ef3a8aee40b8062`.

## Padding review (2026-10-21)

### vs chunk **174** (no double count)

| | Lines |
|--|------:|
| **174** credited (`linked-sectors-move-chunk174`) | **149** |
| **880** gross remainder (union of uncredited bands below) | **225** |
| **880** net matrix credit | **0** |
| TU lines in `p_linkedsectors.cpp` (approx.) | **382** |
| Check: 149 + 225 − 7 (VM glue **79–85** counted in remainder only) ≈ 367 + separators | (remainder overlaps **174** only at boundaries, not interior) |

**174 credited symbols (unchanged):** `IsLinked` **67–77**, `P_MoveLinkedSectors` **133–200**, `AddSingleSector` **237–265**, `P_AddSectorLinks` **297–337**.

**880 deferred bands (function boundaries, all UNCredited vs tally):**

| Band | Lines | Native |
|------|------:|--------|
| **1–66** | 66 | headers + `LINK_*` enums |
| **78–132** | 55 | VM `isLinked`, `MoveCeiling`, `MoveFloor` |
| **201–226** | 26 | `P_StartLinkedSectorInterpolations` |
| **266–296** | 31 | `RemoveTaggedSectors` + separators |
| **338–384** | 47 | `P_AddSectorLinksByID` |

### Behavior ported (matrix credit)

| Native | Behavior ported |
|--------|-----------------|
| (none) | **no** (deferred) |

### Gross vs net

| | Value |
|--|------:|
| Gross remainder surveyed | **225** |
| Net behavior-ported | **0** |

### Test filter note (root cause)

`dotnet test HCDE.sln --filter FullyQualifiedName~VmLinkedSectorTests` builds every test project; **13/14** assemblies report *No test matches the given testcase filter* because the filter only applies to `HCDE.Playsim.Tests`. That is expected VSTest behavior, not a failed run. Playsim reports **10** passed. Use `dotnet test tests/HCDE.Playsim.Tests/HCDE.Playsim.Tests.csproj --filter …` for a single-project run without noise.

## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 880 --to 880` (0 hard / 0 soft).

## Seams

Implementation landed off-tally: `VmLinkedSectorPlaneMove`, `VmLinkedSectorInterpolation`, `AddSectorLinksById`, and `MoveLinkedSectors` direct `AuthoritySimulation` calls (no movement delegates). Matrix credit **0** until an honest ≥150-line contiguous uncredited band exists.

## Credit rationale

Chunk **174** already credits **149** native lines in four bands. Remaining TU slices are separated by credited lines; `land_chunk_fast.sh --band` uses min–max envelope so a multi-gap chunk cannot pass without gaming. Largest contiguous remainder is **66** lines (**1–66**); next is **55** (**78–132**).
