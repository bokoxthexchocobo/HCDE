# Chunk 197 — Decal Clear and ApplyToDecal

`VmDecalRuntimeApply`, `VmDecalInstanceState`, and `VmDecalLibrary.Clear` (tree reset; translation list not ported).

Tests: `VmDecalRuntimeApplyTests`, `VmDecalLibraryTests.Clear_RemovesAllDecals`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
