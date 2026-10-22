# Chunk 715 — Whole `P_CheckMove` (2899–2997), `P_FakeZMovement` (2256–2291), and `CheckForPushSpecial` (2299–2358).

## Scope

Whole `P_CheckMove` (2899–2997), `P_FakeZMovement` (2256–2291), and `CheckForPushSpecial` (2299–2358).

## Native ranges

- `src/playsim/p_map.cpp` **P_FakeZMovement** 2256–2291 (36 lines)
- `src/playsim/p_map.cpp` **CheckForPushSpecial** 2299–2358 (60 lines)
- `src/playsim/p_map.cpp` **P_CheckMove** 2899–2997 (99 lines)

**Credited native LOC:** 195 (tally `nativeLoc` 195).

## Managed

`VmPMapCheckMoveFakePush.cs`
Managed LOC (tally): 254.
Conversion commit: `7ef89c8cab60576753ecfd17cf600a6e0a028996`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `CheckMove` / `ShouldContinueAfterFailedPosition` / `ResolveNewZ` | `P_CheckMove` 2899–2997 |
| `CeilingBlocksMove` / `FloorStepGate` / `DropoffBlocks` / `FloorPicChanged` | 2933–2987 |
| `FakeZMovement` | `P_FakeZMovement` 2256–2291 |
| `CheckForPushSpecial` / `PushWindowAllowsPass` | `CheckForPushSpecial` 2299–2358 |


**Post-review ratio:** 254/195 managed/native ≈ 130% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapCheckMoveFakePushTests.FakeZMovement_ClipsToFloor`
- `VmPMapCheckMoveFakePushTests.CheckForPushSpecial_ActivatesPushWall`
- `VmPMapCheckMoveFakePushTests.CheckMove_BlocksLowCeiling`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 715 --to 715` (0 hard / 0 soft).

## Seams

`P_CheckPosition`, `P_TestMobjZ`, `P_ActivateLine`, `P_GetMidTexturePosition`, and 3D-floor iteration remain native.

## Credit rationale

Polyobject crush checks and push/impact line activation hosted as faithful helper ports.
