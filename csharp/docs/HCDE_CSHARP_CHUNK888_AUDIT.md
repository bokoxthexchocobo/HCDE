# Chunk 888 — `MapLoader::checkGLVertex`, `checkGLVertex3`, and `LoadGLSegs` (`glnodes.cpp` 241–415).

## Scope

`MapLoader::checkGLVertex`, `checkGLVertex3`, and `LoadGLSegs` (`glnodes.cpp` 241–415).

## Native ranges

- `src/maploader/glnodes.cpp` **MapLoader::checkGLVertex** 241–246 (6 lines)
- `src/maploader/glnodes.cpp` **MapLoader::checkGLVertex3** 248–253 (6 lines)
- `src/maploader/glnodes.cpp` **MapLoader::LoadGLSegs** 261–415 (155 lines)

**Credited native LOC:** 167 (tally `nativeLoc` 167).

## Managed

`VmMapLoaderLoadGlSegs.cs`
Managed LOC (tally): 252.
Conversion commit: `c004b38c30cd14748afeae5aa2887f498fcb1496`.

## Padding review (2026-10-21)

**Behavior ported (yes/no)**

| Native | Behavior ported |
|--------|-----------------|
| `MapLoader::checkGLVertex` **241–246** | **yes** |
| `MapLoader::checkGLVertex3` **248–253** | **yes** |
| `MapLoader::LoadGLSegs` **261–415** | **yes** |

| Managed | Native (`glnodes.cpp`) |
|---------|----------------------|
| `CheckGlVertex` | **241–246** |
| `CheckGlVertex3` | **248–253** |
| `LoadGlSegs` | **261–415** |

### Padding map (hull **241–415**)

| Native lines | Reason |
|--------------|--------|
| **247**, **254–260** | comment/separator gaps between functions (in hull, no extra symbols) |

### Gross vs net

| | Value |
|--|------:|
| Gross native (`nativeLoc` sum of ranges) | **167** |
| Net behavior-ported | **167** |

### Out-of-body uncredit (888)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **167** |
| **Out-of-body removed** | **0** |
| **Honest** | **167** |


**Post-review ratio:** 252/167 managed/native ≈ 151% (src file LOC at HEAD; see tally).

## Tests

- `VmMapLoaderLoadGlSegsTests.CheckGlVertex_RemapsHighBitThroughFirstGlVertex`
- `VmMapLoaderLoadGlSegsTests.CheckGlVertex3_RemapsSignBitThroughFirstGlVertex`
- `VmMapLoaderLoadGlSegsTests.LoadGlSegs_ClassicMiniseg_PopulatesSegWithoutLinedef`
- `VmMapLoaderLoadGlSegsTests.LoadGlSegs_ClassicLinedef_ClearsTwoSidedWhenBackMissing`
- `VmMapLoaderLoadGlSegsTests.LoadGlSegs_Gl3Header_ParsesAfterMagicSkip`
- `VmMapLoaderLoadGlSegsTests.LoadGlSegs_ReturnsFalseWhenVertexOutOfRange`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 888 --to 888` (0 hard / 0 soft).

## Seams

Lump bytes are supplied as `ReadOnlySpan<byte>` (native `FileReader::Read`); no `FileReader` host type in credited file. `PartnerSeg` pointer wiring is represented as `PartnerIndex` on `GlSeg` (same partner index validation as native).

## Credit rationale

Faithful GL node seg loader for classic `glseg_t` and `gNd3`/`glseg3_t` paths; vertex remap helpers are whole-function ports on the same row.
