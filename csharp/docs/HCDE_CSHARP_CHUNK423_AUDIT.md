# Chunk 423 — LS_SetPlayerProperty power grants and cheat flag masks Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

LS_SetPlayerProperty power grants and cheat flag masks Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `3eff03063c26b2dea42e3cddadf8c7909f8f121d`.

## Padding review (2026-10-21)



### Out-of-body uncredit (423)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **218** |
| **Out-of-body removed** | **218** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 423 --to 423` (0 hard / 0 soft).

## Seams

`GiveInventoryType`, power actors, and live `player_t` mutation are not hosted.

## Credit rationale

Credits power-table indexing, ALLMAP console-player gate, `CF_*` mask mapping, and frozen-prop compat clearing from `p_lnspec.cpp`.
