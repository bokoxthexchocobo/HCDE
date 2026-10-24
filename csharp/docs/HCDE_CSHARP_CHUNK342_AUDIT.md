# Chunk 342 — EMAPINFO unsupported sample stats header Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

EMAPINFO unsupported sample stats header Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `c628ec9170c6a04ced2d17d0181b47dbf54acb11`.

## Padding review (2026-10-21)



### Out-of-body uncredit (342)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **18** |
| **Out-of-body removed** | **18** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 342 --to 342` (0 hard / 0 soft).

## Seams

Anonymous-namespace `TMap` registries are not re-hosted; only the unsupported-sample cap used by `RecordUnsupported` is modeled.

## Credit rationale

Credits the uncredited `hcde_emapinfo.cpp` header band 33–50 defining `EMapInfoUnsupportedSample` and the eight-sample retention policy consumed by stats recording helpers.
