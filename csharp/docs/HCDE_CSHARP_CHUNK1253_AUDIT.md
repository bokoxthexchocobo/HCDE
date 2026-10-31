# Chunk 1253 — Light color, shader light distance, and fog setup from hw_setcolor.cpp (1-155), through end of file.

## Scope

Light color, shader light distance, and fog setup from hw_setcolor.cpp (1-155), through end of file.

## Native ranges

- `src/rendering/hwrenderer/scene/hw_setcolor.cpp` **SetColor** 1–155 (155 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmSetColor1253Slice.cs`
Managed LOC (tally): 167.
Conversion commit: `3bceda793e16d15f215a1be9f7ed0b53695c0fc3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| SetColor (**26–47**) | yes |
| SetShaderLight (**49–82**) | yes |
| SetFog (**84–155**) | yes |
| Band hull (**1–155**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Light level and fog density | other file |
| Render-state object | host log |

A fullbright color is white with desaturation zero. Software lighting then stores soft level 255, and any other mode stores no soft level. A shaded color asks the host for the hardware light and the palette color, then clamps the sector plus relative light into 0 through 255 for the soft level. Shader distance uses 256 times visibility over 8. An outside light below 96 uses half of that distance plus the original light times the distance over 96 over 2, then the factor uses 96. A factor of exactly 1 stores distance 0. A level at or below 0 stores factor 1 and distance 0. A fade table forces density 70 and color 808080. Otherwise a colormap that is not fullbright asks the host for density and then clears the color alpha. A skybox adds half the density. Fog mode 0 or a density of 0 turns fog off. Black fog in Doom mode, or in a software mode whose blend factor is above 0, runs the shader-light path. Build mode stores 0.2 times the density and 1 over 31. Additive fog clears the color after that choice. Software lighting with blend 0, bright fog, and a nonzero color stores soft level 255. Build counts as software lighting.

| Managed (`VmSetColor1253Slice.cs`) | Native (`hw_setcolor.cpp`) |
|------------------------------------|--------------------------------|
| Color, shader light, and fog | **1–155** |
| Band hull (exclusive slice) | **1–155** |

### Chunk **1253** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **155** |
| **Net headline** | **+155** vs post-**1252** |


**Post-review ratio:** 167/155 managed/native ≈ 108% (src file LOC at HEAD; see tally).

## Tests

- `VmSetColor1253SliceTests.Color_ClampsRelativeLightAndZerosAUnitShaderFactor`
- `VmSetColor1253SliceTests.Fog_DensifiesInASkyboxAndFoldsBlackFogThroughTheShader`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1253 --to 1253` (0 hard / 0 soft).

## Seams

CalcLightLevel, CalcLightColor, and GetFogDensity stay in hw_lighting.cpp and are host callbacks. The software-mode predicate and the 0..255 light clamp are the header inlines those calls use. r_visibility and gl_fogmode are stored cvars. The skybox flag and the level fade-table and bright-fog flags are host state.

## Credit rationale

UNCredited hw_setcolor.cpp 1-155 (+155). A full tally line scan shows no overlap. The file ends at line 155.
