# Chunk 715 — Whole `P_CheckMove` (2899–2997), `P_FakeZMovement` (2256–2291), and `CheckForPushSpecial` (2299–2358).

## Scope

Whole `P_CheckMove` (2899–2997), `P_FakeZMovement` (2256–2291), and `CheckForPushSpecial` (2299–2358).

## Native ranges

- `src/playsim/p_map.cpp` **P_FakeZMovement** 2256–2291 (36 lines)
- `src/playsim/p_map.cpp` **CheckForPushSpecial** 2299–2358 (60 lines)
- `src/playsim/p_map.cpp` **P_CheckMove** 2899–2991 (93 lines)
- `src/playsim/p_map.cpp` **P_CheckMove** 2993–2997 (5 lines)

**Credited native LOC:** 194 (tally `nativeLoc` 194).

## Managed

`VmPMapCheckMoveFakePush.cs`
Managed LOC (tally): 265.
Conversion commit: `8ac29af84227e4d30a40234662e0655fe6ddcf05`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `CheckMove` / `ShouldContinueAfterFailedPosition` / `ResolveNewZ` | `P_CheckMove` 2899–2997 |
| `CeilingBlocksMove` / `FloorStepGate` / `DropoffBlocks` / `FloorPicChanged` | 2933–2987 |
| `FakeZMovement` / `Distance2D` / `TargetCenter` | `P_FakeZMovement` 2256–2291 |
| `CheckForPushSpecial` / `PushWindowAllowsPass` | `CheckForPushSpecial` 2299–2358 |

### Out-of-body uncredit (715)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **195** |
| **Out-of-body removed** | **1** |
| **Honest** | **194** |


**Post-review ratio:** 265/194 managed/native ≈ 137% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapCheckMoveFakePushTests.FakeZMovement_ClipsToFloor`
- `VmPMapCheckMoveFakePushTests.FakeZMovement_FloatsTowardTarget`
- `VmPMapCheckMoveFakePushTests.CheckForPushSpecial_ActivatesPushWall`
- `VmPMapCheckMoveFakePushTests.CheckForPushSpecial_ActivatesImpact`
- `VmPMapCheckMoveFakePushTests.CheckMove_BlocksLowCeiling`
- `VmPMapCheckMoveFakePushTests.CheckMove_AllowsWhenNoclip`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 715 --to 715` (0 hard / 0 soft).

## Seams

`P_CheckPosition`, `P_TestMobjZ`, `P_ActivateLine`, `P_GetMidTexturePosition`, and 3D-floor iteration remain native.

## Credit rationale

Polyobject crush checks and push/impact line activation hosted as faithful helper ports.
