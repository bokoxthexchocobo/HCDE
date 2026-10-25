# Chunk 515 — FBehavior::LoadScriptsDirectory

## Scope

FBehavior::LoadScriptsDirectory

## Native ranges

- `src/playsim/p_acs.cpp` **FBehavior::LoadScriptsDirectory** 2794–2991 (198 lines)

**Credited native LOC:** 198 (tally `nativeLoc` 198).

## Managed

`VmAcsLoadScriptsDirectory.cs`
Managed LOC (tally): 111.
Conversion commit: `2800f4188f14a4d26521f8ce6e0c06eb5849cd4b`.

## Padding review (2026-10-21)

| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `DecodeOldScriptNumber` / `DecodeOldScriptType` / `CountEnhancedScriptsBy12` / `CountLittleEnhancedScriptsBy8` / `IsAcsMagicHeader` / `ShouldSkipEnhancedChunks` / `InitScriptVarCount` / `ShouldSortScripts` / `IsDuplicateScriptNumber` / `ShouldSwapClosedFirst` / `SnamIndexToNameTableIndex` / `IsNamedScriptNumber` / `SaryChunkLargeEnough` / `FlagChunkEntryCount` / `LoopHasMoreFlagEntries` / `CompareScriptNumber` / `EnhancedUsesIntermediateLayout` / `MapOldScriptEntry` / `MapEnhancedScriptEntry` / `TryResolveDuplicateSwap` / `FindChunkPresent` / `EnhancedScriptEntrySize` / `ScriptDirectoryHeaderSkipWords` / `PtrOffsetAfterCount` / `HasScriptDirectoryChunk` / `OldFormatDuplicateScanEnabled` / `MaxScriptIndex` / `CanCompareAdjacent` / `SflgChunkPayloadOffset` / `SvctChunkPayloadOffset` / `SaryScriptNumberOffset` / `SnamStringTableOffset` / `SnamLookupBaseIndex` / `ResolveSnamStringOffset` / `ScriptPtrFound` / `ApplyScriptFlags` / `ApplyVarCount` / `VarCountChunkPresent` / `ArrayChunkPresent` / `NameChunkPresent` / `ContinueSaryWalk` / `ResetScriptsDirectory` | `LoadScriptsDirectory` 2794–2991 (2817: `if (NumScripts != 0)`; 2839: `if (scripts.b == NULL)`; 2843: `else if (*(uint32_t *)Data != MAKE_ID('A','C','S',0))`) |

### Out-of-body uncredit (515)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **198** |
| **Out-of-body removed** | **0** |
| **Honest** | **198** |


**Post-review ratio:** 111/198 managed/native ≈ 56% (src file LOC at HEAD; see tally).

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
