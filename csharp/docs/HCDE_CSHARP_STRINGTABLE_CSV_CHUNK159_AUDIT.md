# Chunk 159 — `ParseLanguageCSV`

`VmStringTableLanguageCsv`: header columns, filter/`CheckGame`, default column remap, empty cell deletes.

**Not credited:** full `parseCSV` (chunk 154), `InsertString` gender macros.

Tests: `VmStringTableExtendedTests.ParseLanguageCsv_LoadsIdentifierRow`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
