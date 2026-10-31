# Chunk 1275 — Software renderer precache, camera views, and the default colormap from r_swrenderer.cpp (1-271).

## Scope

Software renderer precache, camera views, and the default colormap from r_swrenderer.cpp (1-271).

## Native ranges

- `src/rendering/swrenderer/r_swrenderer.cpp` **FSoftwareRenderer** 1–271 (271 lines)

**Credited native LOC:** 271 (tally `nativeLoc` 271).

## Managed

`VmSwRenderer1275Slice.cs`
Managed LOC (tally): 251.
Conversion commit: `f338b6835b3183d9ab97c676b91d4fd5b88f9cff`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Freelook cvar (**49–53**) | yes |
| Init and create (**64–73**) | yes |
| PreparePrecache (**75–92**) | yes |
| PrecacheTexture (**94–117**) | yes |
| Precache (**119–174**) | yes |
| RenderView (**176–188**) | yes |
| WriteSavePic (**192–204**) | yes |
| DrawRemaining (**206–213**) | yes |
| SetClearColor (**215–218**) | yes |
| RenderTextureView (**222–255**) | yes |
| SetColormap (**257–271**) | yes |
| Band hull (**1–271**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Shade map build | host colormap |
| Scene pixels | host drawer |

The old freelook cvar stores its value and sends pitch limits only while a user game is running. Init builds shade maps and then software colormaps. Prepare reads true color before it rejects a missing, invalid, or canvas texture. Checked pixels unload only when the cache value is zero. Unchecked pixels register only when the cache is nonzero, and that path reads true color again. A column fetch wins when the cache shares any wall, sky, or sprite bit, including a sprite hit alone. A flat uses the pixel fetch. Zero cache does neither. Precache marks sprites from the last index downward and writes the sprite hit over the previous value. A sprite index past the list faults before those writes. Frames walk forward, and only valid rotations are stored. Precache then prepares every texture from the end, then fetches every texture from the end. Render installs the global view, draws, and copies the scene view back before any camera. A missing camera texture returns after that install and does not restore a later camera pitch. A present camera saves the light, switches to the camera field of view, draws with the first-update flag, restores the field of view and the light, and copies the camera view back. The save picture draws without that flag and writes a paletted image, right side up, for the restored sector. Remaining player sprites install, draw, and copy the view back. The default colormap is white with no desaturation, then the level fade. A nonzero global colormap selects the custom name. A zero fade and a zero global colormap select the fade table. Any other fade leaves the name unset.

| Managed (`VmSwRenderer1275Slice.cs`) | Native (`r_swrenderer.cpp`) |
|--------------------------------------|-----------------------------|
| Precache, views, and colormap | **1–271** |
| Band hull (exclusive slice) | **1–271** |

### Chunk **1275** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **271** |
| **Net headline** | **+271** vs post-**1274** |


**Post-review ratio:** 251/271 managed/native ≈ 93% (src file LOC at HEAD; see tally).

## Tests

- `VmSwRenderer1275SliceTests.Precache_OverwritesSpriteHitsAndLeavesAFlatOnPixels`
- `VmSwRenderer1275SliceTests.Views_RestoreAfterTheDrawAndSkipAMissingCamera`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1275 --to 1275` (0 hard / 0 soft).

## Seams

Shade maps, texture pixels, and the scene draw are host inputs. The slice keeps the sprite-hit overwrite, the column-mode mask, the camera early return, and the colormap priority.

## Credit rationale

UNCredited r_swrenderer.cpp 1-271 (+271). A full tally line scan shows no overlap. The file ends at line 271.
