# Chunk 196 — Decal library BST insert and lookup

`VmDecalLibrary`, `VmDecalTreeNode` mirroring `FDecalLib::AddDecal` and scan/get helpers in `decallib.cpp`.

Tests: `VmDecalLibraryTests` (insert, replace-by-name, duplicate spawn id clearing).

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
