# Chunk 160 — `LoadStrings` seam

`IVmStringTableLumpSource`, `VmStringTableLoadStrings`, `DeleteForLabel` / `DeleteString`.

**Not credited:** real WAD `FileSystem`, `UpdateLanguage` diagnostics, post-load macro retention.

Tests: `VmStringTableExtendedTests.LoadStrings_ReadsLanguageAndMacros`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
