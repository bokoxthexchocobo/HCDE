# Chunk 141 — `DoModSetup` / `ReadNetVars` / `ReadCVars`

Dictionary reads for mod/serverinfo sections; `ReadCVars` credited as ini→sink (not `FStringCVar` creation).

Tests: `VmGameConfigModArchiveTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
