# Chunk 1426 — S_SetListener through S_SerializeSounds in s_doomsound.cpp (852-1014).

## Scope

S_SetListener through S_SerializeSounds in s_doomsound.cpp (852-1014).

## Native ranges

- `src/sound/s_doomsound.cpp` **S_SetListener** 852–1014 (163 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmDoomSound1426Slice.cs`
Managed LOC (tally): 229.
Conversion commit: `a596b10eebc73bfec6fe52057f81b56a62c6058f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| S_SetListener (**858–887**) | yes |
| S_UpdateSounds (**896–909**) | yes |
| Channel serialize (**918–952**) | yes |
| S_SerializeSounds (**960–1014**) | yes |
| Band hull (**852–1014**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Playing queries | end at line 851 |
| Sector sound origins | start at line 1016 |
| Zone, sequence, device, and channel bodies | callees |

A listener actor stores yaw radians as a float, the sound position, and underwater only when the water level is exactly 3. Velocity is cleared. A null sector or a null zone list crashes. A zone number outside the list crashes. The environment comes from that zone and the listener is valid. A null actor stores angle 0, a zero position, no environment, and an invalid listener. A null sound engine crashes when the listener is installed. Update installs the listener first. Positive tic counts update every level once per tic. Zero and negative counts skip the level list, so a null list is safe. The world timer is read after that loop, and a null level crashes there. A closed serializer object leaves the channel alone. An open object records the channel fields in native order. A save older than 4560 divides pitch by 128 after the pitch field is visited. Actor, sector, and poly sources record their labels and can replace the source pointer. An unattached source records three point values and crashes when the point is missing. Any other source type stops with the unknown-source message before the object closes. Saving syncs the device on, writes active channels from last to first with each start time replaced by the device position and then restored, and syncs the device off. An empty list does not open the array. A refused array does not close it. A null device crashes before the first sync. A null engine crashes after that sync. Loading stops every channel, restores one channel per archive entry, marks each evicted and absolute-time, and sets the restart two tics ahead. A refused array still sets that restart. A null level crashes after the restored channels and before the restart. A null allocated channel crashes before its flags change.

| Managed (`VmDoomSound1426Slice.cs`) | Native (`s_doomsound.cpp`) |
|-------------------------------------|---------------------------|
| Listener, update, and save | **852–1014** |
| Band hull (exclusive slice) | **852–1014** |

### Chunk **1426** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **163** |
| **Net headline** | **+163** vs the prior chunk |


**Post-review ratio:** 229/163 managed/native ≈ 140% (src file LOC at HEAD; see tally).

## Tests

- `VmDoomSound1426SliceTests.ListenerAndUpdate`
- `VmDoomSound1426SliceTests.SerializeAndSave`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1426 --to 1426` (0 hard / 0 soft).

## Seams

Zone, sequence, device, and channel allocation calls stay callees. Playing queries end at line 851. Sector sound origins start at line 1016.

## Credit rationale

UNCredited s_doomsound.cpp 852-1014 (+163). This continues the sound engine after the playing queries and stops before CalcSectorSoundOrg. The band is shorter than 400 lines and is not sliced.
