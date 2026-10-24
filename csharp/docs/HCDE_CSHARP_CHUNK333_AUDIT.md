# Chunk 333 — ResolveSiblingLump tail and ApplyEMapInfoProperty through gravity

## Scope

ResolveSiblingLump tail and ApplyEMapInfoProperty through gravity

## Native ranges

- `src/gamedata/hcde_emapinfo.cpp` **ResolveSiblingLump ApplyEMapInfoProperty head** 539–663 (125 lines)

**Credited native LOC:** 125 (tally `nativeLoc` 125).

## Managed

`VmEMapInfoPropertyDispatch.cs`, `VmEMapInfoPropertyDispatchTests.cs`
Managed LOC (tally): 75.
Conversion commit: `7ce73956b19c142294394bbcda47c0686469fd0b`.

## Tests

- `VmEMapInfoPropertyDispatchTests.ShouldTryShortLumpName_RejectsLongNames`
- `VmEMapInfoPropertyDispatchTests.TryApplyLevelName_SetsTitle`
- `VmEMapInfoPropertyDispatchTests.TryApplyGravity_ZeroIsMax`
- `VmEMapInfoPropertyDispatchTests.TryApplyDisableJump_SetsFlag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 333 --to 333` (0 hard / 0 soft).

## Seams

`fileSystem` lump checks are reduced to short-name heuristics; `level_info_t` fields map onto `VmEMapInfoLevelDraft` without touching the native parse loop.

## Credit rationale

Credits `hcde_emapinfo.cpp` 539–663: sibling lump short-name fallback, `ResolveExtraDataLump` wiring, and the first `ApplyEMapInfoProperty` keys through `gravity`/`colormap`/`disable-jump` exercised by the dispatch tests.
