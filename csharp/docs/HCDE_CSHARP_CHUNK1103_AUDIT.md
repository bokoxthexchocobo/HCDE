# Chunk 1103 — `SetVolume`, `ChangeSoundPitch`, `SetPitch`, `GetSoundPlayingInfo`, `IsChannelUsed`, `IsSourcePlayingSomething`, and `EvictAllChannels` loop head from `s_sound.cpp` (**1051–1200**); follows **1102** StartSound mid band.

## Scope

`SetVolume`, `ChangeSoundPitch`, `SetPitch`, `GetSoundPlayingInfo`, `IsChannelUsed`, `IsSourcePlayingSomething`, and `EvictAllChannels` loop head from `s_sound.cpp` (**1051–1200**); follows **1102** StartSound mid band.

## Native ranges

- `src/common/audio/sound/s_sound.cpp` **SoundEngine volume pitch query evict** 1051–1200 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSoundEngineVolumePitchQuery1103Slice.cs`
Managed LOC (tally): 250.
Conversion commit: `60150899bb7fde180985a6b78a1773e200b74295`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `SetVolume` clamp (**1047–1052**) | yes |
| `ChangeSoundPitch` scan (**1063–1071**) | yes |
| `SetPitch` floor (**1077–1079**) | yes |
| `GetSoundPlayingInfo` (**1092–1115**) | yes |
| `IsChannelUsed` seen mask (**1131–1146**) | yes |
| `IsSourcePlayingSomething` (**1157–1167**) | yes |
| `EvictAllChannels` evict/abstime (**1187–1198**) | yes (policy) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1050–1051** | assert / host `GSnd` |
| **1032–1042** | `ChangeActorVolume` scan (outside hull) |
| **1192–1197** | host `GSnd->GetPosition` / `StopChannel` |
| **1201+** | restore recursion outside hull |

| Managed (`VmSoundEngineVolumePitchQuery1103Slice.cs`) | Native (`s_sound.cpp`) |
|-----------------------------------------------------|-------------------------|
| Volume/pitch/query/evict predicates | **1051–1200** |
| Band hull (exclusive slice) | **1051–1200** |

### Chunk **1103** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1102** |


**Post-review ratio:** 250/150 managed/native ≈ 167% (src file LOC at HEAD; see tally).

## Tests

- `VmSoundEngineVolumePitchQuery1103SliceTests.ClampUnitVolume_ClampsHighAndLow`
- `VmSoundEngineVolumePitchQuery1103SliceTests.RunSetVolume_AppliesClampedValue`
- `VmSoundEngineVolumePitchQuery1103SliceTests.CountPlayingInfo_CountsMatchingOrgId`
- `VmSoundEngineVolumePitchQuery1103SliceTests.IsChannelUsed_UsesSeenBitmask`
- `VmSoundEngineVolumePitchQuery1103SliceTests.RunEvictChannel_SetsAbstimeWhenStopping`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1103 --to 1103` (0 hard / 0 soft).

## Seams

`GSnd->ChannelVolume/Pitch`, `GetPosition`, and `StopChannel` remain host-injected; **1201+** continues restore-evict recursion.

## Credit rationale

UNCredited **`s_sound.cpp`** **1051–1200** (+150); **601–1050** overlaps credited **659** cluster.
