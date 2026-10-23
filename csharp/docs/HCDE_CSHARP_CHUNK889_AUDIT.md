# Chunk 889 — `MapLoader::LoadGLSubsectors` and `LoadNodes` (`glnodes.cpp` 424–643).

## Scope

`MapLoader::LoadGLSubsectors` and `LoadNodes` (`glnodes.cpp` 424–643).

## Native ranges

- `src/maploader/glnodes.cpp` **MapLoader::LoadGLSubsectors** 424–512 (89 lines)
- `src/maploader/glnodes.cpp` **MapLoader::LoadNodes** 520–643 (124 lines)

**Credited native LOC:** 213 (tally `nativeLoc` 213).

## Managed

`VmMapLoaderLoadGlSubsectorsAndNodes.cs`
Managed LOC (tally): 302.
Conversion commit: `8b6e1196893413f3653e2c0c9840d6b7f0dec2b5`.

## Padding review (2026-10-21)

**Behavior ported (yes/no)**

| Native | Behavior ported |
|--------|-----------------|
| `MapLoader::LoadGLSubsectors` **424–512** | **yes** |
| `MapLoader::LoadNodes` **520–643** | **yes** |

| Managed | Native (`glnodes.cpp`) |
|---------|----------------------|
| `LoadGlSubsectors` | **424–512** |
| `LoadGlNodes` | **520–643** |

### Padding map (hull **424–643**)

| Native lines | Reason |
|--------------|--------|
| **513–519** | comment/separator gap between `LoadGLSubsectors` and `LoadNodes` (in hull, no extra symbols) |

### Gross vs net

| | Value |
|--|------:|
| Gross native (`nativeLoc` sum of ranges) | **213** |
| Net behavior-ported | **213** |


**Post-review ratio:** 302/213 managed/native ≈ 142% (src file LOC at HEAD; see tally).

## Tests

- `VmMapLoaderLoadGlSubsectorsAndNodesTests.LoadGlSubsectors_Classic_PopulatesFirstSegIndex`
- `VmMapLoaderLoadGlSubsectorsAndNodesTests.LoadGlSubsectors_Classic_FailsWhenSubsectorNotClosed`
- `VmMapLoaderLoadGlSubsectorsAndNodesTests.LoadGlSubsectors_Miniseg_InheritsFrontSectorFromAnchor`
- `VmMapLoaderLoadGlSubsectorsAndNodesTests.LoadGlSubsectors_Gl3_SkipsMagicHeader`
- `VmMapLoaderLoadGlSubsectorsAndNodesTests.LoadGlNodes_Classic_ResolvesSubsectorChild`
- `VmMapLoaderLoadGlSubsectorsAndNodesTests.LoadGlNodes_Classic_FailsWhenNodeChildReused`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 889 --to 889` (0 hard / 0 soft).

## Seams

Lump bytes are `ReadOnlySpan<byte>` (native `FileReader::Read`). BSP child pointers are `BspChild` (subsector vs node index) instead of native `node_t::children` pointer tagging (`&subsectors[i]+1`). Subsector `firstline` is `FirstSegIndex` into the seg list.

## Credit rationale

Faithful GL subsector lump loader (classic and `gNd3`) plus classic/`format5` node lump loader; reuses `VmMapLoaderClassicNodes::ResolveChild` for the same child validation as native.
