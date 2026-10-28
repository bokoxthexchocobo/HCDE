# Chunk 180 — terrain splash + friction

`VmTerrainSplashParser` / `VmTerrainFrictionParser` from `p_terrain.cpp`.

**Not credited:** `GenericParse`, `ParseOuter`, `ParseTerrain` lump driver.

Tests: `VmTerrainSplashParserTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
