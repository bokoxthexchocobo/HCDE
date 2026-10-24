# Chunk 814 — Whole `P_LineAttack` puff-type overload, `CheckLineTrace`, `P_LineTrace`, and `P_LinePickActor` from `p_map.cpp` (5052–5237).

## Scope

Whole `P_LineAttack` puff-type overload, `CheckLineTrace`, `P_LineTrace`, and `P_LinePickActor` from `p_map.cpp` (5052–5237).

## Native ranges

- `src/playsim/p_map.cpp` **P_LineAttack** 5052–5067 (16 lines)
- `src/playsim/p_map.cpp` **CheckLineTrace** 5083–5100 (18 lines)
- `src/playsim/p_map.cpp` **P_LineTrace** 5102–5200 (99 lines)
- `src/playsim/p_map.cpp` **P_LinePickActor** 5208–5237 (30 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Portfolio delta

**Gross CLI band:** **186** lines in the UNCredited-range probe; tally credits **163** whole-function native LOC only.
**23** uncovered lines in that band are inter-function gaps, comments, or non-function lines (not credited).

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **186** |
| **Credited** (whole functions, this row) | **163** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+163** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

`VmPMapLineTrace.cs`
Managed LOC (tally): 148.
Conversion commit: `af98583193d42a9e84cb8e14b9a7a6f9f4c3168f`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|----------------------|
| `P_LineAttack` (FName puff) | **5052–5067** |
| `CheckLineTrace` | **5083–5100** |
| `P_LineTrace` | **5102–5200** |
| `P_LinePickActor` | **5208–5237** |

**Post-review ratio:** 148/163 managed/native ≈ 91% (src file LOC at HEAD; see tally).

## Land hygiene

- Port code **`f32ac48a`**; `chunk814.json` test-band fix **`af985831`**; chunk **481** `managedSourceFiles` exclusivity in **`1e2d91f1`**.
- **`bash csharp/tools/land_chunk_fast.sh --resume 814`** → tally **`b351b8dd`** → hardened `verify_full_solution_tests.sh` at **`b351b8dd`** → push (no hand tally).

## Tests

- `BuildLineFlags_ClearsBlockOnThrublock` mirrors native **`P_LineTrace` 5151** (`TRF_THRUBLOCK` clears `ML_BLOCKEVERYTHING` on the trace line mask); listed on the `P_LineTrace` span for reaudit band coverage, not as a synthetic helper.

- `VmPMapLineTraceTests.PuffTypeMissing_ReturnsNullWhenClassUnknown`
- `VmPMapLineTraceTests.CheckLineTrace_SkipsPortal`
- `VmPMapLineTraceTests.PortalCount_SubtractsTwo`
- `VmPMapLineTraceTests.BuildLineFlags_ClearsBlockOnThrublock`
- `VmPMapLineTraceTests.LinePick_ReturnsActorOnHit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 814 --to 814` (0 hard / 0 soft).

## Seams

`Trace`, `PClass::FindActor`, and sector queries are injected.

## Credit rationale

First tally credit for ACS/trace helpers after chunk710 class `P_LineAttack`; retires recognition-only chunk480 row.
