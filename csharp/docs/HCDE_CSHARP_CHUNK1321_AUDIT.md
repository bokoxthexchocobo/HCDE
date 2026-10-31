# Chunk 1321 — Shade maps, the fuzz table, particle textures, and the wall-column lights from r_draw.cpp (1-285).

## Scope

Shade maps, the fuzz table, particle textures, and the wall-column lights from r_draw.cpp (1-285).

## Native ranges

- `src/rendering/swrenderer/drawers/r_draw.cpp` **R_InitShadeMaps** 1–285 (285 lines)

**Credited native LOC:** 285 (tally `nativeLoc` 285).

## Managed

`VmDrawTables1321Slice.cs`
Managed LOC (tally): 205.
Conversion commit: `036a3ab0f1a2348f609e89e5c7409178a2d7f204`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Dynamic-light cvar and the shade maps (**49–123**) | yes |
| Fuzz offset table (**125–168**) | yes |
| Particle textures (**170–190**) | yes |
| Fuzz cursor (**192–215**) | yes |
| Wall-column lights (**219–283**) | yes |
| Band hull (**1–285**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| The column pixel drawer | the host records the light list |

Each of the 16 shade sets fills 32 maps. A byte is `((k + 2) * alpha + 256) >> 14`, capped at 64. The identity map stores its own index. The fuzz count walks the signed init table, then each offset is the clamped shade product. Changing the dynamic-light cvar records the value and notifies the host. A fuzz-scale frame adds the random-table step and wraps the cursor at 100. Without fuzz scale, a column advances by its visible height and a frame start does not. A mirrored column measures from the right edge. A light outside its radius, a negative normal constant, or an inactive light is skipped. The list stops at 16 lights.

| Managed (`VmDrawTables1321Slice.cs`) | Native (`r_draw.cpp`) |
|--------------------------------------|------------------------|
| Shade, fuzz, particles, and column lights | **1–285** |
| Band hull (exclusive slice) | **1–285** |

### Chunk **1321** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **285** |
| **Net headline** | **+285** vs post-**1320** |


**Post-review ratio:** 205/285 managed/native ≈ 72% (src file LOC at HEAD; see tally).

## Tests

- `VmDrawTables1321SliceTests.Init_BuildsShadeFuzzAndParticleTables`
- `VmDrawTables1321SliceTests.Fuzz_AdvancesAndSetLightsKeepsTheNearColumn`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1321 --to 1321` (0 hard / 0 soft).

## Seams

The dynamic-light cvar sync is a host callback. The column drawer records the lights. Shade and particle tables are the data this file builds.

## Credit rationale

UNCredited r_draw.cpp 1-285 (+285). A full tally line scan shows no overlap. The file ends at line 285.
