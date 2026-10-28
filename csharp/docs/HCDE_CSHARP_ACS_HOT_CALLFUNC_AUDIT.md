# ACS hot CallFunc behavior audit

## Scope

`ACSF_SpawnForced`, `ACSF_Get/SetUserVariable`, `ACSF_GetThingUDMF*`, completed side/sector UDMF reads in `TryGetUdmf`.

## Implementation

- `AcsCallFunctions.cs` switch arms + `Actor.ScriptUserVariables` on `Actor`

## Tests

- `AcsHotCallFuncBehaviorTests` (4 cases) including wire id 36 dispatch.

## Verified

- Full Playsim suite green after changes.
