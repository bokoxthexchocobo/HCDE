# Chunk 709 — Whole `P_TryMove` body (`p_map.cpp` 2368–2882). Wrapper overload 2884–2890 excluded.

## Scope

Whole `P_TryMove` body (`p_map.cpp` 2368–2882). Wrapper overload 2884–2890 excluded.

## Native ranges

- `src/playsim/p_map.cpp` **P_TryMove** 2368–2882 (515 lines)

**Credited native LOC:** 515 (tally `nativeLoc` 515).

## Managed

`VmPMapTryMove.cs`
Managed LOC (tally): 418.
Conversion commit: `b5445d1ac4eb37d8def3c35f2aee01388c17040e`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `TryMove` (portal loop, commit, spechit cross, heightsec, `portalstep` relink) | `P_TryMove` 2368–2882 |
| `ShouldCollideWhenBlocked` / `PassMobjBlocked` | 2391–2420 |
| `HeightCrush` / `MustLowerToFit` / `FlyCeilingBlocks` / `MissileStepBlocked` / `StepUpTooHigh` | 2437–2491 |
| `DropoffBlocked` / `DropoffCompatDisables` / `LargeDogDropoffCancel` | 2514–2547 |
| `CantLeaveFloorPic` / `BotDangerMoveBlocked` / `MbfBouncerStepBlocked` | 2561–2610 |
| pushline spechit / blasted damage | 2854–2880 |


**Post-review ratio:** 418/515 managed/native ≈ 81% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapTryMoveTests.TryMove_RejectsHeightCrush`
- `VmPMapTryMoveTests.TryMove_AllowsFloatOkWhenOpen`
- `VmPMapTryMoveTests.TryMove_BlocksPassMobjWhenCompat`
- `VmPMapTryMoveTests.TryMove_BlocksDropoff`
- `VmPMapTryMoveTests.TryMove_RunsSpechitCrossAfterPortalOk`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 709 --to 709` (0 hard / 0 soft).

## Seams

`P_CheckPosition`, `P_TestMobjZ`, `P_CollidedWith`, portal translation, `P_ActivateLine`, `CheckForPushSpecial`, and sector actions are injected.

## Credit rationale

Single-function oversize exception (515 native LOC): must not split across chunks.
