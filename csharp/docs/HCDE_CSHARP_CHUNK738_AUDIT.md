# Chunk 738 — Drop, give-type spawn, friendship, and health pickup (`p_mobj.cpp` 1045–1056, 1111–1124, 1141–1159, 1179–1301).

## Scope

Drop, give-type spawn, friendship, and health pickup (`p_mobj.cpp` 1045–1056, 1111–1124, 1141–1159, 1179–1301).

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::DropInventory** 1045–1056 (12 lines)
- `src/playsim/p_mobj.cpp` **AActor::GiveInventoryType** 1111–1124 (14 lines)
- `src/playsim/p_mobj.cpp` **AActor::CopyFriendliness** 1141–1159 (19 lines)
- `src/playsim/p_mobj.cpp` **P_GetRealMaxHealth** 1179–1226 (48 lines)
- `src/playsim/p_mobj.cpp` **P_GiveBody** 1236–1301 (66 lines)

**Credited native LOC:** 159 (tally `nativeLoc` 159).

## Managed

`VmPMobjInventoryGive.cs`
Managed LOC (tally): 222.
Conversion commit: `b23b14cc4aa89988ad59ab88b615c671f0257cb6`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `DropInventory` | `AActor::DropInventory` 1045–1056 |
| `GiveInventoryType` | `AActor::GiveInventoryType` 1111–1124 |
| `CopyFriendliness` | `AActor::CopyFriendliness` 1141–1159 |
| `GetRealMaxHealth` | `P_GetRealMaxHealth` 1179–1226 |
| `GiveBody` | `P_GiveBody` 1236–1301 |


**Post-review ratio:** 222/159 managed/native ≈ 140% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjInventoryGiveTests.DropInventory_VmPath_ReturnsResult`
- `VmPMobjInventoryGiveTests.GiveInventoryType_PickupFailureDestroys`
- `VmPMobjInventoryGiveTests.CopyFriendliness_CopiesFlagsAndTarget`
- `VmPMobjInventoryGiveTests.CopyFriendliness_ResetHealth_UsesSpawnHealth`
- `VmPMobjInventoryGiveTests.GetRealMaxHealth_MaxPickupHealthCaps`
- `VmPMobjInventoryGiveTests.GetRealMaxHealth_ExplicitMaxAddsBonus`
- `VmPMobjInventoryGiveTests.GiveBody_DeadActor_ReturnsFalse`
- `VmPMobjInventoryGiveTests.GiveBody_PlayerNegativePercent_RaisesHealth`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 738 --to 738` (0 hard / 0 soft).

## Seams

`VMCall` for `DropInventory`, `Spawn`/`CallTryPickup`, `SpawnHealth`, morph style, and skill health factor are injected.

## Credit rationale

Whole actor inventory/health helpers; excludes VM DEFINE wrappers.
