# Chunk 164 — per-gender macro expansion

`VmStringTableMacros.ApplyMacrosForGenders` in `InsertString`.

**Not credited:** `Printf` on bad macro syntax.

Tests: `VmStringTableFinishTests.InsertString_ExpandsMacrosPerGender`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
