# Chunk 153 — `stringtable.cpp` lookups

`VmStringTable`: `UpdateLanguage` via `ForEachLangID` order, `exists`, `CheckString` (`$$` alias), `GetLanguageString`, `MatchDefaultString`, `GetString`.

**Not credited:** gendered `@[]` macros, `InsertString` multi-file delete, IWAD `LoadStrings`.

Tests: `VmStringTableTests.CheckString_FollowsDollarAlias`, `LoadAndLookup_ReturnsInsertedString`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
