# Chunk 515 — FBehavior::LoadScriptsDirectory

## Scope

FBehavior::LoadScriptsDirectory

## Native ranges

- `src/playsim/p_acs.cpp` **FBehavior::LoadScriptsDirectory** 2794–2991 (198 lines)

**Credited native LOC:** 198 (tally `nativeLoc` 198).

## Managed

`VmAcsLoadScriptsDirectory.cs`
Managed LOC (tally): 362.
Conversion commit: `d7c1d96b55b18e217ccdf50eeeaf26452c10979b`.

## Padding review (2026-10-21)

| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `DecodeOldScriptNumber` / `DecodeOldScriptType` / `CountEnhancedScriptsBy12` / `CountLittleEnhancedScriptsBy8` / `IsAcsMagicHeader` / `ShouldSkipEnhancedChunks` / `InitScriptVarCount` / `ShouldSortScripts` / `IsDuplicateScriptNumber` / `ShouldSwapClosedFirst` / `SnamIndexToNameTableIndex` / `IsNamedScriptNumber` / `SaryChunkLargeEnough` / `FlagChunkEntryCount` / `LoopHasMoreFlagEntries` / `CompareScriptNumber` / `EnhancedUsesIntermediateLayout` / `MapOldScriptEntry` / `MapEnhancedScriptEntry` / `TryResolveDuplicateSwap` / `FindChunkPresent` / `EnhancedScriptEntrySize` / `ScriptDirectoryHeaderSkipWords` / `PtrOffsetAfterCount` / `HasScriptDirectoryChunk` / `OldFormatDuplicateScanEnabled` / `MaxScriptIndex` / `CanCompareAdjacent` / `SflgChunkPayloadOffset` / `SvctChunkPayloadOffset` / `SaryScriptNumberOffset` / `SnamStringTableOffset` / `SnamLookupBaseIndex` / `ResolveSnamStringOffset` / `ScriptPtrFound` / `ApplyScriptFlags` / `ApplyVarCount` / `VarCountChunkPresent` / `ArrayChunkPresent` / `NameChunkPresent` / `ContinueSaryWalk` / `ResetScriptsDirectory` | `LoadScriptsDirectory` 2794–2991 (2817: `if (NumScripts != 0)`; 2839: `if (scripts.b == NULL)`; 2843: `else if (*(uint32_t *)Data != MAKE_ID('A','C','S',0))`) |
| `ScriptPtrSlot` / `ScriptsDirectoryScratch` / `ResolveEnhancedScriptCount` / `PopulateOldFormatDirectory` / `PopulateEnhancedFormatDirectory` / `InitializeScriptDefaults` / `SortScriptsInPlace` / `ResolveOldFormatAdjacentDuplicates` / `ApplySflgChunkEntries` / `ApplySvctChunkEntries` / `WalkSaryChunks` / `ApplySnamNameIndices` / `RunLoadScriptsDirectory` | format switch **2812–2881**, default init **2888–2992**, duplicate swap **2902–2917**, SFLG/SVCT/SARY/SNAM passes **2920–2990** |

### Out-of-body uncredit (515)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **198** |
| **Out-of-body removed** | **0** |
| **Honest** | **198** |


**Post-review ratio:** 362/198 managed/native ≈ 183% (src file LOC at HEAD; see tally).

## Tests

- `VmAcsLoadScriptsDirectoryTests.DecodeOldScriptNumber_SplitsPacked`
- `VmAcsLoadScriptsDirectoryTests.CountEnhancedScriptsBy12_Divides`
- `VmAcsLoadScriptsDirectoryTests.TryResolveDuplicateSwap_ClosedWins`
- `VmAcsLoadScriptsDirectoryTests.SnamIndexToNameTableIndex_Negative`
- `VmAcsLoadScriptsDirectoryTests.ShouldSkipEnhancedChunks_OldFormat`
- `VmAcsLoadScriptsDirectoryTests.ResolveEnhancedScriptCount_UsesEightByteStrideForMagicHeader`
- `VmAcsLoadScriptsDirectoryTests.RunLoadScriptsDirectory_OldFormat_SortsAndSwapsClosedDuplicate`
- `VmAcsLoadScriptsDirectoryTests.ApplySflgChunkEntries_UpdatesFlagsWhenScriptExists`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 515 --to 515` (0 hard / 0 soft).

## Seams

`FindChunk`, `ParseLocalArrayChunk`, `UnescapeStringTable`, and `qsort` are not hosted.

## Credit rationale

Credits ACS script directory parsing for old/enhanced/little-enhanced lumps, duplicate closed-script ordering, and SNAM/SFLG/SVCT/SARY chunks from `p_acs.cpp` (2794–2991).
