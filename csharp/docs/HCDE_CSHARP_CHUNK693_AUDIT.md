# Chunk 693 — Whole `ThingCheck`, `CheckPlane`, and `EditTraceResult` (`p_trace.cpp` 668–768, 952–1025). Retired (2026-10-24): `--band` hull overlaps existing credit (p-trace-traverse-chunk692 src/playsim/p_trace.cpp 776-944).

## Scope

Whole `ThingCheck`, `CheckPlane`, and `EditTraceResult` (`p_trace.cpp` 668–768, 952–1025). Retired (2026-10-24): `--band` hull overlaps existing credit (p-trace-traverse-chunk692 src/playsim/p_trace.cpp 776-944).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `c0a3658082f8dc582d21d47ebe9b1defa7595932`.

## Padding review (2026-10-21)

| Managed | Native (`p_trace.cpp`) |
|---------|----------------------|
| `ThingCheck` top intercept clip | `ThingCheck` 668–686 |
| `ThingCheck` bottom intercept clip | 687–701 |
| `ThingCheck` 3D-floor ceiling/floor + callback | 703–742 |
| `ThingCheck` actor hit + callback | 744–767 |
| `CheckPlane` | `CheckPlane` 952–972 |
| `EditTraceResult` `TRACE_HitSky` floor/ceiling/wall | `EditTraceResult` 982–1001 |
| `EditTraceResult` `TRACE_NoSky` floor/ceiling/wall | 1003–1023 |

### Out-of-body uncredit (693)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **168** |
| **Out-of-body removed** | **0** |
| **Honest** | **168** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 693 --to 693` (0 hard / 0 soft).

## Seams

3D-floor early outs in `ThingCheck` use injected callbacks in full sim.

## Credit rationale

Thing/plane intercept and trace flag editing after traverse chunk 692.
