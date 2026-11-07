# Chunk 1399 — AlignPlane, SetSlopes, and CopySlopes from slopes.cpp (422-588).

## Scope

AlignPlane, SetSlopes, and CopySlopes from slopes.cpp (422-588).

## Native ranges

- `src/maploader/slopes.cpp` **MapLoader::AlignPlane** 422–588 (167 lines)

**Credited native LOC:** 167 (tally `nativeLoc` 167).

## Managed

`VmSlopes1399Slice.cs`
Managed LOC (tally): 198.
Conversion commit: `0a6e48e69920157003e123586237d19c40e55558`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| AlignPlane (**439–501**) | yes |
| SetSlopes (**509–539**) | yes |
| CopySlopes (**547–588**) | yes |
| Band hull (**422–588**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| CopyPlane | callee at lines 110-137 |
| GetPlaneTexZ | callee; the sector carries the texture heights |
| Lines before 422 | already outside this hole |

AlignPlane returns immediately when the line has no back sector, after reading the sector's first vertex. The reference vertex is the first sector vertex with a strictly greater absolute cross distance from the line. which 0 selects the floor and flips a downward normal. which 1 selects the ceiling and flips an upward normal. Any other which selects the ceiling and does not flip. SetSlopes clears Plane_Align (181) and uses args bits, including ceiling bits stored in args 0 when args 1 is 0. CopySlopes clears Plane_Copy (118), calls CopyPlane for each non-zero tag, then copies floor or ceiling planes across the line from args 4.

| Managed (`VmSlopes1399Slice.cs`) | Native (`slopes.cpp`) |
|----------------------------------|----------------------|
| Align, set, and copy | **422–588** |
| Band hull (exclusive slice) | **422–588** |

### Chunk **1399** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **167** |
| **Net headline** | **+167** vs the prior chunk |


**Post-review ratio:** 198/167 managed/native ≈ 119% (src file LOC at HEAD; see tally).

## Tests

- `VmSlopes1399SliceTests.AlignPlaneUsesTheFirstFurthestVertexAndFlipsCeilingNormals`
- `VmSlopes1399SliceTests.SetSlopesAndCopySlopesFollowArgsAndThenTheSideCopy`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1399 --to 1399` (0 hard / 0 soft).

## Seams

The slope setup above this band ends at line 421. CopyPlane is a callee at lines 110-137. The file ends at line 588.

## Credit rationale

UNCredited slopes.cpp 422-588 (+167). After chunk 1398 this is the unique largest hole. GlobalScopeStateNode.cpp 1-163 is 163. The band is shorter than 400 lines and is not sliced. It runs through end of file.
