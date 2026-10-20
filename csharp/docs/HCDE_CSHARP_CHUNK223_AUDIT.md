# Chunk 223 — TranslateLineDef Boomish branch, sector special translation, xlat expressions.

## Scope

TranslateLineDef Boomish branch, sector special translation, xlat expressions.

## Native ranges

- `src/gamedata/p_xlat.cpp` **TranslateLineDef Boomish** 178–294 (117 lines)
- `src/gamedata/p_xlat.cpp` **TranslateSectorSpecial** 296–325 (30 lines)
- `src/gamedata/p_xlat.cpp` **XlatExprEval** 327–455 (129 lines)

**Credited native LOC:** 276 (tally `nativeLoc` 276).

## Managed

(see conversion commit file list)
Managed LOC (tally): 175.
Conversion commit: `a2e37df1b6cc0e99899cb7e07eb9b03703a8e07b`.

## Tests

- `VmXlatBoomTranslateTests.TryTranslateBoomish_MatchesRange`
- `VmXlatSectorTranslateTests.TranslateSectorSpecial_AppliesMaskRules`
- `VmXlatExpressionEvalTests.Eval_AddsAndUsesTag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 223 --to 223` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
