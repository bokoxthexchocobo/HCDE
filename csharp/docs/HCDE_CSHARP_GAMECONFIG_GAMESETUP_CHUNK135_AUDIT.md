# Chunk 135 — `gameconfigfile.cpp` `DoGameSetup` + menu option filter

`VmGameConfigGameSetup` reads game-scoped INI sections into a dictionary, parses `ConsoleAliases`, and applies HCDE UI baseline when `LastRun` &lt; 232.

`VmMenuOptionFilter` ports `CheckSkipGameOptionBlock` from `doommenu.cpp` (MENUDEF game-option blocks).

**Not credited:** `SetRavenDefaults`, `SetStrifeDefaults`, `ReadCVars` into live `FBaseCVar`, netgame archive paths.

Tests: `VmGameConfigGameSetupTests`, `VmMenuOptionFilterTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
