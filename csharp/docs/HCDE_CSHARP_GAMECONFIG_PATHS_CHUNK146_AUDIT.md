# Chunk 146 — `GetConfigPath` / `CollectDefaultSearchPaths`

`VmGameConfigPaths`, `VmGameConfigSearchPaths`; wired on `VmGameConfigFile`.

**Not credited:** macOS `M_GetMacSearchDirectories`, Haiku paths, `M_GetConfigPath` platform bodies.

Tests: `VmGameConfigPathsTests`, `VmGameConfigSearchPathsTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
