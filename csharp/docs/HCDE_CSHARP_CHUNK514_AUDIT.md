# Chunk 514 — PatchWeapon whole function

## Scope

PatchWeapon whole function

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchWeapon** 2253–2437 (185 lines)

**Credited native LOC:** 185 (tally `nativeLoc` 185).

## Managed

`VmDehackedPatchWeaponDispatch.cs`
Managed LOC (tally): 286.
Conversion commit: `639dd22f0fa69d050daf76e613b2f7b040267b46`.

## Padding review (2026-10-21)

| Managed | Native (`d_dehacked.cpp`) |
|---------|-------------------|
| `IsWeaponSlotValid` / `ShouldMakeStateDefines` / `LineEndsWithFrameSuffix` / `TryMapFrameLabel` / `FrameLabelForPrefix` / `IsAmmoTypeKey` / `IsDecalKey` / `IsAmmoUseKey` / `IsMinAmmoKey` / `IsMbf21BitsKey` / `NormalizeAmmoTypeIndex` / `DecalLookupSucceeded` / `ParseMbf21NumericToken` / `TokenIsNumeric` / `ApplyMbf21Token` / `ParseMbf21Line` / `ReadyStateIsChainsaw` / `HasReadyState` / `ResolveReadySound` / `LoopContinues` / `WeaponTypeMissing` / `FormatUnknownWeaponKey` / `TryApplyWeaponFrameLine` | `PatchWeapon` 2253–2437 (2261: `if (weapNum >= 0 && weapNum < 9 && (unsigned)weapNum < WeaponNames.Si…`; 2264: `if (type != NULL)`; 2271: `if (type == NULL)`) |
| `ClassifyWeaponPatchLine` / `ProcessWeaponPatchLine` / `WeaponPatchSession` / `DecodeMbf21WeaponBits` / `FinalizePatchWeapon` | main loop **2277–407**, MBF21 **2352–400**, tail **2409–434** |

### Out-of-body uncredit (514)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **185** |
| **Out-of-body removed** | **0** |
| **Honest** | **185** |


**Post-review ratio:** 286/185 managed/native ≈ 155% (src file LOC at HEAD; see tally).

## Tests

- `VmDehackedPatchWeaponDispatchTests.IsWeaponSlotValid_AcceptsInRange`
- `VmDehackedPatchWeaponDispatchTests.FrameLabelForPrefix_SwapsSelectDeselect`
- `VmDehackedPatchWeaponDispatchTests.NormalizeAmmoTypeIndex_ClampsHigh`
- `VmDehackedPatchWeaponDispatchTests.ParseMbf21Line_CombinesNumericAndName`
- `VmDehackedPatchWeaponDispatchTests.ResolveReadySound_ChainsawUsesIdle`
- `VmDehackedPatchWeaponDispatchTests.TryApplyWeaponFrameLine_RecordsBobbingReady`
- `VmDehackedPatchWeaponDispatchTests.ClassifyWeaponPatchLine_RecognizesMbf21`
- `VmDehackedPatchWeaponDispatchTests.DecodeMbf21WeaponBits_ClearsKickbackOnNegativeBit`
- `VmDehackedPatchWeaponDispatchTests.ProcessWeaponPatchLine_SetsPatchedStatesOnFrame`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 514 --to 514` (0 hard / 0 soft).

## Seams

`GetLine`, `FindState`, `statedef.InstallStates`, and `DecalLibrary` are not hosted.

## Credit rationale

Credits DeHackEd weapon frame remapping, ammo/decal/MBF21 dispatch, and chainsaw ready-sound tail from `d_dehacked.cpp` (2253–2437).
