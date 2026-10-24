# Chunk 322 — MAPINFO inter/exit/enter picture assigns (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **18** < **150**.

## Scope

MAPINFO inter/exit/enter picture assigns (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **18** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `c5e4347dfaa6ce42007fb4a8975eedc810bbed02`.

## Padding review (2026-10-21)



### Out-of-body uncredit (322)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **18** |
| **Out-of-body removed** | **0** |
| **Honest** | **18** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 322 --to 322` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Credits string assignment for inter/exit/enter pics — not anim/specialaction tails.
