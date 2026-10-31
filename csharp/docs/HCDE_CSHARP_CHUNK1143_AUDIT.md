# Chunk 1143 — Marker targets, crosshair, drawer order, and the serialize head from `am_map.cpp` (**3301–3450**); follows **1142**.

## Scope

Marker targets, crosshair, drawer order, and the serialize head from `am_map.cpp` (**3301–3450**); follows **1142**.

## Native ranges

- `src/am_map.cpp` **Automap drawer** 3301–3450 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapDrawer1143Slice.cs`
Managed LOC (tally): 158.
Conversion commit: `ecf029d5211801d342d9bee9c1df2ff2ac478957`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Marker target, zoom scale, drawn-subsector gate (**3301–3325**) | yes |
| Crosshair (**3335–3338**) | yes |
| Drawer delta, pan, passes, frame (**3346–3433**) | yes |
| Serialize head through both scales (**3442–3450**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **3151–3300** | cheat boxes and mark font already credited by **1142** |
| **3451+** | remaining serialize fields; the file ends at **3594** |

`args[0] == 0` draws the marker actor once. `args[2] == 1` multiplies scale by `scale_mtof`. `args[1] == 1` requires `SSECMF_DRAWN` (**2**). `F_PANINC` is **140 / 35**. Pan step is that value times `scale_ftom * delta * 35`, and an inactive automap does not store the new clock. `LEVEL2_ALLMAP` is **2**. Cheat **4** does not draw things unless a scanner is carried on an all-map level. Subsector fill still draws in overlay when `fillAlpha > 0`. The crosshair is `((width/2), (height+1)/2)` and only when the automap owns the frame. The save head writes **20** mark doubles plus both scales.

| Managed (`VmAmMapDrawer1143Slice.cs`) | Native (`am_map.cpp`) |
|------------------------------------------|---------------------|
| Drawer and save head | **3301–3450** |
| Band hull (exclusive slice) | **3301–3450** |

### Chunk **1143** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1142** |


**Post-review ratio:** 158/150 managed/native ≈ 105% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapDrawer1143SliceTests.Markers_ScaleWithZoomOnlyWhenRequestedAndStopAfterASelfTarget`
- `VmAmMapDrawer1143SliceTests.ManualPan_KeepsAConstantPixelSpeedAndCancelsOppositeButtons`
- `VmAmMapDrawer1143SliceTests.Drawer_ClearsOnlyTheFullMapAndDrawsThingsForScannerOrCheat`
- `VmAmMapDrawer1143SliceTests.SaveHead_StoresTwentyMarkDoublesAndBothScales`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1143 --to 1143` (0 hard / 0 soft).

## Seams

Follow, zoom, and the draw calls stay host-supplied. `bigstate` begins at **3451**.

## Credit rationale

UNCredited **`am_map.cpp`** **3301–3450** (+150), inside the 3594-line file, with no conversion or recognition overlap.
