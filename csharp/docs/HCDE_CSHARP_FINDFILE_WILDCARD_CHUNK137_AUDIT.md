# Chunk 137 — `findfile.cpp` path normalize + wildcard filter

`VmFindFileWildcard` ports `D_AddFile` slash normalization / duplicate detection and `D_ApplyWildcardExtensionFilter`.

**Not credited:** `D_AddFile` filesystem/stat, `D_AddWildFile` directory scan, `D_AddConfigWads`.

Tests: `VmFindFileWildcardTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
