# Chunk 147 — `M_SaveCustomKeys`

`VmKeySectionArchive.SaveCustomKeys` archives per-action binds via `VmKeyBindings.ArchiveBindings(matchcmd)`.

**Not credited:** `addkeysection` CCMDs, `LoadKeys`, `KeySections` TArray lifecycle.

Tests: `VmKeySectionArchiveTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
