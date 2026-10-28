# Principal audit — ACS map mutation opcodes

**Date:** 2026-10-19  
**Scope:** Native `p_acs.cpp` `ChangeFlat`, `SetLineTexture`, `ClearLineSpecial`, `DoSpawn`, `DoSpawnSpot`, `PCD_SETRESULTVALUE`, and stack/direct opcode dispatch.  
**Result:** Accepted for this slice. Seven new Playsim opcode tests pass; full Playsim suite remains green.

## Native behavior

- `ChangeFlat` resolves a BEHAVIOR string to a flat name and applies it to all sectors matching a tag (floor vs ceiling).
- `SetLineTexture` resolves a wall texture and applies it to front/back sidedefs by line id and `TEXTURE_TOP/MIDDLE/BOTTOM`.
- `ClearLineSpecial` clears `activationline->special` when present.
- `DoSpawn` / `DoSpawnSpot` spawn actors from class names with ACS fixed-point coordinates and optional TID anchors.
- `SetResultValue` stores the script result value popped from the stack.

## Managed implementation

- `AcsMapMutations.cs` centralizes flat/line/spawn helpers.
- `AcsVm` executes opcodes 65–68, 83, 97, 149–152, and 257.
- `AuthoritySimulation.SpawnAcsActor` creates map actors from Doom class names (simplified placement vs native `P_TestMobjLocation`).
- `LevelSector.CeilingPic` and `LevelSide` wall texture names are mutable for runtime ACS edits.

## Divergences

- Spawn does not honor `dmflags` monster suppression or full collision rejection on failed placement (non-`force` only rejects invalid sector index).
- `TAGSTR` library-id tagging uses the managed global string pool only (no library OR in tests).
- Print/HUD/music/console decoded-only opcodes remain unimplemented in this chunk.

## Test evidence

```text
dotnet test csharp/tests/HCDE.Playsim.Tests/HCDE.Playsim.Tests.csproj -c Release --filter FullyQualifiedName~AcsMapMutationOpcodeTests
```

Seven tests passed. Full Playsim: `16962` passed.

## LOC tally

`LOC tally: converted this run = 161 native lines (280 C# lines added); native remaining = 507,607`
