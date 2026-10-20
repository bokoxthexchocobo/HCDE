# Chunk 224 — p_xlat triggertype header, R_InitSpriteDefs scan loop, BigFont planner branches.

## Scope

p_xlat triggertype header, R_InitSpriteDefs scan loop, BigFont planner branches.

## Native ranges

- `src/gamedata/p_xlat.cpp` **triggertype_e and TranslateLineDef header** 1–48 (48 lines)
- `src/gamedata/p_xlat.cpp` **TranslateLineDef tag/id hook** 98–107 (10 lines)
- `src/gamedata/p_xlat.cpp` **TranslateLineDef simple lookup** 117–117 (1 lines)
- `src/gamedata/doomfont.cpp` **InitDoomFonts BigFont selection** 94–118 (25 lines)
- `src/r_data/sprites.cpp` **R_InitSpriteDefs between-hash gap** 342–346 (5 lines)
- `src/r_data/sprites.cpp` **R_InitSpriteDefs lump scan** 396–460 (65 lines)

**Credited native LOC:** 154 (tally `nativeLoc` 154).

## Managed

(see conversion commit file list)
Managed LOC (tally): 200.
Conversion commit: `540c07958b88ab69ff55c8bb1ef0f06133924276`.

## Tests

- `VmXlatLineFlagTranslateTests.ApplyMaskPass_ClearsMaskedBits`
- `VmXlatLineFlagTranslateTests.ApplyZeroSpecial_ClearsArgs`
- `VmDoomFontPlannerTests.PlanBigFont_UsesZBigWhenNoFontB`
- `VmSpriteInitScanTests.ScanSpriteTextures_InvokesInstall`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 224 --to 224` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
