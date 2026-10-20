# Chunk 363 — am_toggletexture and am_setmark CCMD guards

## Scope

am_toggletexture and am_setmark CCMD guards

## Native ranges

- `src/am_map.cpp` **CCMD am_toggletexture** 214–218 (5 lines)
- `src/am_map.cpp` **CCMD am_setmark guard** 220–224 (5 lines)

**Credited native LOC:** 10 (tally `nativeLoc` 10).

## Managed

`VmAutomapFollowGridCommands.cs`, `VmAutomapFollowGridCommandsTests.cs`
Managed LOC (tally): 7.
Conversion commit: `fcee11d23b5dca7496b751e924d7349c8e3ffa1c`.

## Tests

- `VmAutomapFollowGridCommandsTests.ResolveTexturedMessageKey_UsesOnString`
- `VmAutomapFollowGridCommandsTests.ShouldRunSetMark_RequiresAutomap`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 363 --to 363` (0 hard / 0 soft).

## Seams

Engine automap instance methods are not hosted; CCMDs reduce to toggles and null guards.

## Credit rationale

Credits `CCMD(am_toggletexture)` and the `am_setmark` automap presence gate from `am_map.cpp`.
