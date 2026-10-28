# Chunk 127 — `gameconfigfile.cpp` `ReadCVars`

`VmGameConfigCvars` mirrors `FGameConfigFile::ReadCVars` and `DoGlobalSetup` GlobalSettings section reads (dictionary sink; no live `FBaseCVar` engine).

Tests: `VmGameConfigExtendedTests.ReadCvars_LoadsGlobalSettings`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
