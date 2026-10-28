# Chunk 209 — VOXELDEF parser

`VmVoxelDefParser` — `VOX_ReadSpriteNames`, `VOX_ReadOptions`, per-sprite voxel binding apply (`R_InitVoxels` tail).

**Seams:** Lump scanner/fileSystem voxel load (`CheckNumForFullName`, `VOX_GetVoxel`) and `FVoxelDef` heap push are not ported; `resolveSpriteIndex` callback substitutes `GetSpriteIndex`.

Tests: `VmVoxelDefParserTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
