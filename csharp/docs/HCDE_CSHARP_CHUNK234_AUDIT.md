# Chunk 234 — Uncredited doomfont gap lines outside HU macro band.

## Scope

Uncredited doomfont gap lines outside HU macro band.

## Native ranges

- `src/gamedata/doomfont.cpp` **doomfont file header** 1–29 (29 lines)
- `src/gamedata/doomfont.cpp` **HU_FONTSIZE line** 35–35 (1 lines)
- `src/gamedata/doomfont.cpp` **InitDoomFonts gap** 93–93 (1 lines)
- `src/gamedata/doomfont.cpp` **InitDoomFonts gap 2** 119–119 (1 lines)
- `src/gamedata/doomfont.cpp` **InitDoomFonts gap 3** 170–170 (1 lines)

**Credited native LOC:** 33 (tally `nativeLoc` 33).

## Managed

(see conversion commit file list)
Managed LOC (tally): 20.
Conversion commit: `20d15060ccd8058244d9e6fca30fb9834482d81c`.

## Tests

- `VmDoomFontConstantsTests.HuFontSize_MatchesNativeRange`
- `VmDoomFontPlannerTests.PlanBigFont_UsesZBigWhenNoFontB`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 234 --to 234` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
