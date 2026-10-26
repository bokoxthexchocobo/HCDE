# Chunk 513 — PatchMisc

## Scope

PatchMisc

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchMisc** 2537–2729 (193 lines)

**Credited native LOC:** 193 (tally `nativeLoc` 193).

## Managed

`VmDehackedPatchMisc.cs`
Managed LOC (tally): 290.
Conversion commit: `14e31e81fcc962a1a5288a0e704001473333f73f`.

## Padding review (2026-10-21)

| Managed | Native (`d_dehacked.cpp`) |
|---------|-------------------|
| `DehMiscInfo` / `CreateMiscStructKeys` / `ProcessMiscLine` / `RunPatchMiscLoop` | `PatchMisc` 2537–2730 (2539–2556: keys[]; 2561–2657: GetLine loop + HandleKey/specials; 525–535: `HandleKey`) |
| `FillMiscDefaultPatchPlan` / `FinalizeMiscInfighting` / `MiscDefaultPatchPlan` | 2659–2726: armor/health/player defaults + infighting normalize |
| Key predicates / `ParseIntField` / infighting + explosion helpers | 2565–2589: BFG/rocket/infight lines; 2578–2580: alpha |
| Powerup blend via `VmDehackedPatchMiscPowerups` + `PowerupBlendTypeNames` | 2590–2650: powerup color names + RGBA |

### Out-of-body uncredit (513)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **194** |
| **Out-of-body removed** | **1** |
| **Honest** | **193** |


**Post-review ratio:** 290/193 managed/native ≈ 150% (src file LOC at HEAD; see tally).

## Tests

- `VmDehackedPatchMiscTests.LineReady_WhenGetLineReturnsOne`
- `VmDehackedPatchMiscTests.BfgCellsKey_Matches`
- `VmDehackedPatchMiscTests.InfightingIgnore_FlipsSign`
- `VmDehackedPatchMiscTests.PowerupPrefix_Detected`
- `VmDehackedPatchMiscTests.ExplosionStyle_InvalidWhenNegative`
- `VmDehackedPatchMiscTests.ProcessMiscLine_AppliesStructKey_InitialHealth`
- `VmDehackedPatchMiscTests.ProcessMiscLine_BfgCells_WhenNotInStructTable`
- `VmDehackedPatchMiscTests.RunPatchMiscLoop_StopsWhenGetLineNotOne`
- `VmDehackedPatchMiscTests.FinalizeMiscInfighting_NormalizesDdAlias`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 513 --to 513` (0 hard / 0 soft).

## Seams

`GetLine`, `HandleKey`, `FindStyle`, `V_SetBlend`, and `types[]` class binding are not hosted.

## Credit rationale

Credits DeHackEd miscellaneous section key dispatch, infighting, explosion style/alpha, and powerup color parsing from `d_dehacked.cpp` (2537–2730).
