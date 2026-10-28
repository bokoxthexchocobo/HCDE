# Chunk 713 — `P_TryMove` tail 2616–2882 plus `P_TestMobjLocation`, `P_FakeZMovement`, `CheckForPushSpecial` (`p_map.cpp`).

## Scope

`P_TryMove` tail 2616–2882 plus `P_TestMobjLocation`, `P_FakeZMovement`, `CheckForPushSpecial` (`p_map.cpp`).

## Native ranges

- `src/playsim/p_map.cpp` **P_TryMove** 2616–2882 (267 lines)
- `src/playsim/p_map.cpp` **P_TestMobjLocation** 2051–2068 (18 lines)
- `src/playsim/p_map.cpp` **P_FakeZMovement** 2256–2291 (36 lines)
- `src/playsim/p_map.cpp` **CheckForPushSpecial** 2299–2358 (60 lines)

**Credited native LOC:** 381 (tally `nativeLoc` 381).

## Managed

`VmPMapMobjLocationFakeZ.cs`
Managed LOC (tally): 196.
Conversion commit: ``.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `TryMoveAfterPortals` / `PortalCrossSide` / `PortalGroupMatches` | `P_TryMove` 2616–2882 |
| `TestMobjLocation` | `P_TestMobjLocation` 2051–2068 |
| `FakeZMovement` | `P_FakeZMovement` 2256–2291 |
| `CheckForPushSpecial` / `WindowAllowsPush` / `MidtexBlocks` | 2299–2358 |


**Post-review ratio:** 196/381 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapTryMoveTests.TryMoveAfterPortals_RunsSpechitCross`
- `VmPMapMobjLocationFakeZTests.PortalCrossSide_DetectsForwardCross`
- `VmPMapMobjLocationFakeZTests.TestMobjLocation_AcceptsWhenZInBounds`
- `VmPMapMobjLocationFakeZTests.FakeZMovement_ClipsToFloor`
- `VmPMapMobjLocationFakeZTests.CheckForPushSpecial_ActivatesPushWall`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 713 --to 713` (0 hard / 0 soft).

## Seams

Portal iteration, `TryMoveAfterPortals`, `P_CheckPosition`, and `P_ActivateLine` remain injected at seams.

## Credit rationale

Completes try-move portal/spechit/pushline path; Z probe and push-special window logic without duplicating chunk704 `P_TestMobjZ`.
