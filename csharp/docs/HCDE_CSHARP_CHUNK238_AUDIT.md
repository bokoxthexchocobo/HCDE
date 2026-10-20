# Chunk 238 — OneKey/Keygroup/Lock check from LOCKDEFS.

## Scope

OneKey/Keygroup/Lock check from LOCKDEFS.

## Native ranges

- `src/gamedata/a_keys.cpp` **OneKey check** 38–80 (43 lines)
- `src/gamedata/a_keys.cpp` **Keygroup check** 87–99 (13 lines)
- `src/gamedata/a_keys.cpp` **Lock check** 106–139 (34 lines)
- `src/gamedata/a_keys.cpp` **Lock globals** 141–157 (17 lines)

**Credited native LOC:** 107 (tally `nativeLoc` 107).

## Managed

`VmActorKeyCheck.cs`, `VmActorKeyCheckTests.cs`, `VmDecalLibCore.cs`, `VmGameInfoParseGameInfoTests.cs`, `VmStatisticsModels.cs`, `VmStatisticsModelsTests.cs`, `VmStatisticsParser.cs`, `VmStatisticsParserTests.cs`, `VmStatisticsTimeFormat.cs`, `VmStatisticsTimeFormatTests.cs`
Managed LOC (tally): 95.
Conversion commit: `e0dff6e4e1d2444fa60803b02ab92e491a153096`.

## Tests

- `VmActorKeyCheckTests.OneKeyMatches_FindsKeyInInventory`
- `VmActorKeyCheckTests.LockKeywords_IncludesAny`
- `VmActorKeyCheckTests.LockMatches_EmptyKeyListRequiresAnyKey`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 238 --to 238` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
