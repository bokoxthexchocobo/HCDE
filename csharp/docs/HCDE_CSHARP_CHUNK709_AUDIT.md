# Chunk 709 — Whole `P_TryMove` body part 1 (`p_map.cpp` 2368–2615); portal/spechit tail in chunk713.

## Scope

Whole `P_TryMove` body part 1 (`p_map.cpp` 2368–2615); portal/spechit tail in chunk713.

## Native ranges

- `src/playsim/p_map.cpp` **P_TryMove** 2368–2615 (248 lines)

**Credited native LOC:** 248 (tally `nativeLoc` 248).

## Managed

`VmPMapTryMove.cs`
Managed LOC (tally): 368.
Conversion commit: `4370842eba9b9cc6d984c994f840f842d87f5882`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `TryMove` | `P_TryMove` 2368–2615 |
| `HeightCrush` / `MustLowerToFit` / `FlyCeilingBlocks` / `MissileStepBlocked` / `StepUpTooHigh` | 2437–2491 |
| `DropoffBlocked` / `DropoffCompatDisables` / `LargeDogDropoffCancel` | 2514–2547 |
| `CantLeaveFloorPic` / `BotDangerMoveBlocked` / `MbfBouncerStepBlocked` | 2561–2610 |
| `PassMobjBlocked` / `ShouldCollideWhenBlocked` | 2397–2420 |


**Post-review ratio:** 368/248 managed/native ≈ 148% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapTryMoveTests.TryMove_RejectsHeightCrush`
- `VmPMapTryMoveTests.TryMove_AllowsFloatOkWhenOpen`
- `VmPMapTryMoveTests.TryMove_BlocksPassMobjWhenCompat`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 709 --to 709` (0 hard / 0 soft).

## Seams

`P_CheckPosition`, `P_TestMobjZ`, `P_CollidedWith`, portal translation, `P_ActivateLine`, and sector actions are injected.

## Credit rationale

Primary try-move validation (height, step, dropoff, bot danger); retires recognition `P_TryMove` band.
