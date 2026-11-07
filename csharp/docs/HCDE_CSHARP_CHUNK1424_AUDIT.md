# Chunk 1424 — FindNestedMusicFile through DoomSoundEngine::StopChannel in s_doomsound.cpp (233-574).

## Scope

FindNestedMusicFile through DoomSoundEngine::StopChannel in s_doomsound.cpp (233-574).

## Native ranges

- `src/sound/s_doomsound.cpp` **FindNestedMusicFile** 233–574 (342 lines)

**Credited native LOC:** 342 (tally `nativeLoc` 342).

## Managed

`VmDoomSound1424Slice.cs`
Managed LOC (tally): 339.
Conversion commit: `d7a654ee6f2770d93c7da2c54f36060eea2de256`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Nested music and FindMusic (**234–255**) | yes |
| S_Init and S_Shutdown (**265–320**) | yes |
| S_Start, precache, and S_InitData (**331–435**) | yes |
| Unpositioned sound and positioned start (**443–501**) | yes |
| Cache, resolve, verify, and stop (**509–573**) | yes |
| Band hull (**233–574**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Music path helpers | end at line 232 |
| Actor-pitched sound | starts at line 575 |
| Filesystem, sndinfo, and base sound-engine bodies | callees |

Nested music scans lumps from the last entry backward. A music path segment and a known extension must both match, ignoring case, and the first hit wins. A null music name or a null path does not match. FindMusic tries the music namespace, then the nested scan, then the full name. A null music name crashes on the namespace lookup. Init traces the call, installs the music callbacks, creates the engine only when it is missing, starts sound, and starts music when either nomusic or nosound is set. A null argument list crashes after sound init and before music. The master volume callback runs, a SNDCURVE lump is read when present, and the engine still initializes when the curve is absent. Shutdown stops music, stops every level sequence, and crashes when the level list is missing. A live engine shuts down and is cleared. The last song becomes empty. Sound closes only when the device is open. Level start runs only when both the device and the engine exist. It stops channels before reading the level, so a missing level crashes after that stop. A missing map info block still reloads global sndinfo. A local sndinfo name that differs, ignoring case, unloads sounds, parses the global set, and adds a local lump only when the full-name lookup is non-negative. A matching sndinfo with a different sequence parses only the sequence. Matching names do nothing. Precache runs only for the primary level. It marks sounds unused, calls the precache virtual on actors that have one, marks game and map sounds, and caches the marked set. A null level crashes after the unused mark. A null map-info sound list crashes after the game sounds. Init data clears both local names, parses global sndinfo, and parses sequence lump -1. Unpositioned pitch passes the listener source, a null rolloff, and the supplied pitch and start. The short sound call uses pitch 0 and start 0. A missing engine crashes. The S_Sound action keeps the low three channel bits and turns the remaining bits into flags. S_StartSound keeps the channel and flags apart. S_StartSoundAt stores the position as x, z, y. Cache skips the base cache when the player-reserve bit is set. Resolve asks for a skinned sound only in that case, and only an actor source is forwarded. Verify rejects a null actor, a dying actor, a silent sector, and a foreign level. A null sector crashes once the actor is alive. Maybe-local plus silent-pickup compat plays only for the listener. Magic silence forces the weapon channel. Stop clears an actor source when the system channel is live and the channel was not evicted, then always calls the base stop.

| Managed (`VmDoomSound1424Slice.cs`) | Native (`s_doomsound.cpp`) |
|-------------------------------------|---------------------------|
| Startup, playback, and channel stop | **233–574** |
| Band hull (exclusive slice) | **233–574** |

### Chunk **1424** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **342** |
| **Net headline** | **+342** vs the prior chunk |


**Post-review ratio:** 339/342 managed/native ≈ 99% (src file LOC at HEAD; see tally).

## Tests

- `VmDoomSound1424SliceTests.MusicLookupInitAndShutdown`
- `VmDoomSound1424SliceTests.LevelStartPrecacheAndPlayback`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1424 --to 1424` (0 hard / 0 soft).

## Seams

Filesystem, music, sndinfo, thinker, and base sound-engine calls stay callees. Music path matching ends at line 232. Actor-pitched sound starts at line 575.

## Credit rationale

UNCredited s_doomsound.cpp 233-574 (+342). This is the next whole-function run after D_InitGame. d_main.cpp 1-421 is declarations, and D_DoomMain is already partly credited. The band is shorter than 400 lines and is not sliced.
