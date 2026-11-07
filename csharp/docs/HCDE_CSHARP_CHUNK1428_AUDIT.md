# Chunk 1428 — ReadSound through snd_reset in s_doomsound.cpp (1255-1523).

## Scope

ReadSound through snd_reset in s_doomsound.cpp (1255-1523).

## Native ranges

- `src/sound/s_doomsound.cpp` **ReadSound** 1255–1523 (269 lines)

**Credited native LOC:** 269 (tally `nativeLoc` 269).

## Managed

`VmDoomSound1428Slice.cs`
Managed LOC (tally): 395.
Conversion commit: `c5ec544ebe1173e563dca29db9588db2068f819a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| ReadSound (**1261–1268**) | yes |
| PickReplacement (**1280–1288**) | yes |
| NoiseDebug (**1297–1416**) | yes |
| sounddebug stat (**1418–1422**) | yes |
| PrintSoundList and console commands (**1430–1523**) | yes |
| Band hull (**1255–1523**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Position limits | end at line 1254 |
| File, roll, draw, spawn, and start bodies | callees |

A lump read opens the reader, fills a buffer of the lump length, and resizes it to the count actually read. A missing reader or a missing buffer crashes. Replacement walks random-header sounds. Each step rolls the RandSound generator with the choice count and continues while the chosen sound is also a header. An empty choice list still rolls, then the index crashes. A missing sound or a missing roll crashes. Debug draws the yellow title at 32 times the clean factor and the gold column labels, then returns when the channel list is null without reading the camera. A live list crashes when the camera is missing. The oldest channel is the end of the next chain and rows walk backward until the head sentinel or the screen bottom. A looping channel is brown and any other channel is grey. A non-3D channel prints dashes for the coordinates and the distance. A 3D channel prints origin X, then origin Z, then origin Y, and the distance only when the distance scale is positive. Volume uses two significant digits. The flag text colors 3, Z, U, M, N, A, L, E, and V. Audibility prints four decimals and the position prints an unsigned count. The sounddebug stat runs that draw and returns an empty string. A null engine crashes before the draw. The sound list prints a random header and its choice names, a player-reserved sound, a lump short name, a link, or a missing sound, then the pitch mask. A null sound or a missing short name crashes. The soundlist command crashes when the engine is missing. playsound does nothing without an argument, prints the missing-sound line for an unknown name, and otherwise starts channel 0 with the UI flag, volume 1, and no attenuation. loopsound requires a player, a local game, and an argument. An unknown name prints the same line. A missing level crashes before the spawn. A null icon does not start a sound. A live icon starts a looping body sound at the player plus 32 units, with idle attenuation and replacement allowed. snd_reset counts one reset.

| Managed (`VmDoomSound1428Slice.cs`) | Native (`s_doomsound.cpp`) |
|-------------------------------------|---------------------------|
| Lump read, debug, and commands | **1255–1523** |
| Band hull (exclusive slice) | **1255–1523** |

### Chunk **1428** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **269** |
| **Net headline** | **+269** vs the prior chunk |


**Post-review ratio:** 395/269 managed/native ≈ 147% (src file LOC at HEAD; see tally).

## Tests

- `VmDoomSound1428SliceTests.ReadAndReplacement`
- `VmDoomSound1428SliceTests.DebugListAndCommands`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1428 --to 1428` (0 hard / 0 soft).

## Seams

File reads, random rolls, drawing, spawn, and sound start calls stay callees. Position limits end at line 1254.

## Credit rationale

UNCredited s_doomsound.cpp 1255-1523 (+269). This finishes the sound engine after the position limits. The band is shorter than 400 lines and is not sliced.
