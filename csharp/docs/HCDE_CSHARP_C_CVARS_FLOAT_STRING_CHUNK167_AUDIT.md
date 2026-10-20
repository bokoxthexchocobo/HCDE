# Chunk 167 — float/string CVars + registry

`VmFloatCvar`, `VmStringCvar`, `VmCvarRegistry.RegisterTyped`/`CreateTyped`/`SyncTypedEntry`.

**Not credited:** ZS custom CVars, color type, callback dispatch.

Tests: `VmCvarRegistryTypedTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
