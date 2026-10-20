# Chunk 439 — P_ActorInSpecialSector damage header, P_UpdateSpecials timelimit, friction movers

## Scope

P_ActorInSpecialSector damage header, P_UpdateSpecials timelimit, friction movers

## Native ranges

- `src/playsim/p_spec.cpp` **P_ActorInSpecialSector damage and exit** 406–477 (72 lines)
- `src/playsim/p_spec.cpp` **P_UpdateSpecials timelimit** 685–696 (12 lines)
- `src/playsim/p_spec.cpp` **P_SetSectorFriction / FrictionToMoveFactor** 751–816 (66 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPSpecFrictionSector.cs`, `VmPSpecFrictionSectorTests.cs`
Managed LOC (tally): 85.
Conversion commit: `f391af022e0627f27f8dae4998bb3a4ca767a56a`.

## Tests

- `VmPSpecFrictionSectorTests.DefaultDamageInterval_RepairsZero`
- `VmPSpecFrictionSectorTests.IronFeetProtectionLevel_FullWins`
- `VmPSpecFrictionSectorTests.TimelimitExit_WhenTimeExceeded`
- `VmPSpecFrictionSectorTests.FrictionToMoveFactor_ClampsTiny`
- `VmPSpecFrictionSectorTests.SectorFrictionFromAmount_Clamped`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 439 --to 439` (0 hard / 0 soft).

## Seams

`P_DamageMobj`, sector thinkers, and tag iterators are not hosted.

## Credit rationale

Credits exit-sector kills, hazard/iron-feet leak policy, timelimit exit, sector friction amount mapping, and ice/mud move-factor formulas from `p_spec.cpp`.
