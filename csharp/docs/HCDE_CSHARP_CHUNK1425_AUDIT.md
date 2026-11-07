# Chunk 1425 — Actor, polyobject, sector, and point sound through S_IsActorPlayingSomething in s_doomsound.cpp (575-851).

## Scope

Actor, polyobject, sector, and point sound through S_IsActorPlayingSomething in s_doomsound.cpp (575-851).

## Native ranges

- `src/sound/s_doomsound.cpp` **S_SoundPitchActor** 575–851 (277 lines)

**Credited native LOC:** 277 (tally `nativeLoc` 277).

## Managed

`VmDoomSound1425Slice.cs`
Managed LOC (tally): 288.
Conversion commit: `0ca351b15a7cf541fbd1de51ac86cbd14af07caa`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Actor pitch, actor sound, and min/max (**581–636**) | yes |
| Poly, point, and sector sources (**645–675**) | yes |
| Play, start, and decorate (**685–724**) | yes |
| Stop, relink, volume, and pitch (**734–813**) | yes |
| Playing queries (**822–850**) | yes |
| Band hull (**575–851**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| StopChannel | ends at line 574 |
| Listener update | starts at line 852 |
| Verify and sound-engine bodies | callees |

A no-rumble flag removes a conflicting rumble bit by subtraction. Otherwise a missing rumble bit is added when haptics are on and the source is the local player, the local camera, or a missile owned by that player. A null actor crashes only when that ownership check runs. Verify then rejects a null actor, a dying actor, a silent sector, and a foreign level. A null sector crashes for a live actor. Maybe-local plus silent-pickup compat plays only for the listener. Magic silence forces the weapon channel before the start. The short actor call uses pitch 0. Min/max distance uses a linear rolloff, attenuation 1, and skips the start when verify fails. A null poly or sector crashes. A different level returns before the start. A point sound stores x, z, y and is unattached. Play returns for a null actor, crashes on a null sector, and returns for a silent or foreign actor. Without the local flag, no-stop skips the start when the actor is already playing that sound. The playing check uses channel 0 under magic silence and does not change the caller's channel. A local sound plays only for the local view, uses the unpositioned path, and forces attenuation 0. No-stop on that path listens for a listener-source sound. The short play call uses pitch 0 and start 0. Decorate looping adds loop and no-stop, a local argument adds the local flag, and the channel is split into its low three bits and the remaining flags. Stops for an actor, sector, or poly use channel -1 under magic silence and the requested channel otherwise. Stopping every actor sound passes the channel range. Relink reads the source position and passes it unless the sound-cutoff compat is set. A null source crashes. Volume changes use channel -1 under magic silence. Pitch changes keep the requested channel. Playing queries return the engine result for an actor, sector, or poly. The actor-busy query uses channel 0 under magic silence.

| Managed (`VmDoomSound1425Slice.cs`) | Native (`s_doomsound.cpp`) |
|-------------------------------------|---------------------------|
| Emitter sound and playing queries | **575–851** |
| Band hull (exclusive slice) | **575–851** |

### Chunk **1425** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **277** |
| **Net headline** | **+277** vs the prior chunk |


**Post-review ratio:** 288/277 managed/native ≈ 104% (src file LOC at HEAD; see tally).

## Tests

- `VmDoomSound1425SliceTests.RumbleSourcesAndRolloff`
- `VmDoomSound1425SliceTests.PlaybackStopsAndQueries`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1425 --to 1425` (0 hard / 0 soft).

## Seams

Verify, listener, and sound-engine calls stay callees. StopChannel ends at line 574. The listener update starts at line 852.

## Credit rationale

UNCredited s_doomsound.cpp 575-851 (+277). This continues the sound engine after StopChannel and stops before S_SetListener. The band is shorter than 400 lines and is not sliced.
