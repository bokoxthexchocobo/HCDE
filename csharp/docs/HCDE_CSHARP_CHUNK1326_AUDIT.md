# Chunk 1326 — MD3 load, surface geometry, and the interpolated frame from models_md3.cpp (1-377).

## Scope

MD3 load, surface geometry, and the interpolated frame from models_md3.cpp (1-377).

## Native ranges

- `src/common/models/models_md3.cpp` **FMD3Model::Load** 1–377 (377 lines)

**Credited native LOC:** 377 (tally `nativeLoc` 377).

## Managed

`VmMd3Model1326Slice.cs`
Managed LOC (tally): 549.
Conversion commit: `68c3533d465d676ea1fd02776a784711562beb0b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Lat/lng unpack (**37–47**) | yes |
| Header, frame names, and skin fallback (**126–177**) | yes |
| Triangle, texcoord, and vertex load (**185–230**) | yes |
| Vertex buffer, Y/Z swap, and raw indices (**238–293**) | yes |
| Skin hit list, frame lookup, and interpolated draw (**302–377**) | yes |
| Band hull (**1–377**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| MD3 tag payload | the header stores the count and does not read the tag bytes |
| LoadSkin file search | callee in model.cpp |

Frame names copy at most 15 bytes into a zeroed 16-byte field. Two or more surfaces set the multi-surface flag. Backslashes in a shader name become slashes. An invalid empty-path skin is loaded again with the model path. Vertices are signed shorts divided by 64, and the packed normal is an unsigned lat/lng. The buffer stores X, Z, Y and the normal NX, NZ, NY. Triangle indices are not biased by the surface vertex cursor. A second build for the same renderer type does not read the lump again. FindFrame is case-insensitive and ignores the default flag. A frame number outside the frame list, including a negative value under the unsigned compare, returns before interpolation. An explicit skin is clamped false. Otherwise a valid surface skin id is used, then the first surface skin, and a missing texture skips that surface. The draw offset is the index cursor times 4. Interpolation is cleared to 0 after the surfaces.

| Managed (`VmMd3Model1326Slice.cs`) | Native (`models_md3.cpp`) |
|------------------------------------|---------------------------|
| Load, geometry, skins, and the frame draw | **1–377** |
| Band hull (exclusive slice) | **1–377** |

### Chunk **1326** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **377** |
| **Net headline** | **+377** vs post-**1325** |


**Post-review ratio:** 549/377 managed/native ≈ 146% (src file LOC at HEAD; see tally).

## Tests

- `VmMd3Model1326SliceTests.Load_CopiesFramesAndFallsBackTheSkin`
- `VmMd3Model1326SliceTests.Geometry_SwapsAxesAndInterpolatesTheFrame`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1326 --to 1326` (0 hard / 0 soft).

## Seams

LittleShort and LittleLong are identity on this little-endian host. FixPathSeperator rewrites backslashes before LoadSkin. LoadSkin is the model.cpp callee: the empty path is tried first, then the model path when that id is invalid. FindFrame ignores nodefault and returns -1. A shader name with no NUL inside its 64-byte field stops at the field.

## Credit rationale

UNCredited models_md3.cpp 1-377 (+377). A full tally line scan shows no overlap. The file ends at line 377.
