# Chunk 134 — `gameconfigfile.cpp` ctor `LastRun` &lt; 227 search paths

`VmGameConfigLastRun227.EnsureSearchPathsForLegacyConfig` mirrors `EnsureValueForKey` loops for IWAD/File/Soundfont search sections when upgrading configs below version 227.

Tests: covered via `VmGameConfigFile` ctor (existing `VmGameConfigFileTests`); no separate test file.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
