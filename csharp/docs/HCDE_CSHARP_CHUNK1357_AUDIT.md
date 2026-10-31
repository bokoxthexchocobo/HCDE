# Chunk 1357 — Drawn subsectors, portal and poly serialize, reverb, and level sounds from p_saveg.cpp (336-592).

## Scope

Drawn subsectors, portal and poly serialize, reverb, and level sounds from p_saveg.cpp (336-592).

## Native ranges

- `src/p_saveg.cpp` **LevelSerialize** 336–592 (257 lines)

**Credited native LOC:** 257 (tally `nativeLoc` 257).

## Managed

`VmLevelSerialize1357Slice.cs`
Managed LOC (tally): 455.
Conversion commit: `c2d7a7a11424643afa4933cd774fad28f67f3a04`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Drawn subsectors (**346–451**) | yes |
| Line and sector portals (**459–497**) | yes |
| Poly, reverb, zone, and sounds (**505–585**) | yes |
| Band hull (**336–592**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| SerializePlayers | the player archive starts at the next function |
| S_FindEnvironment | the reverb table lives outside this file |
| RotatePolyobj | the poly move lives outside this file |
| S_SerializeSounds | the channel archive lives outside this file |

Recalculate ORs the drawn bit when a seg's linedef is non-null and the mapped bit is set. It does not clear a flag that is already set. A null line list throws NullReferenceException. Six subsectors share one byte. Values below 10 are digits, below 36 are uppercase, below 62 are lowercase, 62 is a dash, and 63 is a plus. The write records the vertex count taken after the array opens, the subsector count taken at entry, and the string with no trailing NUL. A closed array records nothing else. A read succeeds only when both counts match. A bad character keeps earlier groups and then recalculates. A NUL ends the string before later characters. A null string throws after the array ends and does not recalculate. A closed read does not recalculate. A clear bit does not erase a flag that is already set.

Line and sector portals write their fields only when the object opens. The default pointer is unused. Byte fields truncate on read. The poly angle and start are copied before any field is written. Later fields read the live poly, so a callback during the angle field can change them, and the recorded start stays the copy. On a read with original points, rotate runs with the archived angle and true, then the delta loses the start after that rotate, then move runs with true even when rotate returns false. The poly's own angle is not replaced by the archive. An empty original list or a closed object skips both calls. A missing hook throws InvalidOperationException.

Reverb stores 0 when reading or when the container is null, otherwise the id. The int serializer runs first. Find runs only on a read and may return null. The zone forwards its environment and passes a null default into that overload. Sounds run only when the level is primary. The order is the sound archive, then the music-volume snapshot, then the music query on a write. The order byte truncates the returned int. The music name stops at the first NUL on write. A read calls the music change with the archived name and order. A false result calls the map-music reset. The volume call always receives the archived volume. A missing hook throws before the later call.

| Managed (VmLevelSerialize1357Slice.cs) | Native (p_saveg.cpp) |
|----------------------------------------|----------------------|
| Drawn subsectors, portals, poly, reverb, and sounds | **336–592** |
| Band hull (exclusive slice) | **336–592** |

### Chunk **1357** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **257** |
| **Net headline** | **+257** vs the prior chunk |


**Post-review ratio:** 455/257 managed/native ≈ 177% (src file LOC at HEAD; see tally).

## Tests

- `VmLevelSerialize1357SliceTests.DrawnSubsectors`
- `VmLevelSerialize1357SliceTests.PortalsPolyReverbAndSounds`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1357 --to 1357` (0 hard / 0 soft).

## Seams

S_FindEnvironment, S_SerializeSounds, S_GetMusic, S_ChangeMusic, SetMusic, SetMusicVolume, RotatePolyobj, and MovePolyobj stay injected. The int serializer is the archive. isPrimaryLevel is the level flag.

## Credit rationale

UNCredited p_saveg.cpp 336-592 (+257). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines. It is not a tie.
