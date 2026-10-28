# Chunk 150 — `name.cpp` `FindName`

`VmNameTable` dynamic case-insensitive name index (not `namedef.h` compile-time table / CRC dedup).

Tests: `VmNameTableTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
