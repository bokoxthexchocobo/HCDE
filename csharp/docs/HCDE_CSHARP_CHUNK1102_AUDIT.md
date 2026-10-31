# Chunk 1102 — `StartSound` rolloff/singular/limit gates, channel overlap/stop scan, pause/pitch branch, and post-start flag merge from `s_sound.cpp` (**451–600**); follows **1101** prologue band.

## Scope

`StartSound` rolloff/singular/limit gates, channel overlap/stop scan, pause/pitch branch, and post-start flag merge from `s_sound.cpp` (**451–600**); follows **1101** prologue band.

## Native ranges

- `src/common/audio/sound/s_sound.cpp` **SoundEngine StartSound mid-body** 451–600 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSoundEngineStartSoundMid1102Slice.cs`
Managed LOC (tally): 152.
Conversion commit: `54fd5c56b18bc3684eb2cda17e810d6983c39907`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Attenuation scale + rolloff pick (**465–483**) | yes |
| Singular/limit evict (**485–502**) | yes |
| Evicted non-loop abort (**507–510**) | yes |
| Empty lump reject + base priority (**516–528**) | yes |
| AUTO overlap + stop scan (**531–556**) | yes |
| Pause non-loop abort (**560–563**) | yes |
| Pitch pick + evicted skip (**565–569**) | yes |
| Post-start 3D/listener flags (**598–600**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **441–463** | link loop (chunk **1101**) |
| **513**, **552**, **578–596** | host `LoadSound` / `StopChannel` / `GSnd` |
| **601+** | rumble outside hull |

| Managed (`VmSoundEngineStartSoundMid1102Slice.cs`) | Native (`s_sound.cpp`) |
|----------------------------------------------------|-------------------------|
| Rolloff/singular/limit/stop/pause/flags | **451–600** |
| Band hull (exclusive slice) | **451–600** |

### Chunk **1102** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1101** |


**Post-review ratio:** 152/150 managed/native ≈ 101% (src file LOC at HEAD; see tally).

## Tests

- `VmSoundEngineStartSoundMid1102SliceTests.ResolveRolloffMinDistance_UsesGlobalWhenUnset`
- `VmSoundEngineStartSoundMid1102SliceTests.RunSingularAndLimitGates_SetsEvictedWhenSingularPlaying`
- `VmSoundEngineStartSoundMid1102SliceTests.ShouldAbortEvictedNonLoop_MatchesNativeGate`
- `VmSoundEngineStartSoundMid1102SliceTests.RunStopMatchingChannels_StopsUnattachedPointMatch`
- `VmSoundEngineStartSoundMid1102SliceTests.MergePostStartChannelFlags_Sets3DWhenAttenuated`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1102 --to 1102` (0 hard / 0 soft).

## Seams

`LoadSound`, `GSnd` start/stop, `StopChannel`, and `CheckSoundLimit` body remain host-injected; **601+** continues rumble/HUD tail.

## Credit rationale

UNCredited **`s_sound.cpp`** **451–600** (+150); follows **1101** (**301–450**).
