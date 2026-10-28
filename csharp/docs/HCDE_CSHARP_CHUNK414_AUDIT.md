# Chunk 414 — LS_Ceiling_* crush and move specials Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

LS_Ceiling_* crush and move specials Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `c9d3a4596d474e4687227582a15cd6fa16ea48a5`.

## Padding review (2026-10-21)



### Out-of-body uncredit (414)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **166** |
| **Out-of-body removed** | **166** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 414 --to 414` (0 hard / 0 soft).

## Seams

`EV_DoCeiling` thinkers are not hosted in these bands.

## Credit rationale

Credits native `LS_Ceiling_*` argument wiring including crush-stop gametype policy from `p_lnspec.cpp`.
