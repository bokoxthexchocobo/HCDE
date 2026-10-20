# ACS CallFunction gameplay/math band — chunk 66 recognition audit

**Native:** `p_acs.cpp` `CallFunction` uncredited lines 5695–6035 (velocity, user variables, class checks, UniqueTID, sqrt/vector/HUD/CVar cases).

**Managed:** Pre-existing `AcsCallFunctions`, `AcsScriptUserVariableService`, `AcsActorTid`, absent-opcode HUD helpers.

**Bucket:** recognition. Tests reference existing Playsim coverage.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
