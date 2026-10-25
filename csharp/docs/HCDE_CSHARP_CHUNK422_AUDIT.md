# Chunk 422 — Automap line flags, camera change, thing move, quake/puzzle Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

Automap line flags, camera change, thing move, quake/puzzle Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `10f2eb52a664bc835b00fceec2f73db81a539a08`.

## Padding review (2026-10-21)



### Out-of-body uncredit (422)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **150** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 422 --to 422` (0 hard / 0 soft).

## Seams

Line iterators, `R_ClearPastViewer`, inventory scan, and `P_StartQuake` are not hosted.

## Credit rationale

Credits automap flag shifts, camera revert cheat policy, `EV_Thing_Move` fog arg, and puzzle-item scan gates from `p_lnspec.cpp`.
