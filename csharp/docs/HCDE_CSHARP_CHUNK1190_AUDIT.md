# Chunk 1190 — Colormap lumps and the invulnerability override from colormaps.cpp (1–224).

## Scope

Colormap lumps and the invulnerability override from colormaps.cpp (1–224).

## Native ranges

- `src/r_data/colormaps.cpp` **R_InitColormaps** 1–224 (224 lines)

**Credited native LOC:** 224 (tally `nativeLoc` 224).

## Managed

`VmColormaps1190Slice.cs`
Managed LOC (tally): 270.
Conversion commit: `8375ea1a677e3ed1629779109842c9978fd73794`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Invulnerability cvars (**37–50**) | yes |
| R_DeinitColormaps (**65–69**) | yes |
| R_InitColormaps (**77–156**) | yes |
| R_ColormapNumForName (**166–182**) | yes |
| R_BlendForColormap (**190–194**) | yes |
| R_UpdateInvulnerabilityColormap (**203–224**) | yes |
| Band hull (**1–224**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

The three cvars are archived and do not run their callbacks at startup. Turning the override on or off rebuilds the invulnerability map. A color change rebuilds it only while the override is on. Init clears both lists, keeps the first lump of each colormap name, and brightens that lump's average by the palette brightness. Names are compared eight characters at a time, ignoring case. COLORMAP is always index 0. A missing WATERMAP is the fixed translucent blue. A color that already has alpha is its own blend. The override writes the two colors as floats divided by 128, and turning it off copies the real inverse map onto the inverse map.

| Managed (`VmColormaps1190Slice.cs`) | Native (`colormaps.cpp`) |
|--------------------------------------|---------------------------|
| Colormap init | **1–224** |
| Band hull (exclusive slice) | **1–224** |

### Chunk **1190** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **224** |
| **Net headline** | **+224** vs post-**1189** |


**Post-review ratio:** 270/224 managed/native ≈ 121% (src file LOC at HEAD; see tally).

## Tests

- `VmColormaps1190SliceTests.Init_BuildsBlendsFromTheFirstColormapLump`
- `VmColormaps1190SliceTests.ColormapNumForName_PrefersTheLastMatchAndWater`
- `VmColormaps1190SliceTests.Invulnerability_UsesCustomColorsOrRestoresTheRealInverse`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1190 --to 1190` (0 hard / 0 soft).

## Seams

Palette entries, colormap-namespace lumps, and the special-colormap builder are host data. allowCustomColormap is accepted and unused. The first fake colormap's lump is left at 0 because this function never reads it.

## Credit rationale

UNCredited colormaps.cpp 1–224 (+224). The file ends at 224. A full tally line scan shows no overlap.
