# Chunk 815 — `HCDEClientPredictsLocalWeapon`, `P_ShouldTickWeaponPSprites`, `Net_ShouldTickWeaponPSprites`, and `FPhysicsLinkBackup` from `p_user.cpp` (112–386).

## Scope

`HCDEClientPredictsLocalWeapon`, `P_ShouldTickWeaponPSprites`, `Net_ShouldTickWeaponPSprites`, and `FPhysicsLinkBackup` from `p_user.cpp` (112–386).

## Native ranges

- `src/playsim/p_user.cpp` **HCDEClientPredictsLocalWeapon** 112–119 (8 lines)
- `src/playsim/p_user.cpp` **P_ShouldTickWeaponPSprites** 126–139 (14 lines)
- `src/playsim/p_user.cpp` **DEFINE_ACTION_FUNCTION(DObject, Net_ShouldTickWeaponPSprites)** 141–147 (7 lines)
- `src/playsim/p_user.cpp` **FPhysicsLinkBackup::FPhysicsLinkBackup** 218–261 (44 lines)
- `src/playsim/p_user.cpp` **FPhysicsLinkBackup::Restore** 263–386 (124 lines)

**Credited native LOC:** 197 (tally `nativeLoc` 197).

## Managed

`VmPUserHcdeWeaponPhysics.cs`
Managed LOC (tally): 288.
Conversion commit: `53e1dd596778ccffe8ce89a5a795b595a8c4baa6`.

## Padding review (2026-10-21)

| Managed | Native (`p_user.cpp`) |
|---------|----------------------|
| `ClientPredictsLocalWeapon` | **112–119** |
| `ShouldTickWeaponPsprites` | **126–139** |
| `NetShouldTickWeaponPsprites` | **141–147** |
| `PhysicsLinkBackup.CaptureFromTouching` / `IndexOnSectorThingList` | **218–261** |
| `PhysicsLinkBackup.Restore` / `RelinkSectorThingNode` / `RelinkActorSectorThingList` / `RelinkBlockmapNode` | **263–386** |

### Out-of-body uncredit (815)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **197** |
| **Out-of-body removed** | **0** |
| **Honest** | **197** |


**Post-review ratio:** 288/197 managed/native ≈ 146% (src file LOC at HEAD; see tally).

## Tests

- `VmPUserHcdeWeaponPhysicsTests.ClientPredictsLocalWeapon_RequiresDedicatedClientSlot`
- `VmPUserHcdeWeaponPhysicsTests.ShouldTickWeaponPsprites_SuppressesAuthoritativeWhenPredicting`
- `VmPUserHcdeWeaponPhysicsTests.ShouldTickWeaponPsprites_TicksOnNewPredictTic`
- `VmPUserHcdeWeaponPhysicsTests.CaptureFromTouching_RecordsSectorIndices`
- `VmPUserHcdeWeaponPhysicsTests.IndexOnSectorThingList_FindsThingPosition`
- `VmPUserHcdeWeaponPhysicsTests.Restore_RelinksTouchingSectorNodeAfterReorder`
- `VmPUserHcdeWeaponPhysicsTests.Restore_DropsTouchingNodeFromUnknownSector`
- `VmPUserHcdeWeaponPhysicsTests.Restore_RelinksActorInSectorThingList`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 815 --to 815` (0 hard / 0 soft).

## Seams

`GetActorIterator`, live blockmap nodes, and `P_DelSecnode` are injected; rollback host supplies sector head map.

## Credit rationale

First tally credit for HCDE predicted weapon psprite gating and physics relink snapshot used during prediction rewind.
