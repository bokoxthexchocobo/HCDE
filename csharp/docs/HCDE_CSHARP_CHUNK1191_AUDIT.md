# Chunk 1191 — Camera-to-canvas assignments from r_canvastexture.cpp (1–200).

## Scope

Camera-to-canvas assignments from r_canvastexture.cpp (1–200).

## Native ranges

- `src/r_data/r_canvastexture.cpp` **FCanvasTextureInfo::Add** 1–200 (200 lines)

**Credited native LOC:** 200 (tally `nativeLoc` 200).

## Managed

`VmCanvasTexture1191Slice.cs`
Managed LOC (tally): 244.
Conversion commit: `11dd97f159689373e09ca2d0433847e7208f1e9a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| FCanvasTextureInfo::Add (**40–77**) | yes |
| SetCameraToTexture (**87–99**) | yes |
| UpdateAll (**109–118**) | yes |
| EmptyList (**128–131**) | yes |
| Serialize (**141–184**) | yes |
| Mark (**194–200**) | yes |
| Band hull (**1–200**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

An invalid texture id is ignored. A texture without a canvas is reported and not stored. The same texture object keeps its first picture number. Viewpoint or field-of-view changes mark the texture for a first update. SetCameraToTexture asks for an overridable wall and continues only when that texture is a hardware canvas. Update visits entries that have a viewpoint and still need an update. An empty list writes nothing. A present list writes canvastextures, skipping a null viewpoint or texture. A closed object writes no fields. A missing array on read leaves the list in place. A successful read assigns each record through Add. Mark visits every viewpoint.

| Managed (`VmCanvasTexture1191Slice.cs`) | Native (`r_canvastexture.cpp`) |
|------------------------------------------|-------------------------------|
| Canvas texture assignments | **1–200** |
| Band hull (exclusive slice) | **1–200** |

### Chunk **1191** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **200** |
| **Net headline** | **+200** vs post-**1190** |


**Post-review ratio:** 244/200 managed/native ≈ 122% (src file LOC at HEAD; see tally).

## Tests

- `VmCanvasTexture1191SliceTests.Add_ReplacesTheSameTextureAndRejectsANonCanvas`
- `VmCanvasTexture1191SliceTests.SetCameraToTexture_RequiresAHardwareCanvas`
- `VmCanvasTexture1191SliceTests.Serialize_SkipsEmptyAndRestoresThroughAdd`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1191 --to 1191` (0 hard / 0 soft).

## Seams

Texture lookup is a host result. A texture id is valid when it is greater than zero. Wall lookup uses texture type Wall and the overridable flag. The archive records the write calls; a missing canvastextures array leaves the list unchanged.

## Credit rationale

UNCredited r_canvastexture.cpp 1–200 (+200). The file ends at 200. A full tally line scan shows no overlap.
