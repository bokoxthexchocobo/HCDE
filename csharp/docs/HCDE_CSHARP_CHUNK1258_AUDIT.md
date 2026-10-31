# Chunk 1258 — Reserved flat vertices, pipeline upload, and the vertex-cursor cap from flatvertices.cpp (1-170).

## Scope

Reserved flat vertices, pipeline upload, and the vertex-cursor cap from flatvertices.cpp (1-170).

## Native ranges

- `src/common/rendering/hwrenderer/data/flatvertices.cpp` **FFlatVertexBuffer** 1–170 (170 lines)

**Credited native LOC:** 170 (tally `nativeLoc` 170).

## Managed

`VmFlatVertices1258Slice.cs`
Managed LOC (tally): 215.
Conversion commit: `f0dd51ff8164aeecbb3258d180f84b115f600dfc`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| FFlatVertexBuffer (**37–97**) | yes |
| ~FFlatVertexBuffer (**105–115**) | yes |
| OutputResized (**123–130**) | yes |
| AllocVertices (**138–148**) | yes |
| Copy (**156–170**) | yes |
| Band hull (**1–170**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| GPU buffer create and delete | host steps |

The constructor fills twenty shadow vertices: a uniform quad, a fullscreen quad, a postprocess quad, and two stencil caps. The five-argument write stores -1 as the lightmap index and leaves the lightmap coordinates unchanged. The index buffer is given four bytes and static usage. Each pipeline buffer is sized to two million vertices of 32 bytes and given three attributes: position at offset 0, texture at offset 12, and lightmap at offset 20. The cursor and the reserved count start at 20, then the reserved run is copied. A copy maps each pipeline, writes the leading shadow vertices at the start index, unmaps a zero-length range because the cursor did not move, and uploads the copied byte range. The current pipeline is restored afterward. A resize rewrites the fullscreen quad and copies four vertices at index 4, which uploads the leading quad rather than the fullscreen quad. Allocating returns the cursor from before the add. The add is kept when the sum reaches the cap, and no vertex is returned.

| Managed (`VmFlatVertices1258Slice.cs`) | Native (`flatvertices.cpp`) |
|----------------------------------------|---------------------------|
| Reserved vertices, copy, and the cap | **1–170** |
| Band hull (exclusive slice) | **1–170** |

### Chunk **1258** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **170** |
| **Net headline** | **+170** vs post-**1257** |


**Post-review ratio:** 215/170 managed/native ≈ 126% (src file LOC at HEAD; see tally).

## Tests

- `VmFlatVertices1258SliceTests.Build_FillsTheReservedQuadsAndUploadsBothPipelines`
- `VmFlatVertices1258SliceTests.Resize_UploadsTheLeadingQuadAndAllocStopsAtTheCap`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1258 --to 1258` (0 hard / 0 soft).

## Seams

Creating and deleting the GPU buffers is a host step. The shadow stores twenty vertices. A copy reads that shadow from its first vertex and writes the run at the requested index on every pipeline. The cursor add happens before the cap test. The cap is 1999500.

## Credit rationale

UNCredited flatvertices.cpp 1-170 (+170). A full tally line scan shows no overlap. The file ends at line 170.
