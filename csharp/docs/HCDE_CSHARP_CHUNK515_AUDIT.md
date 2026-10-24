# Chunk 515 — FBehavior::LoadScriptsDirectory

## Scope

FBehavior::LoadScriptsDirectory

## Native ranges

- `src/playsim/p_acs.cpp` **FBehavior::LoadScriptsDirectory** 2794–2991 (198 lines)

**Credited native LOC:** 198 (tally `nativeLoc` 198).

## Managed

(see conversion commit file list)
Managed LOC (tally): 135.
Conversion commit: `bda7da694da4b3eb9d0f951f1db00a478742d871`.

## Tests

- `VmAcsLoadScriptsDirectoryTests.DecodeOldScriptNumber_SplitsPacked`
- `VmAcsLoadScriptsDirectoryTests.CountEnhancedScriptsBy12_Divides`
- `VmAcsLoadScriptsDirectoryTests.TryResolveDuplicateSwap_ClosedWins`
- `VmAcsLoadScriptsDirectoryTests.SnamIndexToNameTableIndex_Negative`
- `VmAcsLoadScriptsDirectoryTests.ShouldSkipEnhancedChunks_OldFormat`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 515 --to 515` (0 hard / 0 soft).

## Seams

`FindChunk`, `ParseLocalArrayChunk`, `UnescapeStringTable`, and `qsort` are not hosted.

## Credit rationale

Credits ACS script directory parsing for old/enhanced/little-enhanced lumps, duplicate closed-script ordering, and SNAM/SFLG/SVCT/SARY chunks from `p_acs.cpp` (2794–2991).
