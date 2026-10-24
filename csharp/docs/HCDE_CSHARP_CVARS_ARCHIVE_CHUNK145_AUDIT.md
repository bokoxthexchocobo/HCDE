# Chunk 145 — `c_cvars.cpp` archive / lookup

`VmCvarArchive`, `VmCvarRegistry`, `VmCvarFlags`, `VmCvarSearch`; `VmGameConfigArchive.ArchiveGameDataFromRegistry` uses filtered archive.

**Not credited:** `cvarMap` storage, `FBaseCVar` types, `C_CreateCVar`, demo net CVars.

Tests: `VmCvarArchiveTests`, `VmGameConfigArchiveRegistryTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
