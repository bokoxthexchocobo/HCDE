# GetActorProperty coverage reinforcement — chunk 42 audit

**Native:** `p_acs.cpp` `DLevelScript::GetActorProperty` scalar cases 4418–4485 and 4487–4501 (string cases covered via `AcsActorStringProperties`).

**Managed:** `AcsActorProperties` read extensions, `AcsGetActorPropertyCoverageTests`, loc_tally chunk 38 range trim.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
