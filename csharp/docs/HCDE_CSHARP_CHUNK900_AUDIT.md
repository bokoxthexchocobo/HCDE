# Chunk 900 — Texture floors through `LS_Ceiling_CrushAndRaise` (`p_lnspec.cpp` **472–700**). Follows **899** (**≤470**); stops before `LS_Ceiling_LowerAndCrush` (**700+**).

## Scope

Texture floors through `LS_Ceiling_CrushAndRaise` (`p_lnspec.cpp` **472–700**). Follows **899** (**≤470**); stops before `LS_Ceiling_LowerAndCrush` (**700+**).

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Floor_RaiseByTexture** 472–700 (229 lines)

**Credited native LOC:** 229 (tally `nativeLoc` 229).

## Managed

`VmPLnspecFloorStairsCeilingSlice.cs`
Managed LOC (tally): 218.
Conversion commit: `56dcf7a9df108f955316f6e21dbdd6c12e07adaa`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_lnspec.cpp`) |
|---------|-------------------------|
| Texture / TxTy / raise-to-ceiling floors | **472–500** |
| Waggle, transfer, donut, generic floor, floor stop | **502–573** |
| `Stairs*` / `GenericStairs` | **575–650** |
| `Pillar_*` | **652–668** |
| `Ceiling_*` (through crush-and-raise) | **670–698** |

### vs **899**

| | Lines |
|--|------:|
| **899** | **241–470** |
| **900** (this row) | **472–700** (**229**) |
| `LS_Ceiling_LowerAndCrush` | **700+** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **229** |
| Net behavior-ported | **229** |


**Post-review ratio:** 218/229 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecFloorStairsCeilingSliceTests.GenericFloor_RaiseTargetSelectsHighest`
- `VmPLnspecFloorStairsCeilingSliceTests.GenericStairs_TogglesOnRepeatableLine`
- `VmPLnspecFloorStairsCeilingSliceTests.StairsBuildUp_UsesTicsDelay`
- `VmPLnspecFloorStairsCeilingSliceTests.CeilingCrushAndRaise_HalvesUpSpeed`
- `VmPLnspecFloorStairsCeilingSliceTests.FloorDonut_ScalesBothSpeeds`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 900 --to 900` (0 hard / 0 soft).

## Seams

`EV_DoFloor`, `EV_StartWaggle`, `EV_DoChange`, `EV_DoDonut`, `EV_StopFloor`, `EV_BuildStairs`, `EV_DoPillar`, and `EV_DoCeiling` are not hosted.

## Credit rationale

Texture/ceiling floor targets, waggle/transfer/donut, generalized floor target switch, stair flag/delay packing, repeatable generic-stair direction toggle, pillar crush mode, and ceiling value/crush argument wiring via `VmPLnspecFloorStairsCeilingSlice`.
