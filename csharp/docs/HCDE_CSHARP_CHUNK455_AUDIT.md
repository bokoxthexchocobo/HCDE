# Chunk 455 — PIT_CheckThing movement blocking and pickup

## Scope

PIT_CheckThing movement blocking and pickup

## Native ranges

- `src/playsim/p_map.cpp` **PIT_CheckThing** 1401–1434 (34 lines)
- `src/playsim/p_map.cpp` **PIT_CheckThing** 1443–1488 (46 lines)
- `src/playsim/p_map.cpp` **PIT_CheckThing** 1493–1513 (21 lines)
- `src/playsim/p_map.cpp` **PIT_CheckThing** 1528–1538 (11 lines)
- `src/playsim/p_map.cpp` **PIT_CheckThing** 1568–1574 (7 lines)
- `src/playsim/p_map.cpp` **PIT_CheckThing** 1598–1728 (131 lines)
- `src/playsim/p_map.cpp` **PIT_CheckThing** 1758–1777 (20 lines)

**Credited native LOC:** 270 (tally `nativeLoc` 270).

## Managed

`VmPMapPitCheckThing.cs`
Managed LOC (tally): 365.
Conversion commit: `bf47435fabf60699f5debabd004a8e92f263e600`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `SkipSelf` / `ThruActorsPass` / `ThruBitsAllow` | 1401–1408 |
| `NonSolidNonInteractive` | 1422–1424 |
| `BlockDistanceClear` / `SameSpeciesPass` | 1426–1431 |
| `EvaluateEarlyGates` | 1401–1434 |
| `BridgeStepMonster` | 1448–1451 |
| `UnblockWhenMovingApart` / `PassobjVerticalOverlap` | 1482–1485 |
| `EvaluateBridgeAndUnblock` | 1443–1488 |
| `PassobjOverlap` / `PickupOverUnder` / `EvaluatePassobjVertical` | 1493–1513 |
| `DummySwitchOverlap` | 1507–1512 |
| `TouchyDetonate` | 1528–1538 |
| `BumpSpecialPlayer` / `BumpSpecialThrottle` | 1549–1555 |
| `PredictionSolidOnly` | 1568–1574 |
| `BlastedMonsterShove` / `BlastedDamageMass` | 1578–1593 |
| `MissileOrMbfBouncer` | 1598 |
| `NonshootableSolid` / `GhostPass` / `MthruSpecies` / `RipperSkipsCorpse` | 1601–1619 |
| `ProjectileClipHeight` / `OverObject` / `UnderObject` | 1630–1650 |
| `BounceActorZeroDamage` | 1656–1659 |
| `EvaluateMissilePath` | 1598–1728 |
| `SpectralMismatch` / `DoRipping` / `RipperDamageDue` / `BossRipBlocked` | 1688–1714 |
| `ReflectiveTurnSeeker` / `PushablePushed` / `PushFactorApply` | 1702–1726 |
| `SolidBlock` / `PickupTouch` / `MaxStepPickupGate` / `EvaluateSolidAndPickup` | 1758–1777 |
| `KilloughPassobj` / `NoclipPassSolid` / `ReturnPassSolid` | 1772–1777 |
| `FloatMissileSkullfly` | 1445–1446 |
| `MonsterWalkBridge` | 1446–1450 |
| `ChangeSwitchOnUse` / `ClearSpecialOnSuccess` | 1554–1555 (`P_ActivateThingSpecial`, `lastbump`) |
| `DoorMonsterSlow` / `SecretDoorBlocks` / `LaxMonsterSwitch` / `WalkingCrossExpands` / `UseThroughLine` / `MissilePromoteTeleport` / `PlayerUseBack` / `IceCorpseBridge` / `ExpandAnyCross` / `WalkingCrossRequiresFloor` | not isolated in credited `PIT_CheckThing` spans (managed guards only) |

### Out-of-body uncredit (455)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **270** |
| **Out-of-body removed** | **0** |
| **Honest** | **270** |


**Post-review ratio:** 365/270 managed/native ≈ 135% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapPitCheckThingTests.EarlyGates_SkipWhenNonInteractive`
- `VmPMapPitCheckThingTests.RipperSkipsCorpse`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 455 --to 455` (0 hard / 0 soft).

## Seams

`P_CanCollideWith`, `P_DoMissileDamage`, and VM rumble hooks are not hosted.

## Credit rationale

Credits early filters, bridge/unblock, passobj vertical, touchy detonation, prediction solid gate, missile rip/reflect paths, and solid/pickup tail ported in `VmPMapPitCheckThing`.
