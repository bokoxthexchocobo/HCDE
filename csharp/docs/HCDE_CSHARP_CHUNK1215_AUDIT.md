# Chunk 1215 — Splitter side classification from nodebuild_classify_nosse2.cpp (1-159).

## Scope

Splitter side classification from nodebuild_classify_nosse2.cpp (1-159).

## Native ranges

- `src/utility/nodebuilder/nodebuild_classify_nosse2.cpp` **FNodeBuilder::ClassifyLine** 1–159 (159 lines)

**Credited native LOC:** 159 (tally `nativeLoc` 159).

## Managed

`VmNodeClassify1215Slice.cs`
Managed LOC (tally): 106.
Conversion commit: `770ea1d16f906142e25abdbba1b69cb83a06658c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| ClassifyLine (**29-159**) | yes |
| Band hull (**1-159**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

Both endpoints far on one side return that side. Opposite far endpoints split. A near endpoint is on the line when its squared distance is below SIDE_EPSILON squared. Both endpoints on the line follow the splitter direction, and equal coordinates go to the back. One on-line end plus a back end is back. A zero-length splitter is back.

| Managed (`VmNodeClassify1215Slice.cs`) | Native (`nodebuild_classify_nosse2.cpp`) |
|------------------------------------------|-------------------------------------------|
| ClassifyLine | **1-159** |
| Band hull (exclusive slice) | **1-159** |

### Chunk **1215** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **159** |
| **Net headline** | **+159** vs post-**1214** |


**Post-review ratio:** 106/159 managed/native ≈ 67% (src file LOC at HEAD; see tally).

## Tests

- `VmNodeClassify1215SliceTests.Classify_SortsFarSidesAndTheFarBoundary`
- `VmNodeClassify1215SliceTests.Classify_UsesEpsilonAndSplitterDirectionWhenBothEndsAreOnTheLine`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1215 --to 1215` (0 hard / 0 soft).

## Seams

fixed_t coordinates widen to double. FAR_ENOUGH is the float 17179869184, which is exact. SIDE_EPSILON is 6.5536. A zero-length splitter produces an infinite scale and the sign test treats NaN as back.

## Credit rationale

UNCredited nodebuild_classify_nosse2.cpp 1-159 (+159). The file ends at 159. A full tally line scan shows no overlap.
