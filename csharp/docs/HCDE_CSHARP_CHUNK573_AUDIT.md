# Chunk 573 — A_Recoil through A_FadeIn (1381–1589)

## Scope

A_Recoil through A_FadeIn (1381–1589)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_Recoil** 1381–1388 (8 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Print** 1398–1425 (28 lines)
- `src/playsim/p_actionfunctions.cpp` **A_PrintBold** 1433–1456 (24 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Log** 1464–1476 (13 lines)
- `src/playsim/p_actionfunctions.cpp` **A_LogInt** 1484–1493 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **A_LogFloat** 1501–1512 (12 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetTranslucent** 1519–1531 (13 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetRenderStyle** 1538–1547 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **FadeFlags** 1557–1561 (5 lines)
- `src/playsim/p_actionfunctions.cpp` **A_FadeIn** 1563–1589 (27 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmActionPrintLogRecoil.cs`
Managed LOC (tally): 78.
Conversion commit: `0c812ee2609babd80e352b17d7ab27ad6d28f492`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `RecoilThrustAngleDegrees` | `A_Recoil` 1386–1387 |
| `ResolveDollarString` / `ShouldPrintToLocalView` / `SelectMidPrintDuration` / `WithTemporaryMidTime` | `A_Print` 1405–1421 |
| `PrintBoldAlwaysShows` / `SelectMidPrintDuration` | `A_PrintBold` 1440–1454 |
| `ShouldSkipLocalLog` / `FormatLogInt` / `FormatLogFloat` | `A_Log` / `A_LogInt` / `A_LogFloat` 1470–1509 |
| `MapTranslucentMode` / `ShouldClearAlpha1Flag` / `ClampAlpha` | `A_SetTranslucent` 1525–1529 |
| `ApplySetRenderStyle` | `A_SetRenderStyle` 1544–1545 |
| `FadeReduceDefault` / `ClearAlpha1RenderStyleFlag` / `AddFadeInReduce` / `StepFadeIn` | `A_FadeIn` 1570–1587 |

**Post-review ratio:** 78/150 managed/native ≈ 52% (src file LOC at HEAD; see tally).

## Tests

- `VmActionPrintLogRecoilTests.RecoilThrustAngleDegrees_AddsHalfTurn`
- `VmActionPrintLogRecoilTests.ResolveDollarString_LooksUpWhenPrefixed`
- `VmActionPrintLogRecoilTests.ShouldPrintToLocalView_WhenSelfOrTargetLocal`
- `VmActionPrintLogRecoilTests.WithTemporaryMidTime_SwapsDuration`
- `VmActionPrintLogRecoilTests.PrintBoldAlwaysShows`
- `VmActionPrintLogRecoilTests.SelectMidPrintDuration_UsesDefaultWhenNonPositive`
- `VmActionPrintLogRecoilTests.ShouldSkipLocalLog_WhenLocalAndNotSelfView`
- `VmActionPrintLogRecoilTests.FormatLogInt_MatchesPrintf`
- `VmActionPrintLogRecoilTests.FormatLogFloat_MatchesPrintf`
- `VmActionPrintLogRecoilTests.MapTranslucentMode_MapsFuzzyMode`
- `VmActionPrintLogRecoilTests.ClampAlpha_BoundsToUnitInterval`
- `VmActionPrintLogRecoilTests.ApplySetRenderStyle_ClampsAlpha`
- `VmActionPrintLogRecoilTests.ShouldClearAlpha1Flag_IsTrue`
- `VmActionPrintLogRecoilTests.StepFadeIn_ClampsAndMarksRemove`
- `VmActionPrintLogRecoilTests.FadeReduceDefault_UsesTenthWhenZero`
- `VmActionPrintLogRecoilTests.ClearAlpha1RenderStyleFlag_ClearsBit`
- `VmActionPrintLogRecoilTests.AddFadeInReduce_UsesDefaultReduce`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 573 --to 573` (0 hard / 0 soft).

## Seams

`C_MidPrint`, `Printf`, `V_GetFont`, and `Thrust` are not hosted.

## Credit rationale

Faithful managed mirrors for local-view gating, `$` string resolution, mid-print timing, and render-style mapping.
