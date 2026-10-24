# ACS hot CallFunc batch 2 audit

## Scope

`p_acs.cpp` `ACSF_GetAirSupply`, `ACSF_SetAirSupply`, `ACSF_SpawnSpotForced`, `ACSF_GetUserArray`, `ACSF_SetUserArray`.

## Implementation

- `AcsCallFunctions.cs` dispatch + helpers
- `PlayerPawn.AirFinishedTic` (map-tic air supply storage)
- `AcsHotCallFuncBehaviorTests` (+3 cases)

## Tests

Air supply round-trip player 0, spawn spot forced at activator, user-array index 2 round-trip.

## Verified

- `dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green.
