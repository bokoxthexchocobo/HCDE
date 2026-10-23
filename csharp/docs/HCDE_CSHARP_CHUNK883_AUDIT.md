# Chunk 883 — Retired (2026-10-23): six disjoint `b_func.cpp` functions fail land `--band` hull **45–563** (overlaps **879** gap credit). Re-land as contiguous `b_func.cpp` bands that pass `--band` honestly.

## Scope

Retired (2026-10-23): six disjoint `b_func.cpp` functions fail land `--band` hull **45–563** (overlaps **879** gap credit). Re-land as contiguous `b_func.cpp` bands that pass `--band` honestly.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`UncreditedRangeCheckGateTests.cs`
Managed LOC (tally): 0.
Conversion commit: `7b215c1ab27c26c153d2d7729a72943550feeead`.

## Padding review (2026-10-21)

### Retire reason

| Issue | Resolution |
|-------|------------|
| Six disjoint native bands on `b_func.cpp` | Hull **45–563** overlaps **879** (**125–150**, **267–449**, …) |
| Per-row land gate (no `--band`) | **Reverted** — gate tooling must not loosen contiguous-band rule |

### Behavior ported (matrix credit)

| Native | Behavior ported |
|--------|-----------------|
| (none) | **no** |

### Padding map

| Native lines | Reason |
|--------------|--------|
| **45–124**, **151–266**, **454–478**, **490–510**, **512–551**, **556–563** | Attempted credit; retired — re-chunk contiguously |

### Gross vs net

| | Value |
|--|------:|
| Gross attempted | **290** |
| Net behavior-ported | **0** |

## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 883 --to 883` (0 hard / 0 soft).

## Seams

Off-tally helpers `VmBBotFuncReachFire` / `CajunReachable` remain for **875** direct calls until a passing contiguous chunk re-credits them.

## Credit rationale

Contiguous-band gate (`uncredited_range_check --chunk N --band`) rejects multi-gap same-TU credit; not fragment stitching.
