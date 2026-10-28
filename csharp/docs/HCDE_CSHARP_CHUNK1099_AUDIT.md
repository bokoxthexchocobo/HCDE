# Chunk 1099 — `SoundEnabled`, `SoundEngine::Init/Clear/Shutdown`, `MarkUsed`, and `CacheMarkedSounds` head from `s_sound.cpp` (**1–150**); follows status-bar EOF pivot (**1097–1098**).

## Scope

`SoundEnabled`, `SoundEngine::Init/Clear/Shutdown`, `MarkUsed`, and `CacheMarkedSounds` head from `s_sound.cpp` (**1–150**); follows status-bar EOF pivot (**1097–1098**).

## Native ranges

- `src/common/audio/sound/s_sound.cpp` **SoundEngine lifecycle head** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSoundEngineLifecycle1099Slice.cs`
Managed LOC (tally): 92.
Conversion commit: `b72bd1aa0d5d30ec9c5dedc49e67fd3789851126`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `SoundEnabled` gate (**45–48**) | yes |
| `DEFAULT_PITCH` (**50–53**) | yes |
| `Init` drain + curve move (**63–72**) | yes (policy) |
| `Clear` stop/unload/random reset (**80–86**) | yes (policy) |
| `Shutdown` free-list walk (**94–106**) | yes (policy) |
| `MarkUsed` validity (**114–120**) | yes |
| `CacheMarkedSounds` playing mark + used cache + unused unload (**128–150**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–44** | license/includes/CVAR macros |
| **65–70**, **82–85**, **98–105** | host channel/sfx mutations |
| **133**, **140**, **147** | host `MarkUsed` / `CacheSound` / `UnloadSound` calls |
| **151+** | `CacheSound` implementation outside hull |

| Managed (`VmSoundEngineLifecycle1099Slice.cs`) | Native (`s_sound.cpp`) |
|------------------------------------------------|-------------------------|
| Enable + lifecycle/cache predicates | **45–150** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **1099** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1098** |


**Post-review ratio:** 92/150 managed/native ≈ 61% (src file LOC at HEAD; see tally).

## Tests

- `VmSoundEngineLifecycle1099SliceTests.ResolveSoundEnabled_MatchesNativeGate`
- `VmSoundEngineLifecycle1099SliceTests.RunCacheMarkedSounds_CachesUsedAndUnloadsUnlinked`
- `VmSoundEngineLifecycle1099SliceTests.DefaultPitch_MatchesNativeEnum`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1099 --to 1099` (0 hard / 0 soft).

## Seams

CVAR registration, `ReturnChannel`, `StopAllChannels`, `UnloadAllSounds`, `CacheSound`/`UnloadSound`, and `GSnd` decode remain host-injected; **151+** continues with `CacheSound` body.

## Credit rationale

UNCredited **`s_sound.cpp`** **1–150** (+150); no overlap with **659** restart/load cluster (**649+**).
