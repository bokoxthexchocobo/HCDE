# ACS GetActorVelX/Y/Z — chunk 59 audit

**Native:** `p_acs.cpp` `DLevelScript::CallFunction` `ACSF_GetActorVelX/Y/Z` 5477–5490.

**Managed:** `AcsActorVelocityService.cs`, `AcsActorVelocityServiceTests`; `AcsCallFunctions` delegates.

Client-side TID selection and `DoubleToACS` edge cases beyond represented fixed velocity are unchanged.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
