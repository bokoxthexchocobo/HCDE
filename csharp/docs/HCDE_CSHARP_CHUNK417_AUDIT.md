# Chunk 417 — LS_Thing_Hate targeting policy

## Scope

LS_Thing_Hate targeting policy

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Thing_Hate** 1565–1726 (162 lines)

**Credited native LOC:** 162 (tally `nativeLoc` 162).

## Managed

`VmPLnspecThingHate.cs`, `VmPLnspecThingHateTests.cs`
Managed LOC (tally): 88.
Conversion commit: `8c0d6d0aac65710872775bc8c87534b33befad9f`.

## Tests

- `VmPLnspecThingHateTests.HateTarget_RejectsDormant`
- `VmPLnspecThingHateTests.HateFlags_SetsNoSightForMode2`
- `VmPLnspecThingHateTests.HateFlags_SetsHuntPlayersForMode4`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 417 --to 417` (0 hard / 0 soft).

## Seams

Actor iterators, `SeeState`, and live mobj flags are not hosted.

## Credit rationale

Credits `LS_Thing_Hate` hatee selection and `MF3`/`MF4` attitude flags from `p_lnspec.cpp`.
