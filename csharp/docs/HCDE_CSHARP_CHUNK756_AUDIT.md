# Chunk 756 — Gravity cvar sanitize, actor networking/mark, `GetTics`, `P_CheckMissileSpawn`, spawn sound (`p_mobj.cpp` 122–846, 7756–7908).

## Scope

Gravity cvar sanitize, actor networking/mark, `GetTics`, `P_CheckMissileSpawn`, spawn sound (`p_mobj.cpp` 122–846, 7756–7908).

## Native ranges

- `src/playsim/p_mobj.cpp` **CUSTOM_CVAR(Float, sv_gravity)** 122–132 (11 lines)
- `src/playsim/p_mobj.cpp` **AActor::EnableNetworking** 175–184 (10 lines)
- `src/playsim/p_mobj.cpp` **AActor::PropagateMark** 192–200 (9 lines)
- `src/playsim/p_mobj.cpp` **GetTics** 834–846 (13 lines)
- `src/playsim/p_mobj.cpp` **P_CheckMissileSpawn** 7779–7862 (84 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, CheckMissileSpawn)** 7864–7869 (6 lines)
- `src/playsim/p_mobj.cpp` **P_PlaySpawnSound** 7880–7900 (21 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, PlaySpawnSound)** 7902–7908 (7 lines)

**Credited native LOC:** 161 (tally `nativeLoc` 161).

## Managed

`VmPMobjMissileSpawnCheck.cs`
Managed LOC (tally): 119.
Conversion commit: `9a84694136ab76cc2479bffb8c8125117054e4a2`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `SanitizeSvGravity` | `CUSTOM_CVAR(Float, sv_gravity, …)` 122–132 |
| `ShouldAbortNetworkingDisable` | `AActor::EnableNetworking` 175–184 |
| `PropagateMarkBehaviors` | `AActor::PropagateMark` 192–200 |
| `GetTics` | `GetTics` inline 834–846 |
| `RandomizeMissileTics` / `AdvanceSpawnPosition` / `MbfGrenadeSpawnPolicy` | `P_CheckMissileSpawn` 7779–7857 |
| `ShouldDestroyOnHorizon` / `ShouldBounceWallOnBlock` / `ShouldExplodeOnBlock` | blocked move branch 7834–7855 |
| (native macro only) | `DEFINE_ACTION_FUNCTION(AActor, CheckMissileSpawn)` 7864–7869 |
| `HasSeeSound` / `PlaySpawnSoundOnMissile` / `PlaySpawnSoundOnSpawner` / `PlaySpawnSoundAtPosition` | `P_PlaySpawnSound` 7880–7900 |
| (native macro only) | `DEFINE_ACTION_FUNCTION(AActor, PlaySpawnSound)` 7902–7908 |


**Post-review ratio:** 119/161 managed/native ≈ 74% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjMissileSpawnCheckTests.GetTics_FastHalves`
- `VmPMobjMissileSpawnCheckTests.AdvanceSpawnPosition_HalvesUntilInsideRadius`
- `VmPMobjMissileSpawnCheckTests.ShouldDestroyOnHorizon_WhenBlockedByHorizonLine`
- `VmPMobjMissileSpawnCheckTests.PlaySpawnSoundAtPosition_WhenNoSpawnerAndNotSilent`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 756 --to 756` (0 hard / 0 soft).

## Seams

`P_TryMove`, `P_ExplodeMissile`, `P_BounceWall`, `S_Sound`, and `GC::Mark` are not hosted.

## Credit rationale

Ports whole `P_CheckMissileSpawn` and `P_PlaySpawnSound` band omitted by partial chunk493 credits.
