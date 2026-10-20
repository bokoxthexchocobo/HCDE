# ACS GetActorViewHeight — chunk 62 audit

**Native:** `p_acs.cpp` `ACSF_GetActorViewHeight` 5562–5576.

**Managed:** `AcsActorViewHeightService.cs`, `AcsActorViewHeightServiceTests`.

Player `crouchviewdelta` beyond represented `CrouchFactor` policy is unchanged.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
