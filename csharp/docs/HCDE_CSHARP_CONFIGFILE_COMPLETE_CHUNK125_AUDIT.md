# Chunk 125 — complete `configfile.cpp` `FConfigFile`

`VmConfigFile` implements INI load/save, sections, key/value CRUD, `EnsureValueForKey`, iteration, and multiline `<<<` values.

Tests: `VmConfigFileTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
