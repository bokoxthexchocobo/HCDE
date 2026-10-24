# Script user variables and actor orientation — chunk 54 audit

**Native:** `p_acs.cpp` `GetVarAddrType` 4952–4990, `SetUserVariable` 4992–5013, `GetUserVariable` 5015–5040, `SetActorAngle` 5184–5204, `SetActorPitch` 5206–5226, `SetActorRoll` 5228–5248.

**Managed:** `AcsScriptUserVariableService.cs`, `AcsActorOrientationService.cs`, tests; `AcsCallFunctions` delegates.

String/float user fields and ZScript field metadata are not converted; int slots only.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
