# Chunk 152 — `stringtable.cpp` `LoadLanguage`

`VmStringTableLoader`: `[` language blocks (incl. `default` → `en-US` via legacy remap), `$ifgame` skip, `InsertString` on active maps.

**Not credited:** `DeleteForLabel`, binary lump skip, CSV/`LoadStrings` filesystem path.

Tests: `VmStringTableTests.LoadAndLookup_ReturnsInsertedString`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
