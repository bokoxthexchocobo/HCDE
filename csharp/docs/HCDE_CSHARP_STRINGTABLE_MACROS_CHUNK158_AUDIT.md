# Chunk 158 — LMACROS and `@[macro]`

`VmStringTableMacros`, macro dictionary on `VmStringTable`, gender slots filled from macro column 0.

**Not credited:** four-gender distinct macro columns per gender index in native `InsertString`.

Tests: `VmStringTableExtendedTests.LoadStrings_ReadsLanguageAndMacros`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
