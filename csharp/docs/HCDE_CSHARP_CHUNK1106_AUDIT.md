# Chunk 1106 — `FindSoundByResIDNoHash`, `FindSoundByLump`, `AddSoundLump`, `FindSoundTentative`, `CacheRandomSound`, and `GetMSLength` link branch from `s_sound.cpp` (**1501–1650**); follows **1105**.

## Scope

`FindSoundByResIDNoHash`, `FindSoundByLump`, `AddSoundLump`, `FindSoundTentative`, `CacheRandomSound`, and `GetMSLength` link branch from `s_sound.cpp` (**1501–1650**); follows **1105**.

## Native ranges

- `src/common/audio/sound/s_sound.cpp` **SoundEngine find lookup MS length** 1501–1650 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSoundEngineFindLookup1106Slice.cs`
Managed LOC (tally): 129.
Conversion commit: `15ff33c82ff25664c10b7f25eb450938fddafdb8`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `FindSoundByResIDNoHash` scan (**1514–1521**) | yes |
| `FindSoundByLump` (**1533–1541**) | yes |
| `AddSoundLump` field assign (**1553–1563**) | yes (seam) |
| `FindSoundTentative` create path (**1577–1583**) | yes |
| `CacheRandomSound` choice loop (**1596–1604**) | yes (policy) |
| `GetMSLength` random longest (**1627–1644**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1553–1562** | host sfx struct writes |
| **1603** | host `CacheSound` |
| **1652–1654** | host load/MS length outside hull |

| Managed (`VmSoundEngineFindLookup1106Slice.cs`) | Native (`s_sound.cpp`) |
|------------------------------------------------|-------------------------|
| Find/add/cache/MS-length predicates | **1501–1650** |
| Band hull (exclusive slice) | **1501–1650** |

### Chunk **1106** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1105** |


**Post-review ratio:** 129/150 managed/native ≈ 86% (src file LOC at HEAD; see tally).

## Tests

- `VmSoundEngineFindLookup1106SliceTests.ScanFindByLump_ReturnsIndexWhenPresent`
- `VmSoundEngineFindLookup1106SliceTests.ResolveFindSoundTentative_CreatesWhenMissing`
- `VmSoundEngineFindLookup1106SliceTests.RunCacheRandomSound_InvokesCacheForEachChoice`
- `VmSoundEngineFindLookup1106SliceTests.ResolveMsLength_PicksLongestRandomChoice`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1106 --to 1106` (0 hard / 0 soft).

## Seams

S_sfx mutation, `LoadSound`, `GSnd->GetMSLength`, and `CacheSound` host calls remain injected; **1651+** continues tail of `GetMSLength`.

## Credit rationale

UNCredited **`s_sound.cpp`** **1501–1650** (+150); follows **1105** (**1351–1500**).
