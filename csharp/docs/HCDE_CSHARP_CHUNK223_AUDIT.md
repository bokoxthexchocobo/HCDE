# Chunk 223 — TranslateLineDef Boomish branch, sector special translation, xlat expressions.

## Scope

TranslateLineDef Boomish branch, sector special translation, xlat expressions.

## Native ranges

- `src/gamedata/p_xlat.cpp` **TranslateLineDef Boomish** 178–294 (117 lines)
- `src/gamedata/p_xlat.cpp` **TranslateSectorSpecial** 296–325 (30 lines)
- `src/gamedata/p_xlat.cpp` **XlatExprEval** 327–331 (5 lines)
- `src/gamedata/p_xlat.cpp` **XlatExprEval** 333–338 (6 lines)
- `src/gamedata/p_xlat.cpp` **XlatExprEval** 340–348 (9 lines)
- `src/gamedata/p_xlat.cpp` **XlatExprEval** 350–358 (9 lines)
- `src/gamedata/p_xlat.cpp` **XlatExprEval** 360–368 (9 lines)
- `src/gamedata/p_xlat.cpp` **XlatExprEval** 370–379 (10 lines)
- `src/gamedata/p_xlat.cpp` **XlatExprEval** 381–390 (10 lines)
- `src/gamedata/p_xlat.cpp` **XlatExprEval** 392–401 (10 lines)
- `src/gamedata/p_xlat.cpp` **XlatExprEval** 403–411 (9 lines)
- `src/gamedata/p_xlat.cpp` **XlatExprEval** 413–421 (9 lines)
- `src/gamedata/p_xlat.cpp` **XlatExprEval** 423–431 (9 lines)
- `src/gamedata/p_xlat.cpp` **XlatExprEval** 433–440 (8 lines)
- `src/gamedata/p_xlat.cpp` **XlatExprEval** 442–455 (14 lines)

**Credited native LOC:** 264 (tally `nativeLoc` 264).

## Managed

(see conversion commit file list)
Managed LOC (tally): 183.
Conversion commit: `152a84a7aeffbff1d342d2de1f7df0692f38d8a9`.

## Padding review (2026-10-21)



### Out-of-body uncredit (223)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **276** |
| **Out-of-body removed** | **12** |
| **Honest** | **264** |


**Post-review ratio:** 183/264 managed/native ≈ 69% (src file LOC at HEAD; see tally).

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
