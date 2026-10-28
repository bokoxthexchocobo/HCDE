# Chunk 659 — Restart/load/singular/volume plus limit/stop/relink (`s_sound.cpp` 649–1044, 831–1022).

## Scope

Restart/load/singular/volume plus limit/stop/relink (`s_sound.cpp` 649–1044, 831–1022).

## Native ranges

- `src/common/audio/sound/s_sound.cpp` **SoundEngine::RestartChannel** 649–708 (60 lines)
- `src/common/audio/sound/s_sound.cpp` **SoundEngine::LoadSound** 718–792 (75 lines)
- `src/common/audio/sound/s_sound.cpp` **SoundEngine::CheckSingular** 802–812 (11 lines)
- `src/common/audio/sound/s_sound.cpp` **SoundEngine::ChangeSoundVolume** 1023–1043 (21 lines)
- `src/common/audio/sound/s_sound.cpp` **SoundEngine::StopSoundID** 870–882 (13 lines)
- `src/common/audio/sound/s_sound.cpp` **SoundEngine::StopSound** 892–904 (13 lines)
- `src/common/audio/sound/s_sound.cpp` **SoundEngine::StopSound** 914–928 (15 lines)
- `src/common/audio/sound/s_sound.cpp` **SoundEngine::StopActorSounds** 938–955 (18 lines)
- `src/common/audio/sound/s_sound.cpp` **SoundEngine::StopAllChannels** 963–975 (13 lines)
- `src/common/audio/sound/s_sound.cpp` **SoundEngine::RelinkSound** 985–1015 (31 lines)

**Credited native LOC:** 270 (tally `nativeLoc` 270).

## Managed

`VmSoundEngineRestartChannel.cs`, `VmSoundEngineLoadSound.cs`, `VmSoundEngineCheckSingular.cs`, `VmSoundEngineLimitStop.cs`
Managed LOC (tally): 413.
Conversion commit: `29acb6212fc9a7896e74f37bc65b03b6d4bd9fd1`.

## Padding review (2026-10-21)

| Managed | Native (`s_sound.cpp`) |
|---------|-------------------------|
| `RestartChannel` / `RestartChannelContext` | `RestartChannel` 649–708 |
| `LoadSound` / `LoadSoundContext` | `LoadSound` 718–792 |
| `CheckSingular` / `ChangeSoundVolume` | 802–812, 1023–1044 |
| `CheckSoundLimit` / stop/relink methods | `CheckSoundLimit` 831–860, 870–1022 |

### Out-of-body uncredit (659)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **308** |
| **Out-of-body removed** | **38** |
| **Honest** | **270** |


**Post-review ratio:** 413/270 managed/native ≈ 153% (src file LOC at HEAD; see tally).

## Tests

- `VmSoundEngineRestartChannelTests.RestartChannel_ClearsEvictedOnSuccess`
- `VmSoundEngineRestartChannelTests.RestartChannel_AbortsWithoutEvictedFlag`
- `VmSoundEngineLoadSoundTests.LoadSound_DecodesVocHeader`
- `VmSoundEngineLoadSoundTests.LoadSound_SharesCatalogBuffer`
- `VmSoundEngineCheckSingularTests.CheckSingular_FindsOrgId`
- `VmSoundEngineCheckSingularTests.ChangeSoundVolume_ClipsAndUpdates`
- `VmSoundEngineLimitStopTests.ShouldStopUnpositioned_MatchesAutoChannel`
- `VmSoundEngineLimitStopTests.ShouldStopActorSound_ByOrgId`
- `VmSoundEngineLimitStopTests.ShouldStopActorChannel_RangeOrAll`
- `VmSoundEngineLimitStopTests.ShouldRelinkSource_MatchesFromPointer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 659 --to 659` (0 hard / 0 soft).

## Seams

`GSnd`, `ReadSound`, `StopChannel`, and channel lists on contexts.

## Credit rationale

Merged chunk660 limit/stop cluster on whole-function boundaries.
