# Chunk 416 — LS_Stairs_* / Generic_Stairs and Thing_Raise/Stop/SetGoal Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

LS_Stairs_* / Generic_Stairs and Thing_Raise/Stop/SetGoal Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `3475c650f48eb2b32238d2f3f546e9cd4bda4f04`.

## Padding review (2026-10-21)



### Out-of-body uncredit (416)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **171** |
| **Out-of-body removed** | **171** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 416 --to 416` (0 hard / 0 soft).

## Seams

`EV_BuildStairs`, `P_Thing_Raise`, and patrol iterators are not hosted.

## Credit rationale

Credits stair builder and thing raise/stop/goal argument wiring from `p_lnspec.cpp`.
