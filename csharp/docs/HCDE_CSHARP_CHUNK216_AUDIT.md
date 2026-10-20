# Chunk 216 — InitDoomFonts lump selection and HU font constants.

## Scope

InitDoomFonts lump selection and HU font constants.

## Native ranges

- `src/gamedata/doomfont.cpp` **HU font constants** 30–34 (5 lines)
- `src/gamedata/doomfont.cpp` **InitDoomFonts small font** 36–92 (57 lines)
- `src/gamedata/doomfont.cpp` **InitDoomFonts fallbacks** 120–169 (50 lines)

**Credited native LOC:** 112 (tally `nativeLoc` 112).

## Managed

`VmDoomFontPlanner.cs`
Managed LOC (tally): 112.
Conversion commit: `7a78aca3d301f3bebcdeb22796d9375f033fc1c3`.

## Tests

- `VmDoomFontPlannerTests.HuFontSize_MatchesNativeRange`
- `VmDoomFontPlannerTests.PlanSmallFont_PrefersFontAWhenPresent`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 216 --to 216` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
