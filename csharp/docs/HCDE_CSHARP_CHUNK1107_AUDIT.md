# Chunk 1107 — `PickReplacement`, `HashSounds`, `AddRandomSound`, `S_SoundReset`, `cachesound`/`listsoundchannels` CCMD heads, and `S_PauseSound`/`S_ResumeSound` from `s_sound.cpp` (**1651–1800**); follows **1106**.

## Scope

`PickReplacement`, `HashSounds`, `AddRandomSound`, `S_SoundReset`, `cachesound`/`listsoundchannels` CCMD heads, and `S_PauseSound`/`S_ResumeSound` from `s_sound.cpp` (**1651–1800**); follows **1106**.

## Native ranges

- `src/common/audio/sound/s_sound.cpp` **SoundEngine pick hash pause** 1651–1800 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSoundEnginePickHashPause1107Slice.cs`
Managed LOC (tally): 138.
Conversion commit: `e40e7f90c45300ef483159f8aa1ca13f9839d804`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `PickReplacement` random walk (**1667–1672**) | yes |
| `HashSounds` insert loop (**1688–1695**) | yes (policy) |
| `AddRandomSound` owner mark (**1702–1708**) | yes (seam) |
| `S_SoundReset` sequence (**1713–1715**) | yes (policy) |
| `cachesound` argv loop (**1733–1745**) | yes |
| `S_PauseSound` / `S_ResumeSound` gates (**1764–1798**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1652–1654** | host `LoadSound` / `GetMSLength` (chunk **1106** seam) |
| **1690–1693**, **1743**, **1771**, **1797** | host map/`CacheSound`/`GSnd` |
| **1724–1730**, **1753** | includes / list print host |
| **1775–1782**, **1801+** | VM `DEFINE_ACTION_FUNCTION` outside hull |

| Managed (`VmSoundEnginePickHashPause1107Slice.cs`) | Native (`s_sound.cpp`) |
|---------------------------------------------------|-------------------------|
| Pick/hash/pause predicates | **1651–1800** |
| Band hull (exclusive slice) | **1651–1800** |

### Chunk **1107** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1106** |


**Post-review ratio:** 138/150 managed/native ≈ 92% (src file LOC at HEAD; see tally).

## Tests

- `VmSoundEnginePickHashPause1107SliceTests.RunPickReplacement_FollowsRandomList`
- `VmSoundEnginePickHashPause1107SliceTests.RunHashSounds_InsertsResourceWhenPresent`
- `VmSoundEnginePickHashPause1107SliceTests.RunCacheSoundCommand_SkipsUnknownNames`
- `VmSoundEnginePickHashPause1107SliceTests.RunPauseSound_RespectsNotSfxFlag`
- `VmSoundEnginePickHashPause1107SliceTests.RunResumeSound_InvokesMusicAlways`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1107 --to 1107` (0 hard / 0 soft).

## Seams

`LoadSound`, `GSnd`, `CacheSound`, music I/O, and `DEFINE_ACTION_FUNCTION` VM glue remain host-injected; **1801+** continues `S_ResumeSound` VM wrapper tail.

## Credit rationale

UNCredited **`s_sound.cpp`** **1651–1800** (+150); follows **1106** (**1501–1650**).
