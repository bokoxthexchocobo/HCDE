# Chunk 129 — autoload / autoexec

`VmGameConfigAutoload` mirrors legacy autoload section renames, autoload hierarchy creation, `CreateStandardAutoExec`, and `AddAutoexec` path collection.

Tests: `VmGameConfigExtendedTests.DoAutoloadSetup_RenamesLegacySection`, `AddAutoexec_CollectsExistingPaths`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
