# Chunk 652 — PatchPars and PatchCodePtrs DEH sections (`d_dehacked.cpp` 2731–2925, excluding chunk285 bands)

## Scope

PatchPars and PatchCodePtrs DEH sections (`d_dehacked.cpp` 2731–2925, excluding chunk285 bands)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchPars** 2731–2795 (65 lines)
- `src/gamedata/d_dehacked.cpp` **PatchCodePtrs** 2797–2879 (83 lines)
- `src/gamedata/d_dehacked.cpp` **PatchCodePtrs** 2899–2925 (27 lines)

**Credited native LOC:** 175 (tally `nativeLoc` 175).

## Managed

`VmDehackedPatchParsCodePtr.cs`
Managed LOC (tally): 101.
Conversion commit: `e1aab5c4024dcdafcf0c695522e5d90e15d3b2d6`.

## Padding review (2026-10-21)

| Managed | Native (`d_dehacked.cpp`) |
|---------|---------------------------|
| `IsParKey` / `FindFirstWhitespaceIndex` / `TryParseParLine` | `PatchPars` 2731–2795 |
| `IsCodePtrKey` / `TryParseCodePtrReplacement` / `ShouldApplyCodePtrPatch` | `PatchCodePtrs` 2797–2879, 2899–2925 |


**Post-review ratio:** 101/175 managed/native ≈ 58% (src file LOC at HEAD; see tally).

## Tests

- `VmDehackedPatchParsCodePtrTests.TryParseParLine_MapFormat`
- `VmDehackedPatchParsCodePtrTests.TryParseParLine_EpisodeFormat`
- `VmDehackedPatchParsCodePtrTests.NormalizeActionSymbol_AddsPrefix`
- `VmDehackedPatchParsCodePtrTests.TryResolveMbfAlias_MatchesAlias`
- `VmDehackedPatchParsCodePtrTests.RemapCPosAttack_DehackedName`
- `VmDehackedPatchParsCodePtrTests.BubblePickupSpriteSort_MovesLeft`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 652 --to 652` (0 hard / 0 soft).

## Seams

`ReplaceSpriteInData`, state table mutation, and `Printf` are not hosted.

## Credit rationale

Whole `PatchPars` and `PatchCodePtrs` parsers excluding thin `PatchMusic`/`ReplaceSpriteInData` slices credited on chunk285.
