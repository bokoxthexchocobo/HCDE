# Chunk 1104 — `RestoreEvictedChannel`, `RestoreEvictedChannels`, `UpdateSounds` scan, and `GetRolloff` from `s_sound.cpp` (**1201–1350**); follows **1103** evict head.

## Scope

`RestoreEvictedChannel`, `RestoreEvictedChannels`, `UpdateSounds` scan, and `GetRolloff` from `s_sound.cpp` (**1201–1350**); follows **1103** evict head.

## Native ranges

- `src/common/audio/sound/s_sound.cpp` **SoundEngine restore update rolloff** 1201–1350 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSoundEngineRestoreUpdateRolloff1104Slice.cs`
Managed LOC (tally): 132.
Conversion commit: `fcc5ff5da741f5aef05f5addb864d80aa4155a98`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `RestoreEvictedChannel` tail policy (**1219–1237**) | yes |
| `RestoreEvictedChannels` entry (**1249–1252**) | yes (seam) |
| `UpdateSounds` 3D scan + restart timer (**1266–1287**) | yes (policy) |
| `GetRolloff` (**1296–1326**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1212–1218** | recursion prologue only |
| **1270–1275** | host `CalcPosVel` / `UpdateSoundParams3D` |
| **1280–1281** | host listener/backend |
| **1335+** | `ChannelEnded` outside hull |

| Managed (`VmSoundEngineRestoreUpdateRolloff1104Slice.cs`) | Native (`s_sound.cpp`) |
|----------------------------------------------------------|-------------------------|
| Restore/update/rolloff | **1201–1350** |
| Band hull (exclusive slice) | **1201–1350** |

### Chunk **1104** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1103** |


**Post-review ratio:** 132/150 managed/native ≈ 88% (src file LOC at HEAD; see tally).

## Tests

- `VmSoundEngineRestoreUpdateRolloff1104SliceTests.GetRolloff_ReturnsOneInsideMinDistance`
- `VmSoundEngineRestoreUpdateRolloff1104SliceTests.RunUpdateSounds_ClearsJustStartedAndRestoresWhenDue`
- `VmSoundEngineRestoreUpdateRolloff1104SliceTests.RunRestoreEvictedNode_ReturnsForgettableWithoutSysChannel`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1104 --to 1104` (0 hard / 0 soft).

## Seams

`RestartChannel`, `ReturnChannel`, `CalcPosVel`, and `GSnd` update calls remain host-injected; **1351+** continues `ChannelEnded`.

## Credit rationale

UNCredited **`s_sound.cpp`** **1201–1350** (+150); follows **1103** (**1051–1200**).
