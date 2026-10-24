# Chunk 220 — TranslateLineDef simple line translation branch.

## Scope

TranslateLineDef simple line translation branch.

## Native ranges

- `src/gamedata/p_xlat.cpp` **SimpleLineTranslations** 118–177 (60 lines)

**Credited native LOC:** 60 (tally `nativeLoc` 60).

## Managed

`VmXlatSimpleLineTranslateTests.cs`
Managed LOC (tally): 60.
Conversion commit: `3347e3a9e2e1bea4ffc9ef78f4b38d75846fdcf6`.

## Tests

- `VmXlatSimpleLineTranslateTests.TryApplySimpleTranslation_MapsSpecial`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 220 --to 220` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
