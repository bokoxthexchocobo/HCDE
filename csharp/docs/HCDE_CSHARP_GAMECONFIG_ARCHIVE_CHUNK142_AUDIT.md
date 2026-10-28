# Chunk 142 — `ArchiveGameData` / `ArchiveGlobalData`

`VmGameConfigArchive` writes cvar snapshots, aliases, and binding archives through `VmConfigFile` + `VmKeyBindings`.

**Not credited:** `C_ArchiveCVars` flag filtering, `M_SaveCustomKeys`, netgame host gating beyond bool parameters.

Tests: `VmGameConfigModArchiveTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
