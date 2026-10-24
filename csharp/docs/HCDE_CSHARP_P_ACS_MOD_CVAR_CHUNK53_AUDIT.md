# ACS mod CVar bridge — chunk 53 audit

**Native:** `p_acs.cpp` `DoSetCVar` 5043–5078, `DoGetCVar` 5081–5104, `SetUserCVar` 5119–5148, `SetCVar` 5150–5171.

**Managed:** `AcsModCvarService.cs`, `AcsModCvarServiceTests`; `AcsCallFunctionsExtended` delegates.

Server-authority `CVAR_SERVERINFO` gate and strict `CVAR_MOD` filtering are not modeled.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
