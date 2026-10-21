# Chunk 539 — PatchPars, PatchCodePtrs, ReplaceSpriteInData pickup loop

## Scope

PatchPars, PatchCodePtrs, ReplaceSpriteInData pickup loop

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchPars** 2731–2794 (64 lines)
- `src/gamedata/d_dehacked.cpp` **PatchCodePtrs** 2797–2870 (74 lines)
- `src/gamedata/d_dehacked.cpp` **ReplaceSpriteInData** 2900–2925 (26 lines)

**Credited native LOC:** 164 (tally `nativeLoc` 164).

## Managed

`VmDehackedPatchParsCodePtr.cs`, `VmDehackedPatchParsCodePtrTests.cs`
Managed LOC (tally): 101.
Conversion commit: `e336c783ccc35618eafa49b5582837db703fbcee`.

## Tests

- `VmDehackedPatchParsCodePtrTests.TryParseParLine_MapFormat`
- `VmDehackedPatchParsCodePtrTests.TryParseParLine_EpisodeFormat`
- `VmDehackedPatchParsCodePtrTests.NormalizeActionSymbol_AddsPrefix`
- `VmDehackedPatchParsCodePtrTests.RemapCPosAttack_DehackedName`
- `VmDehackedPatchParsCodePtrTests.BubblePickupSpriteSort_MovesLeft`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 539 --to 539` (0 hard / 0 soft).

## Seams

`GetLine`, `FindLevelInfo`, `FindState`, `SetPointer`, and `PClass` lookup are not hosted.

## Credit rationale

Credits DEH [PARS] map par parsing, [CodePtr] frame pointer remap/MBF alias rules, and pickup sprite table reorder from `d_dehacked.cpp` (2731–2925).
