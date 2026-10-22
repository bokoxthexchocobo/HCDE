# Chunk 741 — Fake-floor triggers, player land, inventory find/give, CallFallAndSink (`p_mobj.cpp` 1064–1131, 3214–3284, 3334–3454).

## Scope

Fake-floor triggers, player land, inventory find/give, CallFallAndSink (`p_mobj.cpp` 1064–1131, 3214–3284, 3334–3454).

## Native ranges

- `src/playsim/p_mobj.cpp` **P_CheckFakeFloorTriggers** 3214–3275 (62 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, CheckFakeFloorTriggers)** 3277–3284 (8 lines)
- `src/playsim/p_mobj.cpp` **PlayerLandedOnThing** 3334–3347 (14 lines)
- `src/playsim/p_mobj.cpp` **AActor::CallFallAndSink** 3443–3454 (12 lines)
- `src/playsim/p_mobj.cpp` **AActor::FindInventory** 1064–1095 (32 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, FindInventory)** 1097–1103 (7 lines)
- `src/playsim/p_mobj.cpp` **AActor::GiveInventoryType** 1111–1124 (14 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, GiveInventoryType)** 1126–1131 (6 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmPMobjFakeFloorInventory.cs`
Managed LOC (tally): 166.
Conversion commit: ``.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `CheckFakeFloorTriggers` | `P_CheckFakeFloorTriggers` 3214–3275 |
| `CheckFakeFloorTriggersAction` | `DEFINE_ACTION_FUNCTION(AActor, CheckFakeFloorTriggers)` 3277–3284 |
| `PlayerLandedOnThing` | static `PlayerLandedOnThing` 3334–3347 |
| `CallFallAndSink` | `AActor::CallFallAndSink` 3443–3454 |
| `FindInventory` | `AActor::FindInventory` 1064–1095 |
| `FindInventoryAction` | `DEFINE_ACTION_FUNCTION(AActor, FindInventory)` 1097–1103 |
| `GiveInventoryType` | `AActor::GiveInventoryType` 1111–1124 |
| `GiveInventoryTypeAction` | `DEFINE_ACTION_FUNCTION(AActor, GiveInventoryType)` 1126–1131 |


**Post-review ratio:** 166/155 managed/native ≈ 107% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjFakeFloorInventoryTests.CheckFakeFloorTriggers_HitFakeFloor`
- `VmPMobjFakeFloorInventoryTests.CheckFakeFloorTriggers_SkipsWhenPredicting`
- `VmPMobjFakeFloorInventoryTests.CheckFakeFloorTriggersAction_Delegates`
- `VmPMobjFakeFloorInventoryTests.PlayerLandedOnThing_RunsHooks`
- `VmPMobjFakeFloorInventoryTests.CallFallAndSink_VirtualPath`
- `VmPMobjFakeFloorInventoryTests.CallFallAndSink_NativeFallback`
- `VmPMobjFakeFloorInventoryTests.FindInventory_ExactClassMatch`
- `VmPMobjFakeFloorInventoryTests.FindInventory_SubclassMatch`
- `VmPMobjFakeFloorInventoryTests.FindInventoryAction_Delegates`
- `VmPMobjFakeFloorInventoryTests.GiveInventoryType_PickupFailsDestroys`
- `VmPMobjFakeFloorInventoryTests.GiveInventoryTypeAction_Delegates`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 741 --to 741` (0 hard / 0 soft).

## Seams

`TriggerSectorActions`, virtual land hooks, `Spawn`/`CallTryPickup`, and `VMCall` for FallAndSink are injected.

## Credit rationale

Primary fall band was under 150; merged whole Find/Give inventory helpers and CallFallAndSink.
