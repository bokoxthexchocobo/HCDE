# Chunk 168 — `ReadBindings`

`VmKeyBindingsLumpLoader`: `bind`, `doublebind`, `mapbind`, `unbind` lines.

**Not credited:** WAD lump `FScanner` driver, mod override filter edge cases.

Tests: `VmKeyBindingsLumpLoaderTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
