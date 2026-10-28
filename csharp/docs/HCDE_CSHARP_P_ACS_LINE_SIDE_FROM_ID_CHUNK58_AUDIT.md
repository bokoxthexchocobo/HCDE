# LineFromID / SideFromID and UDMF getters — chunk 58 audit

**Native:** `p_acs.cpp` `SideFromID` 4920–4937, `LineFromID` 4939–4950; `CallFunction` UDMF line/side cases 5448–5475.

**Managed:** `AcsLineSideFromId.cs`, `AcsCallFunctions.TryGetUdmf` routing; `AcsLineSideFromIdTests`.

`SetActorTeleFog` (5250–5280) remains credited via existing `AcsActorTeleFog` tests from earlier chunks. Full `CallFunction` switch from 5479+ is not in this slice.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
