# Chunk 439 — P_ActorInSpecialSector damage header, P_UpdateSpecials timelimit, friction movers Retired (2026-10-23): out-of-body credit removed; honest **149** < **150**.

## Scope

P_ActorInSpecialSector damage header, P_UpdateSpecials timelimit, friction movers Retired (2026-10-23): out-of-body credit removed; honest **149** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `07b4e17d62e90e8cfe17625b7bfe4b76102cc8ad`.

## Padding review (2026-10-21)



### Out-of-body uncredit (439)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **1** |
| **Honest** | **149** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 439 --to 439` (0 hard / 0 soft).

## Seams

`P_DamageMobj`, sector thinkers, and tag iterators are not hosted.

## Credit rationale

Credits exit-sector kills, hazard/iron-feet leak policy, timelimit exit, sector friction amount mapping, and ice/mud move-factor formulas from `p_spec.cpp`.
