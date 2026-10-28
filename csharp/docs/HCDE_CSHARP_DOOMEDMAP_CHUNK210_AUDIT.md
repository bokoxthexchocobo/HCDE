# Chunk 210 — MAPINFO DoomEd nums

`VmDoomEdMapinfoParser` — `SpecialMapthingNames`, `ParseDoomEdNums`, sorted dump enumeration (`dumpmapthings`).

**Seams:** `P_FindLineSpecial`, `PClass::FindActor`, and `InitActorNumsFromMapinfo` resolution are injected via callbacks / not ported.

Tests: `VmDoomEdMapinfoParserTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
