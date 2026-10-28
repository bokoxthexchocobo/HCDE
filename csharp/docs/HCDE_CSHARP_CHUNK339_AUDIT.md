# Chunk 339 — MBF FindTracer, seeker, tracer2 defaults (FindTracer moved to chunk 571 tally overlap fix) Retired (2026-10-23): out-of-body credit removed; honest **22** < **150**.

## Scope

MBF FindTracer, seeker, tracer2 defaults (FindTracer moved to chunk 571 tally overlap fix) Retired (2026-10-23): out-of-body credit removed; honest **22** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `9ef6fb65cca5b2650e980d507b37bc793773d163`.

## Padding review (2026-10-21)

| Managed | Native (`d_dehacked.cpp`) |
|---------|---------------------------|
| `VmDehackedMbfPointerEmitters` | `CreateFindTracerFunc` 936–941; `CreateSeekerMissileFunc` 943–952; `CreateTracer2Func` 954–959 |

### Out-of-body uncredit (339)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **22** |
| **Out-of-body removed** | **0** |
| **Honest** | **22** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 339 --to 339` (0 hard / 0 soft).

## Seams

`MBFParamState` and `FunctionCallEmitter` are not re-hosted; managed code mirrors default-argument behavior from the native emitter thunks.

## Credit rationale

Credits default-argument packing for tracer-family emitters; `CreateFindTracerFunc` credited on chunk 339 only for the trimmed default-range slice.
