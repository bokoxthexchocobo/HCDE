# Chunk 738 — Drop/copy-friendliness/health pickup helpers (`p_mobj.cpp` 1045–1056, 1141–1159, 1179–1309).

## Scope

Drop/copy-friendliness/health pickup helpers (`p_mobj.cpp` 1045–1056, 1141–1159, 1179–1309).

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::DropInventory** 1045–1056 (12 lines)
- `src/playsim/p_mobj.cpp` **AActor::CopyFriendliness** 1141–1159 (19 lines)
- `src/playsim/p_mobj.cpp` **P_GetRealMaxHealth** 1179–1226 (48 lines)
- `src/playsim/p_mobj.cpp` **P_GiveBody** 1236–1301 (66 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, GiveBody)** 1303–1309 (7 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Managed

`VmPMobjInventoryGive.cs`
Managed LOC (tally): 207.
Conversion commit: `b23b14cc4aa89988ad59ab88b615c671f0257cb6`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `DropInventory` | `AActor::DropInventory` 1045–1056 |
| `CopyFriendliness` | `AActor::CopyFriendliness` 1141–1159 |
| `GetRealMaxHealth` | `P_GetRealMaxHealth` 1179–1226 |
| `GiveBody` | `P_GiveBody` 1236–1301 |
| `GiveBodyAction` | `DEFINE_ACTION_FUNCTION(AActor, GiveBody)` 1303–1309 |


**Post-review ratio:** 207/152 managed/native ≈ 136% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjInventoryGiveTests.DropInventory_VmPath_ReturnsResult`
- `VmPMobjInventoryGiveTests.CopyFriendliness_CopiesFlagsAndTarget`
- `VmPMobjInventoryGiveTests.CopyFriendliness_ResetHealth_UsesSpawnHealth`
- `VmPMobjInventoryGiveTests.GetRealMaxHealth_MaxPickupHealthCaps`
- `VmPMobjInventoryGiveTests.GetRealMaxHealth_ExplicitMaxAddsBonus`
- `VmPMobjInventoryGiveTests.GiveBody_DeadActor_ReturnsFalse`
- `VmPMobjInventoryGiveTests.GiveBody_PlayerNegativePercent_RaisesHealth`
- `VmPMobjInventoryGiveTests.GiveBodyAction_Delegates`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 738 --to 738` (0 hard / 0 soft).

## Seams

`VMCall` for `DropInventory`, `SpawnHealth`, `GetMaxHealth`, `IntVar`, morph style, and skill health factor are injected.

## Credit rationale

Inventory drop VM gate plus friendship and P_GiveBody stack as whole credited symbols.
