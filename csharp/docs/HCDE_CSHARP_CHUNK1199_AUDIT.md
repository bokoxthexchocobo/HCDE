# Chunk 1199 — Legacy render styles, alpha, visibility, and fuzz adjustment from renderstyle.cpp (1–150).

## Scope

Legacy render styles, alpha, visibility, and fuzz adjustment from renderstyle.cpp (1–150).

## Native ranges

- `src/common/engine/renderstyle.cpp` **FRenderStyle::IsVisible** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmRenderStyle1199Slice.cs`
Managed LOC (tally): 148.
Conversion commit: `36ada46babc8308891a4f43ed4d9e20db9612e6a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| r_drawtrans and r_drawfuzz (**30–31**) | yes |
| LegacyRenderStyles (**35–56**) | yes |
| GetAlpha (**58–68**) | yes |
| FRenderStyle::IsVisible (**79–99**) | yes |
| FRenderStyle::CheckFuzz (**110–150**) | yes |
| Band hull (**1–150**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

GetAlpha returns 0, 1, the passed alpha, or one minus that alpha. Any other selector returns one half. IsVisible rejects STYLEOP_None. Add and reverse-subtract force alpha to 1 when STYLEF_Alpha1 is set and otherwise clamp it to 0 through 1. Those ops are visible when the source factor is not 0 or the destination factor is not 1. Every other blend op is visible. CheckFuzz leaves ordinary ops unchanged. Fuzz-or-add, fuzz-or-sub, and fuzz-or-reverse-sub become add, sub, or reverse-sub when translucent drawing is on and r_drawfuzz is 0. Otherwise r_drawfuzz 2 selects shadow and every other value selects fuzz.

| Managed (`VmRenderStyle1199Slice.cs`) | Native (`renderstyle.cpp`) |
|---------------------------------------|----------------------------|
| Render style | **1–150** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **1199** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1198** |


**Post-review ratio:** 148/150 managed/native ≈ 99% (src file LOC at HEAD; see tally).

## Tests

- `VmRenderStyle1199SliceTests.LegacyTable_MatchesNativeStylesAndAlpha`
- `VmRenderStyle1199SliceTests.IsVisible_AndCheckFuzz_FollowBlendAndCvars`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1199 --to 1199` (0 hard / 0 soft).

## Seams

r_drawtrans and r_drawfuzz are the two cvars declared in this file. CheckFuzz returns the adjusted style instead of mutating a caller-owned object. The legacy table is the nineteen styles in source order.

## Credit rationale

UNCredited renderstyle.cpp 1–150 (+150). The file ends at 150. A full tally line scan shows no overlap.
