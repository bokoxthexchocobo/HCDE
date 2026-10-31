# Chunk 1129 — Vertex bound updates, zoom limits, clip, parchment scroll, pan, and startDisplay head from `am_map.cpp` (**1201–1350**); follows **1128**.

## Scope

Vertex bound updates, zoom limits, clip, parchment scroll, pan, and startDisplay head from `am_map.cpp` (**1201–1350**); follows **1128**.

## Native ranges

- `src/am_map.cpp` **Automap bounds zoom and pan** 1201–1350 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapWindowBounds1129Slice.cs`
Managed LOC (tally): 162.
Conversion commit: `7968403ea915586b28bd1fa1f80338f9cd9f5189`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Vertex min/max updates and player min size (**1201–1217**) | yes |
| `calcMinMaxMtoF` (**1226–1234**) | yes |
| `ClipRotatedExtents` (**1242–1259**) | yes |
| `ScrollParchment` (**1267–1294**) | yes |
| `changeWindowLoc` (**1302–1328**) | yes |
| `startDisplay` clock, pan, zoom, and FTOM size (**1337–1348**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1193–1200** | bound seeds already credited by **1128** |
| **1349–1350** | blank line and the player-search comment |
| **1351+** | console-player fallback outside hull |

`PLAYERRADIUS` is **16**, so the minimum window and max zoom use **32**. Parchment coordinates clamp to **±40000**, then wrap by a positive patch size. `ClipRotatedExtents` pivot arguments are unused in this function. A zero patch size is not wrapped.

| Managed (`VmAmMapWindowBounds1129Slice.cs`) | Native (`am_map.cpp`) |
|---------------------------------------------|---------------------|
| Bounds, zoom, pan, display head | **1201–1350** |
| Band hull (exclusive slice) | **1201–1350** |

### Chunk **1129** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1128** |


**Post-review ratio:** 162/150 managed/native ≈ 108% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapWindowBounds1129SliceTests.ConsiderVertex_GrowsBoxAndPlayerMinSizeIsThirtyTwo`
- `VmAmMapWindowBounds1129SliceTests.ZoomScales_UsesTighterFitAndPlayerHeightCap`
- `VmAmMapWindowBounds1129SliceTests.ClipCenter_LocksWhenNotRotatingAndScrollWrapsPatch`
- `VmAmMapWindowBounds1129SliceTests.ChangeWindowLoc_DisablesFollowAndBeginDisplayResetsClock`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1129 --to 1129` (0 hard / 0 soft).

## Seams

Yaw rotation of the pan vector stays a host callback. Player search in `startDisplay` begins at **1351**.

## Credit rationale

UNCredited **`am_map.cpp`** **1201–1350** (+150), inside the 3594-line file, with no conversion or recognition overlap.
