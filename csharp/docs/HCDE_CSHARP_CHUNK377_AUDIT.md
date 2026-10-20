# Chunk 377 — Automap rotation cache, line-character transforms, and multiplayer player icon policy

## Scope

Automap rotation cache, line-character transforms, and multiplayer player icon policy

## Native ranges

- `src/am_map.cpp` **DAutomap::rotate** 2794–2814 (21 lines)
- `src/am_map.cpp` **DAutomap::rotatePoint** 2822–2831 (10 lines)
- `src/am_map.cpp` **DAutomap::drawLineCharacter** 2839–2877 (39 lines)
- `src/am_map.cpp` **DAutomap::drawPlayers** 2885–2983 (99 lines)

**Credited native LOC:** 169 (tally `nativeLoc` 169).

## Managed

`VmAutomapRotationAndPlayers.cs`, `VmAutomapRotationAndPlayersTests.cs`
Managed LOC (tally): 174.
Conversion commit: `d1dc31150b94bc16a88c6a0f0cc0e53d3ad1400d`.

## Tests

- `VmAutomapRotationAndPlayersTests.Rotate_90Degrees_SwapsAxes`
- `VmAutomapRotationAndPlayersTests.TransformLineCharacter_AppliesScaleAndOffset`
- `VmAutomapRotationAndPlayersTests.ShouldDeferSinglePlayerArrowToThingSprites_WhenCheatAndSprites`
- `VmAutomapRotationAndPlayersTests.ShouldSkipPlayerIcon_RespectsDmflags2Allies`
- `VmAutomapRotationAndPlayersTests.ResolvePlayerColorFromHsv_ReturnsRgbBytes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 377 --to 377` (0 hard / 0 soft).

## Seams

`drawMline`, `D_GetPlayerColor`, portal displacement, and cheat arrow lumps are not hosted.

## Credit rationale

Credits `DAutomap::rotate`, `rotatePoint`, `drawLineCharacter`, and `drawPlayers` from `am_map.cpp`.
