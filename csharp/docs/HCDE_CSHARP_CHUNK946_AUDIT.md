# Chunk 946 — `DLevelScript::RunScript` `PCD_SETGRAVITY` through `PCD_CHECKACTORINVENTORY` head from `p_acs.cpp` (**9600–9749**). Follows **945** (**≤8849**); does not hull **8850–8999** (credited); outside deferred **7366–7509**.

## Scope

`DLevelScript::RunScript` `PCD_SETGRAVITY` through `PCD_CHECKACTORINVENTORY` head from `p_acs.cpp` (**9600–9749**). Follows **945** (**≤8849**); does not hull **8850–8999** (credited); outside deferred **7366–7509**.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::RunScript** 9600–9749 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsVmOpcode946Slice.cs`
Managed LOC (tally): 101.
Conversion commit: `55a12e2dac9a679f767f16ce909063a1e836ea1a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native opcode region | Behavior ported |
|----------------------|-----------------|
| `PCD_SETGRAVITY` / `DIRECT` (**9600–9607**) | yes |
| `PCD_SETAIRCONTROL` / `DIRECT` + notify (**9610–9619**) | yes |
| `PCD_SPAWN` / `DIRECT` (**9622–9629**) | yes |
| `PCD_SPAWNSPOT` / `DIRECT` / `FACING` (**9632–9644**) | yes |
| `PCD_CLEARINVENTORY` / `CLEARACTORINVENTORY` (**9647–9665**) | yes |
| `PCD_GIVEINVENTORY` / `GIVEACTORINVENTORY` / `DIRECT` (**9668–9702**) | yes |
| `PCD_TAKEINVENTORY` / `TAKEACTORINVENTORY` / `DIRECT` (**9705–9739**) | yes |
| `PCD_CHECKINVENTORY` / `CHECKACTORINVENTORY` head (**9742–9749**) | yes |

| Managed (`VmPAcsVmOpcode946Slice.cs`) | Native (`p_acs.cpp`) |
|----------------------------------------|---------------------|
| `LevelScalarFromFixed` / `PopAfterUnarySet` | **9600–9619** |
| `SpawnResultSlot` / `ApplySpawnStackPop` / direct PC counts | **9622–9644** |
| `ClearActorInventoryUsesWorldTarget` | **9652–9664** |
| `ActorInventoryUsesActivatorTid` / stack pops | **9676–9731** |
| `InventoryDirectPcWords` | **9697–9738** |
| `CheckInventoryReplacesTop` / `CheckActorInventory*` | **9742–9749** |
| `TriggersAirControlChanged` | **9613–9619** |

### Chunk **946** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**945** |


**Post-review ratio:** 101/150 managed/native ≈ 67% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsVmOpcode946SliceTests.LevelScalarFromFixed_ConvertsGravity`
- `VmPAcsVmOpcode946SliceTests.ApplySpawnStackPop_ReducesByFive`
- `VmPAcsVmOpcode946SliceTests.SpawnResultSlot_PointsAtFirstArg`
- `VmPAcsVmOpcode946SliceTests.ClearActorInventoryUsesWorldTarget_WhenTidZero`
- `VmPAcsVmOpcode946SliceTests.GiveActorInventoryUsesActivator_WhenTidZero`
- `VmPAcsVmOpcode946SliceTests.CheckActorInventoryStackIndices_UseTopTwoSlots`
- `VmPAcsVmOpcode946SliceTests.TriggersAirControlChanged_ForSetAirControlOpcodes`
- `VmPAcsVmOpcode946SliceTests.SpawnUsesNonForcedPlacement_MatchesNativeDoSpawn`
- `VmPAcsVmOpcode946SliceTests.ApplyCheckActorInventoryStackPop_DropsOneSlot`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 946 --to 946` (0 hard / 0 soft).

## Seams

`DoSpawn`, `DoSpawnSpot`, `ScriptUtil::Exec`, `CheckInventory`, and `Level->AirControlChanged` remain host seams.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmPAcsVmOpcode946Slice.cs` (`AcsVm` opcode handlers are not matrix credit).
