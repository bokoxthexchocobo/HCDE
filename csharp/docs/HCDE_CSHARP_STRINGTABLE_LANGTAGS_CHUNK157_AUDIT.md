# Chunk 157 — LANGUAGE bracket lang tags

`VmStringTableLoader.ReadLangTag`: quoted `"en-US"` and scanner-split `en` `-` `US` merge.

**Not credited:** `ForEachLangID` token machine in `ExtractComponents`, binary lump skip.

Tests: `VmStringTableExtendedTests` quoted/split cases.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
