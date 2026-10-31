# Chunk 1241 — Copy dispatch, clip rect, paletted copy, and bitmap clear from bitmap.cpp (188-483).

## Scope

Copy dispatch, clip rect, paletted copy, and bitmap clear from bitmap.cpp (188-483).

## Native ranges

- `src/common/textures/bitmap.cpp` **FBitmap::CopyPixelData** 188–483 (296 lines)

**Credited native LOC:** 296 (tally `nativeLoc` 296).

## Managed

`VmBitmapCopy1241Slice.cs`
Managed LOC (tally): 572.
Conversion commit: `d5155fa7227d1f982fc7a65e3bf0a7e55e0ad2e2`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Dispatch table (**188–220**) | yes |
| Clip rect (**227–341**) | yes |
| Intersect (**350–373**) | yes |
| True-color copy (**380–393**) | yes |
| Paletted copy (**395–436**) | yes |
| Palette rebuild and blit (**443–468**) | yes |
| Clear (**475–483**) | yes |
| Band hull (**188–483**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| bitmap.cpp 1–26, 46–63, and 99 | each run is shorter than the floor |

Zero counts `height - y` rows and still clears from row 0. `clip.x` is a byte offset. The paletted path does not rotate again. Palette rebuild is a forced copy, so alpha 0 stays 0. Blend shifts are 16 bits. A special index other than 0 writes nothing. The empty default blend writes nothing.

| Managed (`VmBitmapCopy1241Slice.cs`) | Native (`bitmap.cpp`) |
|--------------------------------------|------------------------|
| Dispatch, clip, paletted copy, and clear | **188–483** |
| Band hull (exclusive slice) | **188–483** |

### Chunk **1241** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **296** |
| **Net headline** | **+296** vs post-**1240** |


**Post-review ratio:** 572/296 managed/native ≈ 193% (src file LOC at HEAD; see tally).

## Tests

- `VmBitmapCopy1241SliceTests.Clip_RotatesFlipsAndClearsFromTheFirstRow`
- `VmBitmapCopy1241SliceTests.Dispatch_ConvertsFormatsAndAppliesPaletteOps`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1241 --to 1241` (0 hard / 0 soft).

## Seams

The dispatch table selects one of twelve format readers and one of ten BGRA operators. Blend shifts use 16 bits. Clip rotations 0-7 and the default path share the unrotated setup. The source pointer advances with the original steps, then the steps are replaced, then the clip uses the new steps. An odd rotation swaps width and height after the offset is chosen and before the pointer moves. A result size of 0 or less rejects the copy. Intersect can leave width or height negative and reports success only when both stay positive. CopyPixelDataRGB walks rows after the clip. The paletted blit ignores its rotate argument because the clip already applied it. A nonzero blend rebuilds the 256-entry palette with a forced copy, and an alpha of 0 stays zero. inf->palette replaces the palette only when the blend is 0. Zero starts at row 0 and treats clip.x as a byte offset; the y loop only counts rows. Special colormap index 0 is the only table this slice holds. Any other special blend logs and writes nothing. A blend of -3 or below is the empty default and writes nothing. A color type or operator outside the tables logs and does not write.

## Credit rationale

UNCredited bitmap.cpp 188-483 (+296). Lines 1-26, 46-63, and 99 stay below the floor. A full tally line scan shows no overlap with 27-45, 64-98, or 100-187.
