# Chunk 360 — am_togglefollow and am_togglegrid console commands

## Scope

am_togglefollow and am_togglegrid console commands

## Native ranges

- `src/am_map.cpp` **CCMD am_togglefollow** 200–206 (7 lines)
- `src/am_map.cpp` **CCMD am_togglegrid** 208–212 (5 lines)

**Credited native LOC:** 12 (tally `nativeLoc` 12).

## Managed

`VmAutomapFollowGridCommands.cs`, `VmAutomapFollowGridCommandsTests.cs`
Managed LOC (tally): 17.
Conversion commit: `b80ac56c071212c8a558a82b17ad8b42a57b0ad3`.

## Tests

- `VmAutomapFollowGridCommandsTests.ToggleFollow_FlipsState`
- `VmAutomapFollowGridCommandsTests.ResolveFollowMessageKey_UsesOnString`
- `VmAutomapFollowGridCommandsTests.ResolveGridMessageKey_UsesOffString`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 360 --to 360` (0 hard / 0 soft).

## Seams

`primaryLevel->automap` and `GStrings` lookups are not hosted; CCMD bodies reduce to toggles and message keys.

## Credit rationale

Credits `CCMD(am_togglefollow)` and `CCMD(am_togglegrid)` toggle plus localized status string selection.
