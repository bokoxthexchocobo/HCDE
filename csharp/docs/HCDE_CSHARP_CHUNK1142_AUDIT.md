# Chunk 1142 — Cheat boxes, texture markers, numbered marks, and author-marker frames from `am_map.cpp` (**3151–3300**); follows **1141**.

## Scope

Cheat boxes, texture markers, numbered marks, and author-marker frames from `am_map.cpp` (**3151–3300**); follows **1141**.

## Native ranges

- `src/am_map.cpp` **Automap markers** 3151–3300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapMarkers1142Slice.cs`
Managed LOC (tally): 149.
Conversion commit: `b1d77366830f9ff03888b9d98a5cea869216eca4`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Cheat radius box (**3153–3164**) | yes |
| `DrawMarker` clip and scale (**3178–3207**) | yes |
| Numbered marks (**3215–3250**) | yes |
| Author-marker texture and flip (**3258–3300**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **3001–3150** | keys and triangles already credited by **1141** |
| **3303+** | TID targeting for map markers |

Cheats **3** and **6** draw a unit square at scale `radius`, with angle `drawnAngle - yaw`. A negative marker `xscale` toggles flip and becomes positive. `CXMTOF` / `CYMTOF` truncate toward zero. The default mark font `AMMNUMx` keeps the number textures and a Y adjust of **-3**; any other font draws `'0' + index`. `MF2_DORMANT` is **0x10000000**. A rotation index of **0xFFFF** keeps the picture. Otherwise flip is bit **0** of the sprite frame.

| Managed (`VmAmMapMarkers1142Slice.cs`) | Native (`am_map.cpp`) |
|------------------------------------------|---------------------|
| Boxes, marks, author frames | **3151–3300** |
| Band hull (exclusive slice) | **3151–3300** |

### Chunk **1142** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1141** |


**Post-review ratio:** 149/150 managed/native ≈ 99% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapMarkers1142SliceTests.CheatBox_IsAUnitSquareAtTheRemainingYaw`
- `VmAmMapMarkers1142SliceTests.PlaceMarker_FlipsANegativeScaleAndTruncatesTheFramePoint`
- `VmAmMapMarkers1142SliceTests.Marks_UseTexturesForTheDefaultFontAndDigitsOtherwise`
- `VmAmMapMarkers1142SliceTests.AuthorMarkers_SkipDormantActorsAndReadTheFirstFlipBit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1142 --to 1142` (0 hard / 0 soft).

## Seams

Texture lookup and point rotation stay host-supplied. TID targeting begins at **3303**.

## Credit rationale

UNCredited **`am_map.cpp`** **3151–3300** (+150), inside the 3594-line file, with no conversion or recognition overlap.
