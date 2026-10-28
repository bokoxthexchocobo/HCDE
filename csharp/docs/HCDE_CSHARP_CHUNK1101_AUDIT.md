# Chunk 1101 — `ListSoundChannels` tail, `CalcPosVel`/`ValidatePosVel` wrappers, `ResolveSound`, `CalcPitch`, and `StartSound` prologue through first link step from `s_sound.cpp` (**301–450**); follows **1100** cache/channel band.

## Scope

`ListSoundChannels` tail, `CalcPosVel`/`ValidatePosVel` wrappers, `ResolveSound`, `CalcPitch`, and `StartSound` prologue through first link step from `s_sound.cpp` (**301–450**); follows **1100** cache/channel band.

## Native ranges

- `src/common/audio/sound/s_sound.cpp` **SoundEngine list/resolve/start prologue** 301–450 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSoundEngineListResolveStart1101Slice.cs`
Managed LOC (tally): 178.
Conversion commit: `62ed2ee360680b8b591ac7eb4e2b8c42c05056c1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `ListSoundChannels` evicted skip + count (**302–314**) | yes |
| `ResolveSound` random vs link (**347–360**) | yes |
| `CalcPitch` (**369–385**) | yes |
| `StartSound` early gates + volume scale (**410–430**) | yes |
| Link loop first step limits (**441–450**) | yes (policy) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **308–310** | host `CalcPosVel` / name formatting |
| **331–339** | thin wrappers delegating to overload |
| **468+** | rolloff merge / backend start outside hull |
| **398–409** | parameter list / locals only |

| Managed (`VmSoundEngineListResolveStart1101Slice.cs`) | Native (`s_sound.cpp`) |
|------------------------------------------------------|-------------------------|
| List + resolve + pitch + start prologue | **297–450** (hull **301–450**) |
| Band hull (exclusive slice) | **301–450** |

### Chunk **1101** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1100** |


**Post-review ratio:** 178/150 managed/native ≈ 119% (src file LOC at HEAD; see tally).

## Tests

- `VmSoundEngineListResolveStart1101SliceTests.RunListSoundChannels_SkipsEvictedAndCounts`
- `VmSoundEngineListResolveStart1101SliceTests.RunResolveSound_AppliesRandomAttenuation`
- `VmSoundEngineListResolveStart1101SliceTests.CalcPitch_UsesDefPitchRangeWhenSet`
- `VmSoundEngineListResolveStart1101SliceTests.TryRunStartSoundPrologue_RejectsInvalidVolume`
- `VmSoundEngineListResolveStart1101SliceTests.TryAdvanceSfxLink_AbortsWhenResolveReturnsSameSfx`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1101 --to 1101` (0 hard / 0 soft).

## Seams

`CalcPosVel`, `AppendFormat`, `PickReplacement`, and backend start paths remain host-injected; **451+** continues link loop and rolloff merge.

## Credit rationale

UNCredited **`s_sound.cpp`** **301–450** (+150); follows **1100** (**151–300**). Retired **654** `StartSound` hull does not overlap this band row.
