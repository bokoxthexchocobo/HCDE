# Chunk 1211 — sRGB, OKLab, and OKLCH conversion and the hue mix from colorspace.cpp (1–218).

## Scope

sRGB, OKLab, and OKLCH conversion and the hue mix from colorspace.cpp (1–218).

## Native ranges

- `src/utility/colorspace.cpp` **Color::mix** 1–218 (218 lines)

**Credited native LOC:** 218 (tally `nativeLoc` 218).

## Managed

`VmColorSpace1211Slice.cs`
Managed LOC (tally): 282.
Conversion commit: `a75210923a8b10754e1f2fa59217d125be61c21d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| rgb and rgb24 (**32–60**) | yes |
| _2rgb and rgb2oklab (**62–83**) | yes |
| rgb2oklch through oklch2oklab (**85–126**) | yes |
| oklab, _2oklab, and oklab2rgb (**128–162**) | yes |
| oklab2oklch (**164–177**) | yes |
| alerp and mix (**179–214**) | yes |
| Band hull (**1–218**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

Factories store the space and the three channels. Converting copies the color, then switches on the space. sRGB to OKLab linearizes, takes the LMS cube roots, and stores the float matrix result. OKLab to sRGB cubes the LMS mix in double, rounds the argument to float, and applies the gamma clamp. OKLCH hue is atan2 degrees, or a quiet NaN when chroma is below 0.0001f. A NaN hue becomes zero a and b on the way back to OKLab. mix at or below 0 returns the first color, and mix at or above 1 returns the second. Inside that range both colors become OKLCH, L and C use libstdc++ lerp, and hue takes the short arc across 360. The result converts back to the first color's original space.

| Managed (`VmColorSpace1211Slice.cs`) | Native (`colorspace.cpp`) |
|--------------------------------------|---------------------------|
| Color conversion and mix | **1–218** |
| Band hull (exclusive slice) | **1–218** |

### Chunk **1211** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **218** |
| **Net headline** | **+218** vs post-**1210** |


**Post-review ratio:** 282/218 managed/native ≈ 129% (src file LOC at HEAD; see tally).

## Tests

- `VmColorSpace1211SliceTests.Rgb_RoundTripsOklabOklchAndRgb24`
- `VmColorSpace1211SliceTests.Mix_WrapsHueAndRejectsTheWrongSourceSpace`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1211 --to 1211` (0 hard / 0 soft).

## Seams

Color channels are float. pow, cbrt, sqrt, atan2, cos, sin, and fmod are the C double libm entry points, then the result is stored as float. atan2(float, float) therefore keeps the double angle through the degree scale. A chroma below 0.0001f stores the positive quiet NaN. CONVERT asserts the source space and throws here. rgb24 truncates in-range channels toward zero. The memcpy after the copy constructor rewrites the same bytes.

## Credit rationale

UNCredited colorspace.cpp 1–218 (+218). The file ends at 218. A full tally line scan shows no overlap.
