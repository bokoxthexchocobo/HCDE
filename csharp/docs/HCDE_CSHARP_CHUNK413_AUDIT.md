# Chunk 413 — LS_Floor_* movers and Generic_Floor dispatch Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

LS_Floor_* movers and Generic_Floor dispatch Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `0a47513b66af11626ca4ec1aa8a070748cfb43fa`.

## Padding review (2026-10-21)



### Out-of-body uncredit (413)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **154** |
| **Out-of-body removed** | **154** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 413 --to 413` (0 hard / 0 soft).

## Seams

`EV_DoFloor` and live sector motion are not hosted in these bands.

## Credit rationale

Credits native `LS_Floor_*` argument wiring and `LS_Generic_Floor` target selection from `p_lnspec.cpp`.
