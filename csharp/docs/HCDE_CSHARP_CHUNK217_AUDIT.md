# Chunk 217 — TranslateLineDef flag translation and zero special.

## Scope

TranslateLineDef flag translation and zero special.

## Native ranges

- `src/gamedata/p_xlat.cpp` **FLevelLocals::TranslateLineDef flags** 49–97 (49 lines)
- `src/gamedata/p_xlat.cpp` **FLevelLocals::TranslateLineDef zero special** 108–116 (9 lines)

**Credited native LOC:** 58 (tally `nativeLoc` 58).

## Managed

`VmXlatLineFlagTranslate.cs`, `VmXlatLineFlagTranslateTests.cs`
Managed LOC (tally): 58.
Conversion commit: `eea23f6ec3b4b3701e4edd42c2f97cf4cb8e740f`.

## Tests

- `VmXlatLineFlagTranslateTests.ApplyZeroSpecial_ClearsArgs`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 217 --to 217` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
