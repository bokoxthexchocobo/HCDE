# Chunk 442 — DoSectorDamage filters and P_SectorDamage 3D-floor loop Retired (2026-10-24): `--band` hull overlaps existing credit (p-spec-actor-sector-friction-chunk805 src/playsim/p_spec.cpp 406-501).

## Scope

DoSectorDamage filters and P_SectorDamage 3D-floor loop Retired (2026-10-24): `--band` hull overlaps existing credit (p-spec-actor-sector-friction-chunk805 src/playsim/p_spec.cpp 406-501).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `14b242832d24a5d69853102ec61aab09a1d3453a`.

## Padding review (2026-10-21)



### Out-of-body uncredit (442)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **155** |
| **Out-of-body removed** | **2** |
| **Honest** | **153** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 442 --to 442` (0 hard / 0 soft).

## Seams

Sector tag iterators and `P_DamageMobj` are not hosted.

## Credit rationale

Credits shootable/player/air/protection gates, 3D-floor height swap, in-air damage policy, and terrain damage tick masks from `p_spec.cpp`.
