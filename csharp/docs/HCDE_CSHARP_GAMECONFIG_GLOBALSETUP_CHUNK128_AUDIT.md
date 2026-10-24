# Chunk 128 — `DoGlobalSetup` portable migrations

`VmGameConfigGlobalSetup` ports LastRun release detection, `.WeaponSlots` section purge (`last < 208`), and Hexen binding renames (`last < 210`). Full CVar migration ladder remains native-only.

Tests: `VmGameConfigExtendedTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
