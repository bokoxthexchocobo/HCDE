# Chunk 1140 — Glyph endpoints and player arrows from `am_map.cpp` (**2851–3000**); follows **1139**.

## Scope

Glyph endpoints and player arrows from `am_map.cpp` (**2851–3000**); follows **1139**.

## Native ranges

- `src/am_map.cpp` **Automap player arrows** 2851–3000 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapPlayerArrow1140Slice.cs`
Managed LOC (tally): 161.
Conversion commit: `10ebb606689622cd12e7f62585e159c894d14673`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Glyph rotate and translate (**2854–2876**) | yes |
| Thing-sprite early out and single-player arrow (**2885–2928**) | yes |
| Ally, deathmatch, HSV, and rotated marks (**2930–2982**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2701–2850** | wall colors and glyph scale already credited by **1139** |
| **3001+** | key iterator body |

`DF2_NO_AUTOMAP_ALLIES` is **1<<19**. Cheat **4** still draws arrows when thing sprites are on; cheats **2**, **3**, and **5+** return early when `am_showthingsprites > 0`. A rotated single-player arrow uses **90** degrees and the cheat arrow only when `am_cheat != 0` and that list is non-empty. The draw scale is **0**. Remote marks with alpha below **1** use the almost-background color. Otherwise HSV is converted and each channel is `clamp((int)(c * 255), 0, 255)`. A rotated remote angle is `yaw - (cameraYaw - 90)`.

| Managed (`VmAmMapPlayerArrow1140Slice.cs`) | Native (`am_map.cpp`) |
|---------------------------------------------|---------------------|
| Glyphs and player arrows | **2851–3000** |
| Band hull (exclusive slice) | **2851–3000** |

### Chunk **1140** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1139** |


**Post-review ratio:** 161/150 managed/native ≈ 107% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapPlayerArrow1140SliceTests.FinishGlyph_ScalesRotatesAndTranslatesBothEnds`
- `VmAmMapPlayerArrow1140SliceTests.SinglePlayerMark_UsesNorthWhenRotatedAndTheCheatArrowWhenPresent`
- `VmAmMapPlayerArrow1140SliceTests.RemoteMark_SkipsAlliesAndDeathmatchStrangers`
- `VmAmMapPlayerArrow1140SliceTests.RemoteMark_ColorsFromHsvAndRotatesTheOffsetPosition`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1140 --to 1140` (0 hard / 0 soft).

## Seams

Point rotation and actor position stay host-supplied. Key-inventory filtering begins at **3002**.

## Credit rationale

UNCredited **`am_map.cpp`** **2851–3000** (+150), inside the 3594-line file, with no conversion or recognition overlap.
