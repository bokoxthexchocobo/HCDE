# Chunk 1136 — Polygon origin, hole-ring copy, shade, secret probe, and subsector debug colors from `am_map.cpp` (**2251–2400**); follows **1135**.

## Scope

Polygon origin, hole-ring copy, shade, secret probe, and subsector debug colors from `am_map.cpp` (**2251–2400**); follows **1135**.

## Native ranges

- `src/am_map.cpp` **Automap polygon shade** 2251–2400 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapPolyShade1136Slice.cs`
Managed LOC (tally): 158.
Conversion commit: `cb08eba794361edf1f8b18cf08a6a6e7f6d8ec7f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Fill origin zeros (**2251–2252**) | yes |
| Hole ring vs cleared indices (**2256–2276**) | yes |
| Soft and linear fade, texture scale (**2278–2303**) | yes |
| `AM_CheckSecret` (**2315–2337**) | yes |
| `drawSeg` / `drawPolySeg` rotation (**2346–2376**) | yes |
| Subsector debug gate, colors, yellow segs, poly-node match (**2378–2400**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2101–2250** | textured fill already credited by **1135** |
| **2401–2410** | red poly-seg loop that closes `showSS` |
| **2418+** | 3D-floor boundary and sector-action probes |

`SSECF_HOLE` is **8**. `ML_SECRET` is **0x20**. `NUMCOLORMAPS` is **32**. Soft fade is `clamp(((64 - (light+12)*0.25) - 12) / 32, 0, 1)`. Linear fade is `1 - clamp(light, 0, 255) / 255`. A revealed former secret returns **1** before a still-secret line without `ML_SECRET` returns **2**. `am_map_secrets == 0` draws neither.

| Managed (`VmAmMapPolyShade1136Slice.cs`) | Native (`am_map.cpp`) |
|------------------------------------------|---------------------|
| Shade, secrets, debug segs | **2251–2400** |
| Band hull (exclusive slice) | **2251–2400** |

### Chunk **1136** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1135** |


**Post-review ratio:** 158/150 managed/native ≈ 105% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapPolyShade1136SliceTests.HoleRing_TriangulatesOnlyWhenFlaggedAndMoreThanThreeLines`
- `VmAmMapPolyShade1136SliceTests.FadeLevel_UsesTheSoftRampOrTheLinearHardwareScale`
- `VmAmMapPolyShade1136SliceTests.CheckSecret_PrefersARevealedFrontSector`
- `VmAmMapPolyShade1136SliceTests.DebugStrokes_RotateSegsAndMarkTheShownSubsector`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1136 --to 1136` (0 hard / 0 soft).

## Seams

Earcut and AddPoly stay host-supplied. Red poly-seg drawing begins at **2401**.

## Credit rationale

UNCredited **`am_map.cpp`** **2251–2400** (+150), inside the 3594-line file, with no conversion or recognition overlap.
