# Chunk 583 — A_Warp inventory priming and missing destination ref (3707–3730)

## Scope

A_Warp inventory priming and missing destination ref (3707–3730)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_Warp** 3689–3730 (42 lines)

**Credited native LOC:** 42 (tally `nativeLoc` 42).

## Managed

`VmActionWarp.cs`
Managed LOC (tally): 23.
Conversion commit: `7d03596bc356b0307a2e8b419a2b3859e2a9f462`.

## Padding review (2026-10-21)

**Fidelity review:** Credits `3707–3715`, `3717–3724`, `3727–3730`. Offset math and warp move are seams.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `PrimeWarpChainResult` / `ClearWarpStatePointer` | 3707–3715 |
| `UseTidLookup` | 3717–3724 |
| `WarpFailsWithoutReference` | 3727–3730 |

**Post-review ratio:** 23/42 managed/native ≈ 55% (src file LOC at HEAD; see tally).

## Tests

- `VmActionWarpTests.WarpFailsWithoutReference`
- `VmActionWarpTests.UseTidLookup_WhenFlagged`
- `VmActionWarpTests.PrimeWarpChainResult_WhenSlotPresent`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 583 --to 583` (0 hard / 0 soft).

## Seams

`COPY_AAPTR`, `SingleActorFromTID`, and `P_Thing_Warp` are not hosted.

## Credit rationale

Credits dual return priming, TID selector branch flag, and null reference early exit.
