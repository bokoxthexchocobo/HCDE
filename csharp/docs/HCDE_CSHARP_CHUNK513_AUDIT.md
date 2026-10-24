# Chunk 513 — PatchMisc

## Scope

PatchMisc

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchMisc** 2537–2730 (194 lines)

**Credited native LOC:** 194 (tally `nativeLoc` 194).

## Managed

`VmDehackedPatchMisc.cs`, `VmDehackedPatchMiscTests.cs`, `VmPUserSerializeFreeze.cs`, `VmPUserSerializeFreezeTests.cs`, `VmPUserUnpredictBoss.cs`, `VmPUserUnpredictBossTests.cs`
Managed LOC (tally): 108.
Conversion commit: `93371e41cb09bd927dcc8d20a094fc12f7d40dd7`.

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
