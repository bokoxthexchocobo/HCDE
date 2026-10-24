# Chunk 174 — linked sector movement

`VmLinkedSectorMovement`, `VmLinkedSectorAttach`, link type constants from `p_linkedsectors.cpp`.

**Not credited:** `P_AddSectorLinksByID`, interpolation list, live `P_ChangeSector` / plane crush.

Tests: `VmLinkedSectorTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
