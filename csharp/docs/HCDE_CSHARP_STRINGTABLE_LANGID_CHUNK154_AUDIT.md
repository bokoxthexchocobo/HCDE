# Chunk 154 — language IDs and escapes

`VmStringTableLangId`, `VmStringTableEscapes`, `VmNativeCrc32`, `VmStringTableCsv.Parse` subset.

**Not credited:** `GetFallback` FName table (covered in chunk 149 strings), `ParseLanguageCSV`, `readMacros`.

Tests: `VmStringTableLangIdTests`, `VmStringTableTests.ProcessEscapes_DecodesNewline`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
