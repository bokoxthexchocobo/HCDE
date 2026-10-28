# p_acs ThingCount and CountPlayers — chunk 24 audit

## Scope

Extract native ACS actor census (`ThingCount` with decorate replacement pass) and player counting into dedicated services wired from `AcsVm`.

**Native:** `p_acs.cpp` `DLevelScript::ThingCount` 3674–3753, `CountPlayers` 3774–3783.

**Managed:** `AcsThingCountService.cs`, `AcsThingCountReplacementPolicy.cs`, `AcsPlayerPopulation.cs`, `AcsThingCountServiceTests`.

## Verified

`dotnet build -warnaserror`; full `dotnet test`.
