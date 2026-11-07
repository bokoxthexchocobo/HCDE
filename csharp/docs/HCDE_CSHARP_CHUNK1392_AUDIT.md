# Chunk 1392 — The Doom sound engine declaration and LookupMusic from s_doomsound.cpp (1-180).

## Scope

The Doom sound engine declaration and LookupMusic from s_doomsound.cpp (1-180).

## Native ranges

- `src/sound/s_doomsound.cpp` **LookupMusic** 1–180 (180 lines)

**Credited native LOC:** 180 (tally `nativeLoc` 180).

## Managed

`VmDoomSound1392Slice.cs`
Managed LOC (tally): 207.
Conversion commit: `9245d47f39da86fdaab75a6a19d41dd90e5fa627`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Includes, sndinfo cursors, and the local sndinfo declaration (**1–69**) | yes |
| DoomSoundEngine declaration, AddSoundLump, and CheckSoundLimit (**70–98**) | yes |
| LookupMusic (**108–171**) | yes |
| FindMusic banner before the already-credited extension check (**173–180**) | yes |
| Band hull (**1–180**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| IsKnownMusicExtension | starts at line 181, outside this hole |
| SoundEngine::AddSoundLump | callee; the slice forwards the lump arguments and then stores one zero in UserData |
| SoundEngine::CheckSoundLimit | callee; a non-actor source nulls the forwarded actor first |
| GStrings.CheckString | callee; NAME_None and a missing key return nothing |
| DebugTrace::Markf | callee; the slice records the music trace text |

AddSoundLump calls the base, then resizes that sfx UserData to one int and stores 0. The resid default is -1 and the near-limit default is 2. CheckSoundLimit nulls the actor when the source type is not SOURCE_Actor (2) and then forwards the call. LookupMusic copies * to the primary level music and order only in GS_LEVEL (0) or GS_TITLELEVEL (7). A null or empty name returns an empty string and does not trace. A ,CD, prefix prints the red CD warning once and returns empty. One leading slash is skipped after that check. A $ name prefixes D_ when CheckString finds a value. An alias of NAME_None (0) sets order to -1 and returns empty. Any other alias replaces the name once and does not chain. The caller's music text is left unchanged.

| Managed (`VmDoomSound1392Slice.cs`) | Native (`s_doomsound.cpp`) |
|-------------------------------------|----------------------------|
| Declaration, lump add, limit check, and LookupMusic | **1–180** |
| Band hull (exclusive slice) | **1–180** |

### Chunk **1392** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **180** |
| **Net headline** | **+180** vs the prior chunk |


**Post-review ratio:** 207/180 managed/native ≈ 115% (src file LOC at HEAD; see tally).

## Tests

- `VmDoomSound1392SliceTests.AddSoundLumpZerosUserDataAndLimitDropsNonActors`
- `VmDoomSound1392SliceTests.LookupMusicResolvesStarCdDehackedAndAliases`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1392 --to 1392` (0 hard / 0 soft).

## Seams

IsKnownMusicExtension starts at line 181 and stays outside this band. SoundEngine::AddSoundLump, SoundEngine::CheckSoundLimit, GStrings.CheckString, and DebugTrace::Markf are callees. FindMusic's body stays later.

## Credit rationale

Named start for the fresh branch: uncredited s_doomsound.cpp 1-180 (+180). The band is shorter than 400 lines and is not sliced. IsKnownMusicExtension at 181 is already credited and stays out.
