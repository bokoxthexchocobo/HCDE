# Chunk 427 — Thing_SetSpecial, ChangeTID, Damage/Heal, activation helpers, activate/deactivate/remove/destroy Retired (2026-10-23): out-of-body credit removed; honest **20** < **150**.

## Scope

Thing_SetSpecial, ChangeTID, Damage/Heal, activation helpers, activate/deactivate/remove/destroy Retired (2026-10-23): out-of-body credit removed; honest **20** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `2ae6e417b3339de401dc923488acf17af3d6b4c4`.

## Padding review (2026-10-21)



### Out-of-body uncredit (427)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **235** |
| **Out-of-body removed** | **215** |
| **Honest** | **20** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 427 --to 427` (0 hard / 0 soft).

## Seams

`P_DamageMobj`, `P_GiveBody`, and actor iterators are not hosted.

## Credit rationale

Credits TID/special assignment, damage/heal branches, `THINGSPEC` activation toggles, and destroy/massacre policy from `p_lnspec.cpp`.
