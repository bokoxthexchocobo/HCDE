# Chunk 222 — UMAPINFO token helpers and doomedmap strife tail (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **15** < **150**.

## Scope

UMAPINFO token helpers and doomedmap strife tail (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **15** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `8d4be6af498c2e6f394f5ded322853ee4a240ca6`.

## Padding review (2026-10-21)



### Out-of-body uncredit (222)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **43** |
| **Out-of-body removed** | **28** |
| **Honest** | **15** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 222 --to 222` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Credits small UMAPINFO parse branches and Strife spawnable clear — not full schema tables or CCMD bodies.
