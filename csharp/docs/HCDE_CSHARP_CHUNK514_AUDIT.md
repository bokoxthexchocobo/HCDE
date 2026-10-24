# Chunk 514 — PatchWeapon whole function

## Scope

PatchWeapon whole function

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchWeapon** 2253–2437 (185 lines)

**Credited native LOC:** 185 (tally `nativeLoc` 185).

## Managed

`VmDehackedPatchWeaponDispatchTests.cs`
Managed LOC (tally): 113.
Conversion commit: `f3af28f4d352f63d8c4e2c72f90f958a5b69b899`.

## Tests

- `VmDehackedPatchWeaponDispatchTests.IsWeaponSlotValid_AcceptsInRange`
- `VmDehackedPatchWeaponDispatchTests.FrameLabelForPrefix_SwapsSelectDeselect`
- `VmDehackedPatchWeaponDispatchTests.NormalizeAmmoTypeIndex_ClampsHigh`
- `VmDehackedPatchWeaponDispatchTests.ParseMbf21Line_CombinesNumericAndName`
- `VmDehackedPatchWeaponDispatchTests.ResolveReadySound_ChainsawUsesIdle`
- `VmDehackedPatchWeaponDispatchTests.TryApplyWeaponFrameLine_RecordsBobbingReady`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 514 --to 514` (0 hard / 0 soft).

## Seams

`GetLine`, `FindState`, `statedef.InstallStates`, and `DecalLibrary` are not hosted.

## Credit rationale

Credits DeHackEd weapon frame remapping, ammo/decal/MBF21 dispatch, and chainsaw ready-sound tail from `d_dehacked.cpp` (2253–2437).
