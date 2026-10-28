# Chunk 330 — MapFlagHandlers strife/compat rows (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

MapFlagHandlers strife/compat rows (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `5ab0008de4c4fce15b1dfebf5fe5f70ac128edee`.

## Padding review (2026-10-21)



### Out-of-body uncredit (330)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **2** |
| **Out-of-body removed** | **2** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 330 --to 330` (0 hard / 0 soft).

## Seams

Handler rows keep placeholder `LEVEL_*`/`COMPATF_*` bit values; only catalog lookup and flag-apply helpers are modeled.

## Credit rationale

Credits only handler rows referenced by mid-table lookup tests.
