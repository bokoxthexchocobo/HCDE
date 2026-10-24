# Chunk 409 — RenameSprites IWAD tail, RenameNerve, FixMacHexen

## Scope

RenameSprites IWAD tail, RenameNerve, FixMacHexen

## Native ranges

- `src/d_main.cpp` **RenameSprites BLOD/global** 3058–3083 (26 lines)
- `src/d_main.cpp` **RenameNerve** 3096–3164 (69 lines)
- `src/d_main.cpp` **FixMacHexen** 3177–3246 (70 lines)

**Credited native LOC:** 165 (tally `nativeLoc` 165).

## Managed

`VmDMainFilesystemPatchTests.cs`, `VmDMainFixMacHexen.cs`, `VmDMainRenameNerve.cs`, `VmDMainRenameSpritesIo.cs`
Managed LOC (tally): 124.
Conversion commit: `8d35c0fa959f771b69a7ebedd2ded4621ab61337`.

## Tests

- `VmDMainFilesystemPatchTests.ShouldRenameBlodToBlud_WhenNotDoomChex`
- `VmDMainFilesystemPatchTests.ShouldClearLump_WhenListedForDeletion`
- `VmDMainFilesystemPatchTests.MatchNerveSizeIndex_FindsUnityBuild`
- `VmDMainFilesystemPatchTests.ApplyMap00Rename_ShiftsDigitsLikeNative`
- `VmDMainFilesystemPatchTests.ExtraLumpDeletionRange_Covers299Entries`
- `VmDMainFilesystemPatchTests.IsKnownMacHexenIwadSize_IncludesRetail`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 409 --to 409` (0 hard / 0 soft).

## Seams

Filesystem mutation, MD5 IO, and `deletelumps` tables are not hosted.

## Credit rationale

Credits the `RenameSprites` BLOD/global-lump tail, `RenameNerve`, and `FixMacHexen` from `d_main.cpp`.
