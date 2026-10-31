# Chunk 1322 — Minisegs, the splitter events, and the loop angles from nodebuild_gl.cpp (1-382).

## Scope

Minisegs, the splitter events, and the loop angles from nodebuild_gl.cpp (1-382).

## Native ranges

- `src/utility/nodebuilder/nodebuild_gl.cpp` **FNodeBuilder::AddIntersection** 1–382 (382 lines)

**Credited native LOC:** 382 (tally `nativeLoc` 382).

## Managed

`VmNodeGl1322Slice.cs`
Managed LOC (tally): 415.
Conversion commit: `4b416da5db4ee7de708f11e5e4429feac84558a4`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Intersection distance and the event insert (**39–61**) | yes |
| Forced split of a sharer and its partner (**68–153**) | yes |
| Front and back minisegs (**155–219**) | yes |
| Miniseg fields and the vertex heads (**221–268**) | yes |
| Loop start angle (**270–327**) | yes |
| Loop end angle (**329–382**) | yes |
| Band hull (**1–382**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| The empty Warn body | the host records the sector mismatch |
| SplitSeg in nodebuild.cpp | callee of the sharer walk, not a credited range |

An intersection stores the vertex only when that distance is new. A missing event, or a walk that runs out of successors, leaves the sharer unsplit. A forward walk steps to the next event and splits until the original end vertex. The new piece takes the old next pointer, and a partner is split and swapped the same way. A miniseg is added only when both sides have a loop start and a loop end. Its sidedef is no-side, its line is -1, and its plane comes from the splitter. The front and back sectors are the loop-start fronts. A mismatch that is not the back of either loop start is recorded. A seg that already joins the two vertices, a tighter outgoing angle, or a seg lying on the splitter is not the loop. The angle is the truncated atan2 bam shifted once.

| Managed (`VmNodeGl1322Slice.cs`) | Native (`nodebuild_gl.cpp`) |
|----------------------------------|-----------------------------|
| Intersections, sharers, minisegs, and loop angles | **1–382** |
| Band hull (exclusive slice) | **1–382** |

### Chunk **1322** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **382** |
| **Net headline** | **+382** vs post-**1321** |


**Post-review ratio:** 415/382 managed/native ≈ 109% (src file LOC at HEAD; see tally).

## Tests

- `VmNodeGl1322SliceTests.Intersection_InsertsOnceAndBuildsAMiniseg`
- `VmNodeGl1322SliceTests.Sharer_SplitsBothWaysAndClosesTheLoop`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1322 --to 1322` (0 hard / 0 soft).

## Seams

SplitSeg's vertex unlink lives in nodebuild.cpp. PointToAngle lives in nodebuild_utility.cpp. PointOnSide is the inline in nodebuild.h. The event tree is chunk 1218. This slice includes those callees so the sharer split and the miniseg loops match.

## Credit rationale

UNCredited nodebuild_gl.cpp 1-382 (+382). A full tally line scan shows no overlap. The file ends at line 382.
