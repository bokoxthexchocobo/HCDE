# Chunk 156 — `c_cvars` registry seam + `C_NameKeys`

`VmCvarRegistry`: `Create`, `SetArchiveDefaults`, `DestroyFlagged` (dictionary map, not `FBaseCVar` types). `VmKeyBindFormatting`: `C_NameKeys` overloads.

**Not credited:** `C_ReadCVars`/`C_WriteCVars`, latch/unlatch, `PerformBind`, double-click queue.

Tests: `VmCvarRegistryExtendedTests`, `VmKeyBindFormattingTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
