# Chunk 692 — Whole `FTraceInfo::TraceTraverse` (`p_trace.cpp` 776–944).

## Scope

Whole `FTraceInfo::TraceTraverse` (`p_trace.cpp` 776–944).

## Native ranges

- `src/playsim/p_trace.cpp` **FTraceInfo::TraceTraverse** 776–944 (169 lines)

**Credited native LOC:** 169 (tally `nativeLoc` 169).

## Managed

`VmPTraceTraverse.cs`
Managed LOC (tally): 106.
Conversion commit: `6bbba8972aa6b5636b06907c1f9640b76074cb57`.

## Padding review (2026-10-21)

| Managed | Native (`p_trace.cpp`) |
|---------|----------------------|
| `VmPTraceTraverse` | `TraceTraverse` 776–944 |


**Post-review ratio:** 106/169 managed/native ≈ 63% (src file LOC at HEAD; see tally).

## Tests

- `VmPTraceTraverseTests.TraceTraverse_EntersLinePortalAndSkipsLineCheck`
- `VmPTraceTraverseTests.TraceTraverse_FinalizeMissWhenNoHit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 692 --to 692` (0 hard / 0 soft).

## Seams

`FPathTraverse`, `LineCheck`, `ThingCheck`, and plane finish helpers invoked via delegates.

## Credit rationale

Main trace path loop after portal entry chunk 688.
