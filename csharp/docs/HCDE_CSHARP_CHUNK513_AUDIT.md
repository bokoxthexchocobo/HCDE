# Chunk 513 — PatchMisc

## Scope

PatchMisc

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchMisc** 2537–2729 (193 lines)

**Credited native LOC:** 193 (tally `nativeLoc` 193).

## Managed

`VmDehackedPatchMisc.cs`
Managed LOC (tally): 99.
Conversion commit: `4ab8b77471bd0e6c2257a9002d96943f6e93109a`.

## Padding review (2026-10-21)

| Managed | Native (`d_dehacked.cpp`) |
|---------|-------------------|
| `LineReady` / `BfgCellsKey` / `RocketStyleKey` / `RocketAlphaKey` / `MonstersInfightKey` / `MonstersIgnoreKey` / `PowerupColorPrefix` / `PowerupColorSuffix` / `UnknownMiscLine` / `InfightingFromIgnore` / `ExplosionStyleFound` / `ParseExplosionAlpha` / `PowerupColorValid` / `NoAutofreezeKey` / `StartHealthKey` / `MaxSoulsphereKey` / `GodModeHealthKey` / `IdfaArmorKey` / `IdkfaArmorClassKey` / `GreenArmorClassKey` / `BlueArmorClassKey` / `MegasphereHealthKey` / `ParseIntField` / `ShouldContinueMiscLoop` / `ApplyHandledMisc` / `PowerupColorRgbaParsed` / `UnknownPowerupName` / `DehInfoOffsetValid` / `SoulsphereHealthKey` / `InitialBulletsKey` / `MaxArmorKey` / `MaxHealthKey` / `ParseMiscInt` | `PatchMisc` 2537–2730 (2563: `if (HandleKey (keys, &deh, Line1, atoi (Line2)))`; 2565: `if (stricmp (Line1, "BFG Cells/Shot") == 0)`; 2569: `else if (stricmp (Line1, "Rocket Explosion Style") == 0)`) |

### Out-of-body uncredit (513)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **194** |
| **Out-of-body removed** | **1** |
| **Honest** | **193** |


**Post-review ratio:** 99/193 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmDehackedPatchMiscTests.LineReady_WhenGetLineReturnsOne`
- `VmDehackedPatchMiscTests.BfgCellsKey_Matches`
- `VmDehackedPatchMiscTests.InfightingIgnore_FlipsSign`
- `VmDehackedPatchMiscTests.PowerupPrefix_Detected`
- `VmDehackedPatchMiscTests.ExplosionStyle_InvalidWhenNegative`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 513 --to 513` (0 hard / 0 soft).

## Seams

`GetLine`, `HandleKey`, `FindStyle`, `V_SetBlend`, and `types[]` class binding are not hosted.

## Credit rationale

Credits DeHackEd miscellaneous section key dispatch, infighting, explosion style/alpha, and powerup color parsing from `d_dehacked.cpp` (2537–2730).
