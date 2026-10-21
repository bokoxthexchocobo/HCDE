# Chunk 285 — Whole `PatchPars`, `PatchCodePtrs`, `PatchMusic`, and `ReplaceSpriteInData` (`d_dehacked.cpp` 2731–2925).

## Scope

Whole `PatchPars`, `PatchCodePtrs`, `PatchMusic`, and `ReplaceSpriteInData` (`d_dehacked.cpp` 2731–2925).

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchPars** 2731–2794 (64 lines)
- `src/gamedata/d_dehacked.cpp` **PatchCodePtrs** 2797–2870 (74 lines)
- `src/gamedata/d_dehacked.cpp` **PatchMusic** 2872–2891 (20 lines)
- `src/gamedata/d_dehacked.cpp` **ReplaceSpriteInData** 2894–2925 (32 lines)

**Credited native LOC:** 190 (tally `nativeLoc` 190).

## Managed

`VmDehackedPatchParsCodePtr.cs`, `VmDehackedPatchMusic.cs`, `VmDehackedReplaceSprite.cs`
Managed LOC (tally): 305.
Conversion commit: `4b76b7b4a75b372857afa3b297711db66b24f1b9`.

## Padding review (2026-10-21)

| Managed | Native (`d_dehacked.cpp`) |
|---------|---------------------------|
| `RunPatchParsNative` / `TryParseParLine` / `IsParKey` | `PatchPars` 2731–2794 |
| `RunPatchCodePtrsNative` / frame-line helpers | `PatchCodePtrs` 2797–2870 |
| `RunPatchMusicNative` / `TryParseMusicLine` | `PatchMusic` 2872–2891 |
| `ReplaceSpriteInDataNative` / `BubbleSortSpriteMappings` | `ReplaceSpriteInData` 2894–2925 |


**Post-review ratio:** 305/190 managed/native ≈ 161% (src file LOC at HEAD; see tally).

## Tests

- `VmDehackedPatchParsCodePtrTests.TryParseParLine_MapFormat`
- `VmDehackedPatchParsCodePtrTests.RunPatchParsNative_SetsPar`
- `VmDehackedPatchParsCodePtrTests.RunPatchCodePtrsNative_NullPointer`
- `VmDehackedPatchParsCodePtrTests.RemapCPosAttack_DehackedName`
- `VmDehackedPatchMusicTests.RunPatchMusicNative_InsertsKey`
- `VmDehackedReplaceSpriteTests.ReplaceSpriteInDataNative_PlayerPrefix`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 285 --to 285` (0 hard / 0 soft).

## Seams

`GetLine`, `FindState`, `SetPointer`, `PClass::FindSymbol`, `DehStrings.Insert`, and `Printf` are injected.

## Credit rationale

Merged retired chunk652 bands; each credited symbol is a whole function with no interior overlap.
