# Chunk 514 — PatchWeapon whole function

## Scope

PatchWeapon whole function

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchWeapon** 2253–2437 (185 lines)

**Credited native LOC:** 185 (tally `nativeLoc` 185).

## Managed

`VmDehackedPatchWeaponDispatch.cs`
Managed LOC (tally): 111.
Conversion commit: `a254c4c33c0d8f53ac55ea29ff1f1760482d673d`.

## Padding review (2026-10-21)

| Managed | Native (`d_dehacked.cpp`) |
|---------|-------------------|
| `IsWeaponSlotValid` / `ShouldMakeStateDefines` / `LineEndsWithFrameSuffix` / `TryMapFrameLabel` / `FrameLabelForPrefix` / `IsAmmoTypeKey` / `IsDecalKey` / `IsAmmoUseKey` / `IsMinAmmoKey` / `IsMbf21BitsKey` / `NormalizeAmmoTypeIndex` / `DecalLookupSucceeded` / `ParseMbf21NumericToken` / `TokenIsNumeric` / `ApplyMbf21Token` / `ParseMbf21Line` / `ReadyStateIsChainsaw` / `HasReadyState` / `ResolveReadySound` / `LoopContinues` / `WeaponTypeMissing` / `FormatUnknownWeaponKey` / `TryApplyWeaponFrameLine` | `PatchWeapon` 2253–2437 (2261: `if (weapNum >= 0 && weapNum < 9 && (unsigned)weapNum < WeaponNames.Si…`; 2264: `if (type != NULL)`; 2271: `if (type == NULL)`) |

### Out-of-body uncredit (514)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **185** |
| **Out-of-body removed** | **0** |
| **Honest** | **185** |


**Post-review ratio:** 111/185 managed/native ≈ 60% (src file LOC at HEAD; see tally).

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
