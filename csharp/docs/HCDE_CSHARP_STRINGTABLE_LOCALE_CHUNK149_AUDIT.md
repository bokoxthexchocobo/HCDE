# Chunk 149 — `stringtable.cpp` locale

`VmStringTableLocale`: `GetSystemLocale`, legacy language code remap table.

**Not credited:** Windows locale path, full `FStringTable` lump loader, `NAME_*` enum indices.

Tests: `VmStringTableLocaleTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
