# Chunk 126 — `gameconfigfile.cpp` bootstrap

`VmGameConfigFile` extends `VmConfigFile` with default search-path sections, header text, and legacy gamma migration hook (`LegacyGammaToFixGamma` from chunk 120).

Tests: `VmGameConfigFileTests`, `VmGameConfigLegacyGammaTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
