# Chunk 1105 — `ChannelEnded` classification, `SoundDone`, `ChannelVirtualChanged`, `StopChannel`, and `UnloadAllSounds` walk head from `s_sound.cpp` (**1351–1500**); follows **1104**.

## Scope

`ChannelEnded` classification, `SoundDone`, `ChannelVirtualChanged`, `StopChannel`, and `UnloadAllSounds` walk head from `s_sound.cpp` (**1351–1500**); follows **1104**.

## Native ranges

- `src/common/audio/sound/s_sound.cpp` **SoundEngine channel callbacks stop** 1351–1500 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSoundEngineChannelCallback1105Slice.cs`
Managed LOC (tally): 94.
Conversion commit: `d0064f3ee35d766f0f06c6b24d33312f3f36a8d5`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `ChannelEnded` evicted resolution (**1346–1374**) | yes |
| `SoundDone` return (**1388–1392**) | yes (seam) |
| `ChannelVirtualChanged` (**1404–1411**) | yes |
| `StopChannel` forgettable/evict gate (**1422–1438**) | yes |
| `UnloadAllSounds` loop head (**1441–1443**) | yes (counter) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1356–1357** | host sample length/position |
| **1433** | host `GSnd->StopChannel` |
| **1444+** | per-sfx unload host calls outside partial hull |

| Managed (`VmSoundEngineChannelCallback1105Slice.cs`) | Native (`s_sound.cpp`) |
|------------------------------------------------------|-------------------------|
| Ended/virtual/stop/unload head | **1351–1500** |
| Band hull (exclusive slice) | **1351–1500** |

### Chunk **1105** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1104** |


**Post-review ratio:** 94/150 managed/native ≈ 63% (src file LOC at HEAD; see tally).

## Tests

- `VmSoundEngineChannelCallback1105SliceTests.ResolveChannelEndedEvicted_TreatsLoopAsEvicted`
- `VmSoundEngineChannelCallback1105SliceTests.ApplyVirtualFlag_SetsAndClearsBit`
- `VmSoundEngineChannelCallback1105SliceTests.RunStopChannel_ReturnsWhenNoSysChannel`
- `VmSoundEngineChannelCallback1105SliceTests.CountUnloadIndices_CountsPredicateMatches`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1105 --to 1105` (0 hard / 0 soft).

## Seams

`GSnd->GetPosition/GetSampleLength/StopChannel` and sfx unload I/O remain host-injected; **1501+** continues unload/mark helpers.

## Credit rationale

UNCredited **`s_sound.cpp`** **1351–1500** (+150); follows **1104** (**1201–1350**).
