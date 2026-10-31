# Chunk 1339 — FUE1Model load, geometry, and frame draw from models_ue1.cpp (1-324).

## Scope

FUE1Model load, geometry, and frame draw from models_ue1.cpp (1-324).

## Native ranges

- `src/common/models/models_ue1.cpp` **LoadGeometry** 1–324 (324 lines)

**Credited native LOC:** 324 (tally `nativeLoc` 324).

## Managed

`VmUe1Model1339Slice.cs`
Managed LOC (tally): 748.
Conversion commit: `39c0695281dffc38abf0953ca528fafd277f0f4e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Packed vertex unpack (**31–44**) | yes |
| Data and aniv lump swap (**46–66**) | yes |
| Geometry, normals, and groups (**68–210**) | yes |
| Unload of verts and poly lists (**212–223**) | yes |
| Frame index (**225–231**) | yes |
| Frame draw (**233–269**) | yes |
| Vertex buffer (**271–309**) | yes |
| Skin hit list (**311–319**) | yes |
| Destructor (**321–324**) | yes |
| Band hull (**1–324**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| IndexOf and Substitute bodies | string callees |
| strtol | frame names are numbers |
| Unit and the cross product | vector callees |
| ReadFile and GetGameTexture | filesystem and texture callees |

The first `_d.3d` selects the data lump only when its index equals the length minus 5, using a size_t compare, so a 4-byte name with no match also selects data. Substitute rewrites every match. Deus Ex stores X, Z, and negated Y. Packed coordinates sign-extend after the shift. UVs divide by 255. A zero-length normal stays zero. Groups key on the raw type byte and the texture number, and a mismatch retries the same poly. Weapon triangles advance the vertex cursor and are stored in the buffer, but they are not drawn. Curvy groups store the facet normal. A buffer that already exists for the renderer type skips geometry. FindFrame is base 0 and returns -1 past the frame list. AddSkins ors HIT_Flat through the texture index. Unload clears vertex refs, polys, and group poly lists, and the destructor clears the groups themselves.

| Managed (`VmUe1Model1339Slice.cs`) | Native (`models_ue1.cpp`) |
|------------------------------------|---------------------------|
| Load, geometry, skins, and the frame draw | **1–324** |
| Band hull (exclusive slice) | **1–324** |

### Chunk **1339** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **324** |
| **Net headline** | **+324** vs post-**1338** |


**Post-review ratio:** 748/324 managed/native ≈ 231% (src file LOC at HEAD; see tally).

## Tests

- `VmUe1Model1339SliceTests.NamesUnpackAndPackedFrame`
- `VmUe1Model1339SliceTests.DeusExGroupsNormalsAndDraw`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1339 --to 1339` (0 hard / 0 soft).

## Seams

GetFileFullName, CheckNumForFullName, and ReadFile stay injected. GetGameTexture stays injected. Renderer SetInterpolation, SetMaterial, SetupFrame, DrawArrays, and CreateVertexBuffer stay injected. A zero numverts header throws instead of dividing.

## Credit rationale

UNCredited models_ue1.cpp 1-324 (+324). A full tally line scan shows no overlap. This whole file was tied with RuntimeState.cpp 1-324. Path order takes the model loader.
