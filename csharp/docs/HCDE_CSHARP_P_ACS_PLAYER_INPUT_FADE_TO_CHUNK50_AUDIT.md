# GetPlayerInput / CheckInputNum / DoFadeTo — chunk 50 audit

**Native:** `p_acs.cpp` `DLevelScript::GetPlayerInput` 4616–4642; `p_things.cpp` `P_Thing_CheckInputNum` 536–565; `p_acs.cpp` `DLevelScript::DoFadeTo` 3916–3918.

**Managed:** `AcsPlayerInput.cs` (existing), `AcsPlayerInputIds.cs`, `AcsFadeToService.cs`, `AcsGetPlayerInputNativeParityTests`, `AcsFadeToServiceTests`.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
