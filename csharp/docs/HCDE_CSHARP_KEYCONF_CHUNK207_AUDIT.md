# Chunk 207 — KEYCONF / keysections

Managed: `VmKeyConfParser`, `VmKeyConfIniLoader` (`LoadKeys`), `VmKeyConfPlayerParser` (`clearplayerclasses` / `addplayerclass`).

**Seams:** `D_LoadWadSettings` lump iteration delegates to `ParseKeyConfLumpCommands`; `AddCommandString` dispatch remains in native/client console (not modelled). `LoadKeys` requires `VmConfigFile` + `VmKeyBindings` (ini I/O). `addplayerclass` uses optional `validateClass` callback instead of `PClass::FindActor` / `ValidatePlayerClass`.

Tests: `VmKeyConfParserTests`, `VmKeyConfPlayerParserTests`, `VmKeyConfIniLoaderTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
