# Chunk 1284 — Upper and lower edge splits, the open height inserts, and the vertex count from hw_walls_vertex.cpp (1-288).

## Scope

Upper and lower edge splits, the open height inserts, and the vertex count from hw_walls_vertex.cpp (1-288).

## Native ranges

- `src/rendering/hwrenderer/scene/hw_walls_vertex.cpp` **HWWall::SplitUpperEdge** 1–288 (288 lines)

**Credited native LOC:** 288 (tally `nativeLoc` 288).

## Managed

`VmWallVertex1284Slice.cs`
Managed LOC (tally): 272.
Conversion commit: `2458f51630c7e9a79e92e2f8efb6d2464710d2f2`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Upper edge (**31–62**) | yes |
| Lower edge (**70–101**) | yes |
| Left edge (**109–140**) | yes |
| Right edge (**148–179**) | yes |
| Create (**187–202**) | yes |
| Count edges (**212–271**) | yes |
| Make (**279–288**) | yes |
| Band hull (**1–288**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Ceiling slope | calculated, never stored |
| Floor slope | calculated, never stored |

The upper walk stops at the first fraction at or past the right edge and skips a fraction at or before the left edge. The lower walk runs backward, continues past a fraction at or past the right edge, and stops at a fraction at or before the left edge. A left height at or below the bottom is skipped, and a height at the top is not inserted. The right walk never considers slot zero, and a height at or above the top is skipped before the downward insert. A zero-height edge stores a zero texture factor. The upper split is suppressed only when the no-split-upper flag is set and the side has more than one segment. The lower split also requires the side to have more than one segment. The count adds one less than the segment count for each edge whose flag is clear, even when those segments fall outside the fraction range, and an empty side wraps that unsigned add. A second build leaves the stored index alone. Seamless off, a missing side, the polyobject flag, the no-split flag, and an explicit no-split argument all reserve four corners.

| Managed (`VmWallVertex1284Slice.cs`) | Native (`hw_walls_vertex.cpp`) |
|--------------------------------------|--------------------------------|
| Splits and the count | **1–288** |
| Band hull (exclusive slice) | **1–288** |

### Chunk **1284** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **288** |
| **Net headline** | **+288** vs post-**1283** |


**Post-review ratio:** 272/288 managed/native ≈ 94% (src file LOC at HEAD; see tally).

## Tests

- `VmWallVertex1284SliceTests.Create_InsertsTheOpenHeightsAndSkipsTheRightZeroSlot`
- `VmWallVertex1284SliceTests.Count_OverAllocatesOutsideSplitsAndWrapsAnEmptySide`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1284 --to 1284` (0 hard / 0 soft).

## Seams

The sidedef, segment fractions, and height lists are host records. The ceiling and floor slope factors are calculated and discarded.

## Credit rationale

UNCredited hw_walls_vertex.cpp 1-288 (+288). A full tally line scan shows no overlap. The file ends at line 288.
