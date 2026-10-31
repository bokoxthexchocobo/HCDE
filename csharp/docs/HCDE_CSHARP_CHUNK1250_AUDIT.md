# Chunk 1250 — DeleteAll, simple and complex anim records, and the ANIMATED lump walk from animations.cpp (1-294). InitAnimDefs at line 295 stays later.

## Scope

DeleteAll, simple and complex anim records, and the ANIMATED lump walk from animations.cpp (1-294). InitAnimDefs at line 295 stays later.

## Native ranges

- `src/gamedata/textures/animations.cpp` **DeleteAll** 1–294 (294 lines)

**Credited native LOC:** 294 (tally `nativeLoc` 294).

## Managed

`VmTextureAnim1250Slice.cs`
Managed LOC (tally): 380.
Conversion commit: `92f0870854cbcc533c69716e5558d94af5725cf0`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| DeleteAll (**52–71**) | yes |
| AddAnim (**82–98**) | yes |
| AddSimpleAnim (**109–127**) | yes |
| AddComplexAnim (**137–149**) | yes |
| debuganimated and InitAnimated (**189–285**) | yes |
| Band hull (**1–294**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| InitAnimDefs body | later function |
| ANIMDEFS parser | later functions |

DeleteAll frees switch definitions and clears animations, fire textures, switches, and doors, and leaves the base-pic index map in place. AddAnim replaces a recorded base when that slot still exists. AddSimpleAnim keeps one speed frame and a frame count of the whole range, and only when the endpoints share a use type and a file span. A texture id of 0 or less does not advance, so the compatibility check compares it with itself. AddComplexAnim copies the frame list and marks the animation discrete.

InitAnimated reads 23-byte ANIMATED records until a 0xFF flag. Byte 0 bit 0 selects a wall. Bit 1 allows decals. Bytes 10 through 18 are the start name and bytes 1 through 9 are the end name. A missing start skips the end lookup. A speed above 65535 sets warp style 2 when the start texture is not already warped. Otherwise a one-frame range is skipped, a descending range is swapped to backward, and the tic count is scaled by 1000/35. The backward flag stays set for later records in the same walk. A record that does not fit, or a flag byte past the lump, reports the past-end error and stops.

| Managed (`VmTextureAnim1250Slice.cs`) | Native (`animations.cpp`) |
|----------------------------------------|---------------------------|
| Setup, records, and the ANIMATED walk | **1–294** |
| Band hull (exclusive slice) | **1–294** |

### Chunk **1250** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **294** |
| **Net headline** | **+294** vs post-**1249** |


**Post-review ratio:** 380/294 managed/native ≈ 129% (src file LOC at HEAD; see tally).

## Tests

- `VmTextureAnim1250SliceTests.Init_ReplacesOnTheSameBaseAndWarpsAbove16Bits`
- `VmTextureAnim1250SliceTests.Init_SkipsAShortLumpAndReusesAStaleIndex`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1250 --to 1250` (0 hard / 0 soft).

## Seams

Texture lookup, file ranges, lump bytes, and hardware cleanup are host tables. CheckForTexture uses TEXMAN_Overridable only. A '-' name is texture 0. An index left behind by DeleteAll is replaced when it still falls inside the cleared array and is refused when it does not.

## Credit rationale

UNCredited animations.cpp 1-294 (+294). A full tally line scan shows no overlap. InitAnimDefs begins at line 295.
