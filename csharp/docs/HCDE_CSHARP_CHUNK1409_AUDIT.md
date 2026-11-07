# Chunk 1409 — dmflags, dmflags2, dmflags3, compatflags, compatflags2, and compatmode from d_main.cpp (657-912).

## Scope

dmflags, dmflags2, dmflags3, compatflags, compatflags2, and compatmode from d_main.cpp (657-912).

## Native ranges

- `src/d_main.cpp` **dmflags** 657–912 (256 lines)

**Credited native LOC:** 256 (tally `nativeLoc` 256).

## Managed

`VmDMain1409Slice.cs`
Managed LOC (tally): 235.
Conversion commit: `9deea95b613d477cd61c4c105a674dd1bbb92e77`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| dmflags (**657–684**) | yes |
| dmflags2 (**733–767**) | yes |
| dmflags3 (**803–805**) | yes |
| compatflags (**824–830**) | yes |
| compatflags2 (**832–839**) | yes |
| compatmode (**841–912**) | yes |
| Band hull (**657–912**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| d_main.cpp **651–656** | comment banner before dmflags |
| GetCaps | starts at line 974 |
| R_InitSkyMap, Net_WriteInt8, Net_WriteFloat, AM_Stop, S_UpdateSounds, CheckCheatmode, cht_DoCheat | callees |

 dmflags always reinitializes the sky. DF_NO_FREELOOK writes a center-view command. DF_NO_FOV writes a field of view only for the local authority, and a desired field of view of 0 becomes 90. dmflags2 stops the automap when DF2_NO_AUTOMAP is set and the map is active. DF2_DISALLOW_SPYING snaps the camera back to the player body, updates sounds, attaches the status bar, and shows the name during demo playback or multiplayer. The chasecam loop reads the stored dmflags2. It asks the cheat check, with false, once per in-game player only when DF2_CHASECAM is clear, and only then toggles a player who already has the chasecam cheat. The dmflags3 callback body is empty, so the port keeps the value and takes no further action. compatflags applies compatibility on every level. compatflags2 applies the second set and marks the line-side compat on every level. compatmode selects the two masks for modes 0 through 9, and any other mode uses zeros. Writing each mask runs that cvar callback.

| Managed (`VmDMain1409Slice.cs`) | Native (`d_main.cpp`) |
|----------------------------------|----------------------|
| Deathmatch flags and compatibility mode | **657–912** |
| Band hull (exclusive slice) | **657–912** |

### Chunk **1409** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **256** |
| **Net headline** | **+256** vs the prior chunk |


**Post-review ratio:** 235/256 managed/native ≈ 92% (src file LOC at HEAD; see tally).

## Tests

- `VmDMain1409SliceTests.DmFlagsFreelookFovSpyAndChase`
- `VmDMain1409SliceTests.CompatModeWritesBothMasksThroughTheirCallbacks`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1409 --to 1409` (0 hard / 0 soft).

## Seams

Sky init, net writes, automap stop, sound update, status bar, cheat mode, and the chasecam cheat stay callees. GetCaps starts at line 974.

## Credit rationale

UNCredited d_main.cpp 657-912 (+256). This is the next function-aligned slice of the d_main.cpp hole after 422-649. The comment banner at 651-656 stays outside the function. hq4x and hq4x_asm stay later because they are generated tables. The band is not sliced further.
