# Chunk 1337 — The Eternity spatial audio facade from s_sound_eternity.cpp (1-331): renderer registration, spatial parameter recording, and the probe and status commands.

## Scope

The Eternity spatial audio facade from s_sound_eternity.cpp (1-331): renderer registration, spatial parameter recording, and the probe and status commands.

## Native ranges

- `src/common/audio/sound/s_sound_eternity.cpp` **EternitySoundRenderer** 1–331 (331 lines)

**Credited native LOC:** 331 (tally `nativeLoc` 331).

## Managed

`VmEternitySound1337Slice.cs`
Managed LOC (tally): 328.
Conversion commit: `77a6201f539c372d570d3b35b0d9edbb5a2f1c88`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Facade notes and includes (**1–45**) | yes |
| Spatial submit and renderer (**47–269**) | yes |
| Create and stub (**275–286**) | yes |
| Diagnostic probe (**288–302**) | yes |
| Status command (**304–331**) | yes |
| Band hull (**1–331**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Platform audio devices | not in this file |
| Vendored Eternity mixer | the facade stays silent and returns null |

`UpdateMusicParams`, `UnloadSound`, `StopChannel`, and `MarkStartTime` are empty in the native renderer, so the managed methods are empty too. IsNull stays false. LoadSound and LoadSoundRaw count a load and return a null handle. GetMSLength returns 250. GetSampleLength and GetPosition return 0. GetAudibility returns 0. GetOutputRate returns 44100. CreateStream counts a request and returns null. StartSound records the 2D volume, pitch, and channel flags. StartSound3D records the 3D volume, pitch, distance scale, position, velocity, channel, flags, and priority, casting the vectors to float. Channel volume and pitch replace only those fields. UpdateSoundParams3D records the area flag and the new vectors. UpdateListener and UpdateSounds count. Sync, pause, and inactive store the flag. SetSfxVolume and SetMusicVolume store the levels. IsValid stays true. PrintStatus, PrintDriversList, and GatherStats use the native format strings. Create sets the registration flag, constructs the renderer, and prints the selection line. The stub calls create. The probe increments the diagnostic count and, when a renderer exists, starts a 2D sound at volume 1, pitch 1, and flags 0. Status compares the backend to eternity without case, prints empty as <empty>, and prints the renderer block only when one exists. A second create replaces the current renderer and leaves the constructed flag set.

| Managed (`VmEternitySound1337Slice.cs`) | Native (`s_sound_eternity.cpp`) |
|------------------------------------------|----------------------------------|
| Create, the renderer, the probe, and status | **1–331** |
| Band hull (exclusive slice) | **1–331** |

### Chunk **1337** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **331** |
| **Net headline** | **+331** vs post-**1336** |


**Post-review ratio:** 328/331 managed/native ≈ 99% (src file LOC at HEAD; see tally).

## Tests

- `VmEternitySound1337SliceTests.RendererRecordsSpatialAndStaysSilent`
- `VmEternitySound1337SliceTests.ProbeStatusAndStubReplaceTheRenderer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1337 --to 1337` (0 hard / 0 soft).

## Seams

snd_backend is passed into the status command. Printf lines are collected on the host. Linux and Win32 audio devices stay outside this file. The mixer is still the silent placeholder: start and stream calls return null.

## Credit rationale

UNCredited s_sound_eternity.cpp 1-331 (+331). A full tally line scan shows no overlap. Whole-file voxeldef.cpp 1-331 is the same length. Path order takes this audio facade. Both are the largest remaining holes of at most 400 lines.
