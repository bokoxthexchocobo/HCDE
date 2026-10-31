# Chunk 1100 — `CacheSound`, `UnloadSound`, channel pool link/unlink, and active-channel filters from `s_sound.cpp` (**151–300**); follows **1099** lifecycle head.

## Scope

`CacheSound`, `UnloadSound`, channel pool link/unlink, and active-channel filters from `s_sound.cpp` (**151–300**); follows **1099** lifecycle head.

## Native ranges

- `src/common/audio/sound/s_sound.cpp` **SoundEngine cache/channel pool** 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSoundEngineCacheChannel1100Slice.cs`
Managed LOC (tally): 106.
Conversion commit: `4fcf3c92d3cd040fc126cb45f27d58013219fa96`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `CacheSound` link/random branch (**158–175**) | yes |
| `UnloadSound` data clear gate (**184–192**) | yes (`RunUnloadSound`) |
| `GetChannel` free-list vs allocate (**202–218**) | yes (policy) |
| `ReturnChannel` reset (**229–234**) | yes (policy) |
| `UnlinkChannel` / `LinkChannel` (**242–266**) | yes |
| `AllActiveChannels` filter (**274–288**) | yes |
| `ListSoundChannels` evicted skip head (**297–305**) | yes (predicate) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **162–164**, **168**, **172–173** | host sfx pointer chase / load |
| **188–189**, **213–214** | host `GSnd` / `memset` |
| **306+** | list formatting outside hull |

| Managed (`VmSoundEngineCacheChannel1100Slice.cs`) | Native (`s_sound.cpp`) |
|------------------------------------------------|-------------------------|
| Cache + channel pool predicates | **158–305** |
| Band hull (exclusive slice) | **151–300** |

### Chunk **1100** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1099** |


**Post-review ratio:** 106/150 managed/native ≈ 71% (src file LOC at HEAD; see tally).

## Tests

- `VmSoundEngineCacheChannel1100SliceTests.RunCacheSound_LoadsSingleWhenNotRandom`
- `VmSoundEngineCacheChannel1100SliceTests.RunUnloadSound_UnloadsWhenValidAndAlwaysClears`
- `VmSoundEngineCacheChannel1100SliceTests.ShouldPersistActiveChannel_ExcludesUiAndTransient`
- `VmSoundEngineCacheChannel1100SliceTests.ListChannelsShouldInclude_SkipsEvicted`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1100 --to 1100` (0 hard / 0 soft).

## Seams

`GSnd`, `LoadSound`, `CacheRandomSound`, `CalcPosVel`, and channel struct mutation remain host-injected; **301+** continues `ListSoundChannels` body.

## Credit rationale

UNCredited **`s_sound.cpp`** **151–300** (+150); follows **1099** (**1–150**).
