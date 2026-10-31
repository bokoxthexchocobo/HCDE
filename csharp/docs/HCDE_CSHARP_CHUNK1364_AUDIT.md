# Chunk 1364 — Missing segs, the GL-node probe, and LoadGLVertexes from glnodes.cpp (1-240).

## Scope

Missing segs, the GL-node probe, and LoadGLVertexes from glnodes.cpp (1-240).

## Native ranges

- `src/maploader/glnodes.cpp` **GlNodes** 1–240 (240 lines)

**Credited native LOC:** 240 (tally `nativeLoc` 240).

## Managed

`VmGlNodes1364Slice.cs`
Managed LOC (tally): 241.
Conversion commit: `fb553f48b3d5f67c91f510b55ea12b3db7ff691f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Banner (**1–23**) | yes |
| Includes (**24–47**) | yes |
| `gl_cachenodes` and `gl_cachetime` declarations (**49–50**) | yes |
| GL lump structs (**52–90**) | yes |
| `CheckForMissingSegs` (**101–126**) | yes |
| `CheckForGLNodes` (**134–168**) | yes |
| Magic macros (**179–183**) | yes |
| `LoadGLVertexes` (**185–233**) | yes |
| Utility comment and the blank line before `checkGLVertex` (**235–240**) | yes |
| Band hull (**1–240**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| glnodes.cpp **241–246** | chunk 888 `checkGLVertex` |
| glnodes.cpp **248–415** | chunk 888, vertex widening and `LoadGLSegs` |
| glnodes.cpp **424–643** | chunk 889 |
| glnodes.cpp **644–1195** | 552 lines, above the 400-line ceiling, not sliced |
| glnodes.cpp **1196–1396** | chunk 890, with the small gaps between its functions left in place |
| `VmMapLoaderLoadGlSegs.cs` | already credited; not edited |

`CheckForMissingSegs` sums each seg's vertex length onto `Index(sidedef)`. A null sidedef is skipped, including when its vertices are null. A sidedef that is not in the side array throws `sides`. A null vertex on a hosted seg throws `v1` or `v2`. Each side then compares that sum with `linedef->Delta().Length() - 1`. The compare is strict, so a sum equal to the delta length minus one is not missing. `Delta()` is the stored delta, not the distance between the line's vertices. A null linedef throws `linedef`. The function does not print.

`CheckForGLNodes` rejects `numlines < 1` with `numlines` before it reads a seg. A window that starts before the seg array or runs past it throws `segs`. Closure is pointer identity of `firstseg->v1` and `lastseg->v2`. Two vertices with the same coordinates are open. Two nulls are closed. An open subsector returns false immediately, so a miniseg later in that window is not consulted. A closed subsector scans `Level->segs[j]` for `j` in `0 .. numlines-1`, not the subsector's own seg window. A null linedef there returns true immediately. When every subsector is closed and that prefix has no miniseg, the missing-seg count decides. A positive count prints `N missing segs counted` and `The BSP needs to be rebuilt.` and returns false. Zero returns true and prints nothing. An empty subsector list falls through to that count.

`LoadGLVertexes` stores `firstglvertex` before it looks at the lump. It seeks to `SeekSet`, reads, then asks for the length, including when the length is below 4. A length below 4 returns false and leaves `format5` unchanged. A length of at least 4 whose buffer is shorter than 4 bytes throws `gldata` before `format5` changes. The magic is the first four file bytes as a little-endian int, matching `MAKE_ID` for `gNd2`, `gNd4`, and `gNd5`. `gNd5` sets `format5`. `gNd2` clears it. `gNd4` prints version 4. Every other magic prints version 1. The text is `GL nodes vN found. This format is not supported by HCDE`. Neither failure resizes the vertex array.

The vertex count is `firstglvertex + (length - 4) / 8`, integer division. Bytes past that multiple are ignored. A count that does not fit in a 32-bit list index throws `numvertexes` after `format5` is stored and before the array grows. An empty vertex array with any line throws `vertexes` at the old base pointer, before the resize, so a valid vertex payload is not appended. Otherwise the array grows first. Each line's vertices are then retargeted by their old index. A null endpoint throws `v1` or `v2`. An endpoint that was not in the old array throws `vertexes`. Those throws happen after the growth, and coordinates for the new slots are still unset. Each new vertex is `LittleLong(x) / 65536.0` and `LittleLong(y) / 65536.0`, read little-endian, so a big-endian 65536 lands as 256/65536. A buffer that runs out mid-vertex throws `gldata` after the resize and after the vertices that did fit. `format5` is not consulted again on this path. `gNd5` and `gNd2` write the same coordinates.

| Managed (`VmGlNodes1364Slice.cs`) | Native (`glnodes.cpp`) |
|-----------------------------------|------------------------|
| Missing segs, the GL-node probe, and GL vertex load | **1–240** |
| Band hull (exclusive slice) | **1–240** |

### Chunk **1364** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **240** |
| **Net headline** | **+240** vs the prior chunk |


**Post-review ratio:** 241/240 managed/native ≈ 100% (src file LOC at HEAD; see tally).

## Tests

- `VmGlNodes1364SliceTests.MissingSegsAndClosedSubsectors`
- `VmGlNodes1364SliceTests.LoadGlVertexes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1364 --to 1364` (0 hard / 0 soft).

## Seams

checkGLVertex and the later GL node loaders stay on chunks 888-890. gl_cachenodes and gl_cachetime are declarations. Callers of CheckForGLNodes are not in this band.

## Credit rationale

UNCredited glnodes.cpp 1-240 (+240). A full tally line scan, including recognition ranges, shows no overlap. This is the largest remaining hole of at most 400 lines. It is not a tie. i_main.cpp 1-238 is 238. Gaps longer than 400 lines stay whole and are not sliced. The later glnodes.cpp hole at 644-1195 is 552 lines. Chunk 888 already owns checkGLVertex at 241.
