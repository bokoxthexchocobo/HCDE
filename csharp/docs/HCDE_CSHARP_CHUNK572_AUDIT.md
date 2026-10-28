# Chunk 572 — Re-land: `A_CustomRailgun`, `A_Recoil`, and print/log actions (`p_actionfunctions.cpp` **1267–1512**). Jump/radius-self remain on combat rows (**923–1022**).

## Scope

Re-land: `A_CustomRailgun`, `A_Recoil`, and print/log actions (`p_actionfunctions.cpp` **1267–1512**). Jump/radius-self remain on combat rows (**923–1022**).

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_CustomRailgun** 1267–1374 (108 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Recoil** 1381–1388 (8 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Print** 1398–1425 (28 lines)
- `src/playsim/p_actionfunctions.cpp` **A_PrintBold** 1433–1456 (24 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Log** 1464–1476 (13 lines)
- `src/playsim/p_actionfunctions.cpp` **A_LogInt** 1484–1493 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **A_LogFloat** 1501–1512 (12 lines)

**Credited native LOC:** 203 (tally `nativeLoc` 203).

## Managed

`VmActionJumpRadiusRail.cs`, `VmActionPrintLogRecoil.cs`
Managed LOC (tally): 169.
Conversion commit: `841141689155eb31de489387ee9baa1da6cbed0b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `CustomRailgunShouldAbort` / `NormalizeRailRange` / `NormalizeRailSparsity` / `StealthVisDirOnFire` / `SpreadJitter` / `FillRailParams` / `FlashDamageFlags` | `A_CustomRailgun` **1267–1374** (policy through spread + param fill; `P_RailAttack` seam) |
| `RecoilThrustAngleDegrees` | `A_Recoil` **1381–1388** |
| `ResolveDollarString` / `ShouldPrintToLocalView` / `SelectMidPrintDuration` / `WithTemporaryMidTime` | `A_Print` **1398–1425** |
| `ResolveDollarString` / `SelectMidPrintDuration` | `A_PrintBold` **1433–1456** |
| `ShouldSkipLocalLog` / `ResolveDollarString` | `A_Log` **1464–1476** |
| `ShouldSkipLocalLog` / `FormatLogInt` | `A_LogInt` **1484–1493** |
| `ShouldSkipLocalLog` / `FormatLogFloat` | `A_LogFloat` **1501–1512** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **203** |
| Net behavior-ported | **203** |


**Post-review ratio:** 169/203 managed/native ≈ 83% (src file LOC at HEAD; see tally).

## Tests

- `VmActionJumpRadiusRailTests.CustomRailgunShouldAbort_WhenAimWithoutTarget`
- `VmActionJumpRadiusRailTests.NormalizeRailRange_DefaultsWhenZero`
- `VmActionJumpRadiusRailTests.SpreadJitter_ScalesWhenNotExplicitAngle`
- `VmActionPrintLogRecoilTests.RecoilThrustAngleDegrees_AddsHalfTurn`
- `VmActionPrintLogRecoilTests.ShouldPrintToLocalView_WhenSelfOrTargetLocal`
- `VmActionPrintLogRecoilTests.WithTemporaryMidTime_SwapsDuration`
- `VmActionPrintLogRecoilTests.SelectMidPrintDuration_UsesDefaultWhenNonPositive`
- `VmActionPrintLogRecoilTests.ShouldSkipLocalLog_WhenLocalAndNotSelfView`
- `VmActionPrintLogRecoilTests.FormatLogInt_MatchesPrintf`
- `VmActionPrintLogRecoilTests.FormatLogFloat_MatchesPrintf`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 572 --to 572` (0 hard / 0 soft).

## Seams

`P_RailAttack`, `P_AimLineAttack`, `A_CustomRailgun_ShadowHandling`, `Thrust`, `V_GetFont`, `strbin1`, `C_MidPrint`, and `Printf` are not hosted.

## Credit rationale

Disjoint from retired **573**/**576** hulls; credits railgun setup through `FRailParams` fill, recoil thrust angle, and print/log gating helpers.
