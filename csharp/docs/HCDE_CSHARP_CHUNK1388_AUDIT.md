# Chunk 1388 — Texture manipulation, side parts, the colormap, and the sector-plane repair from p_saveg.cpp (84-275).

## Scope

Texture manipulation, side parts, the colormap, and the sector-plane repair from p_saveg.cpp (84-275).

## Native ranges

- `src/p_saveg.cpp` **Serialize** 84–275 (192 lines)

**Credited native LOC:** 192 (tally `nativeLoc` 192).

## Managed

`VmSaveSidePlane1388Slice.cs`
Managed LOC (tally): 288.
Conversion commit: `ee681908e8afa2d0fc583f583ae38a424b2edf63`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Line return (**84–85**) | yes |
| Texture manipulation (**93–109**) | yes |
| Side part (**118–142**) | yes |
| Side (**150–165**) | yes |
| Linked sector (**173–182**) | yes |
| Colormap under the portal banner (**184–206**) | yes |
| Sector plane (**214–241**) | yes |
| Secplane repair (**249–268**) | yes |
| Closing banner (**270–275**) | yes |
| Band hull (**84–275**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Line body **65–83** | already credited above this hole |
| `sector_t::Serialize` **276+** | chunk 10 |
| Primitive serializers | callees outside this file |
| Struct padding | byte holes between fields are not stored |

A matching texture manipulation, side part, colormap, sector plane, or secplane returns before `BeginObject` when `canSkip` is true. `canSkip` is writing, inside an object, and not a rollback. Colormap equality includes `BlendFactor`, which the archive does not write. The sector plane compares each transform field and both sky textures. `FTransform` sum equality is a different function. The secplane compare includes `negiC` and the dither flag. A null default throws after the object opens for manipulation, side part, side, and sector plane. Linked sectors and the colormap do not read the default. Side textures use a three-element full compare; a rollback or a reading archive still opens the array. Reading clamps that array to its stored length and never grows past 3. Inside the array, `canSkip` is false, so each part is written, and an equal texture manipulation nested under `texturefx` can still be skipped once the part object is open. The manipulation keys are `addcolor`, `yoffset`, `xscale`, and `yscale` for `AddColor`, `ModulateColor`, `BlendColor`, and `DesaturationFactor`. Interpolation and attached decals pass no default. The normal vector is a three-double array. On a successful read, a non-zero `Z` sets `negiC` to `-1 / Z` after the object closes. A missing object or a zero `Z` leaves `negiC` alone. A write does not change `negiC`.

| Managed (`VmSaveSidePlane1388Slice.cs`) | Native (`p_saveg.cpp`) |
|-----------------------------------------|------------------------|
| Side, plane, and colormap serialize | **84–275** |
| Band hull (exclusive slice) | **84–275** |

### Chunk **1388** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **192** |
| **Net headline** | **+192** vs the prior chunk |


**Post-review ratio:** 288/192 managed/native ≈ 150% (src file LOC at HEAD; see tally).

## Tests

- `VmSaveSidePlane1388SliceTests.ManipulationPartAndSideKeepMismatchedKeys`
- `VmSaveSidePlane1388SliceTests.SectorPlaneSkipsOnBytesAndSecplaneRepairsNegiC`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1388 --to 1388` (0 hard / 0 soft).

## Seams

The line body above the return stays outside this hole. sector_t::Serialize starts at 276 and stays chunk 10. Primitive field serializers, DVector3, and the JSON writer stay callees. Padding between fields is not compared.

## Credit rationale

UNCredited p_saveg.cpp 84-275 (+192). After chunk 1387 this is the unique largest hole. r_farclip_line.cpp 1-188 is 188. The band is shorter than 400 lines and is not sliced.
