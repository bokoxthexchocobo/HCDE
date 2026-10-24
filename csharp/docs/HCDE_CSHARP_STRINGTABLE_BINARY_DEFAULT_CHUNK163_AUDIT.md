# Chunk 163 — LANGUAGE binary skip + default table updates

`VmStringTableLoader.TryLoad`, `IsBinaryLanguageLump`, `hasDefaultEntry` + `DeleteForLabel`.

**Not credited:** `sc.isText()` scanner hook, `ScriptMessage` for `~`.

Tests: `VmStringTableFinishTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
