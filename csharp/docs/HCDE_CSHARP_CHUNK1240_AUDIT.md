# Chunk 1240 — Paletted cache, true-color cache, and the format probe order from image.cpp (1-383).

## Scope

Paletted cache, true-color cache, and the format probe order from image.cpp (1-383).

## Native ranges

- `src/common/textures/image.cpp` **FImageSource::GetCachedPalettedPixels** 1–383 (383 lines)

**Credited native LOC:** 383 (tally `nativeLoc` 383).

## Managed

`VmImage1240Slice.cs`
Managed LOC (tally): 464.
Conversion commit: `871f92fd08ca7e77eac707b90adbad474c1159ae`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Precache tables (**32–56**) | yes |
| Default paletted pixels (**64–68**) | yes |
| Paletted cache (**71–123**) | yes |
| Owned or copied pixels (**126–142**) | yes |
| Base copy and translated copy (**158–173**) | yes |
| True-color cache (**181–260**) | yes |
| Precache counts and cache reset (**268–299**) | yes |
| Format probe order (**307–382**) | yes |
| Band hull (**1–383**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A paletted cache entry does not store the requested frame, so frame 0 later returns those pixels. Luminance paletted pixels skip the cache. Luminance true-color pixels are coerced to normal and can use the cache. A one-frame image rewrites frame 1 to 0 before either path. Palette alpha 0 does not write. A null true-color cache bitmap is copied again and the slot remains. FLAT is not probed unless the lump is a flat.

| Managed (`VmImage1240Slice.cs`) | Native (`image.cpp`) |
|----------------------------------|----------------------|
| Cache, copy, precache, and probe order | **1–383** |
| Band hull (exclusive slice) | **1–383** |

### Chunk **1240** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **383** |
| **Net headline** | **+383** vs post-**1239** |


**Post-review ratio:** 464/383 managed/native ≈ 121% (src file LOC at HEAD; see tally).

## Tests

- `VmImage1240SliceTests.PalettedCache_LeavesFrameUnsetAndCopiesViews`
- `VmImage1240SliceTests.BitmapCache_BlitsColumnsAndProbesFormatsInOrder`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1240 --to 1240` (0 hard / 0 soft).

## Seams

CreatePalettedPixels clears Width*Height bytes. GetCachedPalettedPixels searches the paletted cache only when the conversion is normal. A hit with more than one reference returns a view and decrements the count. The last reference with a non-empty buffer moves the pixels out and deletes the entry. A last reference whose buffer length is 0 leaves the entry in place. A miss with no precache row, a paletted count of 1 or less, or a conversion other than normal creates a fresh buffer. Otherwise the slice stores a cache entry, sets the remaining reference count to count-1, and clears the paletted count. That entry does not record the requested frame. The field stays 0, and the log says it was unset, so a later request for frame 0 receives the pixels created for the original frame. GetPalettedPixels returns the owned store, or a copy when the result is only a view. CopyPixels turns luminance into normal before creating pixels, then calls the null-info CopyPixelData path: column-major reads (step height, 1), BGRA writes, and palette alpha 0 is left untouched. The return is 0. CopyTranslatedPixels always creates normal pixels and uses the caller palette. GetCachedBitmap rewrites frame 1 to 0 when the image has one frame. A non-null remap skips the cache and uses the translated copy. Luminance becomes normal before the true-color lookup, so a luminance request can hit the normal cache. The true-color cache records the frame. It uses the true-color precache count, not the paletted count. A hit with more than one reference shares the bitmap. The last reference moves it. A hit whose pixel pointer is null is copied fresh and the empty slot stays. CollectForPrecache adds 1 to the true-color count or the paletted count. BeginPrecaching clears the counts and leaves the pixel caches. EndPrecaching clears both pixel caches and leaves the counts. A null image is not registered. GetImage returns null for lump -1, grows the lump table with null slots, returns an existing image without opening the file, and returns null when the reader is closed. The probe order is IMGZ, PNG, DDS, PCX, STB, QOI, WEBP, TGA, ANM, STARTUP, RAW, FLAT, PATCH, EMPTY, AUTOMAP. FLAT runs only when the caller asked for a flat. The first success is stored.

## Credit rationale

UNCredited image.cpp 1-383 (+383). The file ends at 383. A full tally line scan shows no overlap.
