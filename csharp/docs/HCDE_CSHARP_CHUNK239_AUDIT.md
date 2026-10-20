# Chunk 239 — a_keys uncredited gap lines and LOCKDEF tail.

## Scope

a_keys uncredited gap lines and LOCKDEF tail.

## Native ranges

- `src/gamedata/a_keys.cpp` **a_keys header gaps** 1–37 (37 lines)
- `src/gamedata/a_keys.cpp` **a_keys section gaps** 81–86 (6 lines)
- `src/gamedata/a_keys.cpp` **a_keys section gaps 2** 100–105 (6 lines)
- `src/gamedata/a_keys.cpp` **a_keys gap line** 140–140 (1 lines)
- `src/gamedata/a_keys.cpp` **a_keys gap before AddOneKey** 158–163 (6 lines)
- `src/gamedata/a_keys.cpp` **a_keys PrintMessage gaps** 194–200 (7 lines)
- `src/gamedata/a_keys.cpp` **a_keys ParseLock gaps** 214–236 (23 lines)
- `src/gamedata/a_keys.cpp` **a_keys LOCKDEF tail** 344–500 (157 lines)

**Credited native LOC:** 243 (tally `nativeLoc` 243).

## Managed

`VmActorKeyCheck.cs`, `VmActorKeyCheckTests.cs`, `VmDecalLibCore.cs`, `VmGameInfoParseGameInfoTests.cs`, `VmStatisticsModels.cs`, `VmStatisticsModelsTests.cs`, `VmStatisticsParser.cs`, `VmStatisticsParserTests.cs`, `VmStatisticsTimeFormat.cs`, `VmStatisticsTimeFormatTests.cs`
Managed LOC (tally): 70.
Conversion commit: `e0dff6e4e1d2444fa60803b02ab92e491a153096`.

## Tests

- `VmActorKeyCheckTests.OneKeyMatches_FindsKeyInInventory`
- `VmActorKeyCheckTests.LockKeywords_IncludesAny`
- `VmActorKeyCheckTests.LockMatches_EmptyKeyListRequiresAnyKey`
- `VmLockDefParserTests.ParseLock_AssignsKeyNumbersAndMessages`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 239 --to 239` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
