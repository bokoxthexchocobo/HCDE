# Chunk 489 — SpawnMapThing index wrapper and P_SpawnPuff state/sound policy

## Scope

SpawnMapThing index wrapper and P_SpawnPuff state/sound policy

## Native ranges

- `src/playsim/p_mobj.cpp` **FLevelLocals::SpawnMapThing** 7095–7106 (12 lines)
- `src/playsim/p_mobj.cpp` **P_SpawnPuff** 7116–7260 (145 lines)

**Credited native LOC:** 157 (tally `nativeLoc` 157).

## Managed

`VmPMobjSpawnPuff.cs`, `VmPMobjSpawnPuffTests.cs`
Managed LOC (tally): 108.
Conversion commit: `5646d8365a172f5ee854ca21557265fb7132370f`.

## Tests

- `VmPMobjSpawnPuffTests.Index_BeginsInvasionOnZero`
- `VmPMobjSpawnPuffTests.Puff_RequiresType`
- `VmPMobjSpawnPuffTests.Puff_SkyCrashState`
- `VmPMobjSpawnPuffTests.Puff_MeleeStateWhenClose`
- `VmPMobjSpawnPuffTests.Puff_DrawSplashWhenAllowed`
- `VmPMobjSpawnPuffTests.Puff_PlaySeeSoundOnHit`
- `VmPMobjSpawnPuffTests.Puff_CrashPriorityPrefersSky`
- `VmPMobjSpawnPuffTests.Puff_SkipEffectsWhenTemporary`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 489 --to 489` (0 hard / 0 soft).

## Seams

`Spawn`, `VMCall`, `P_DrawSplash2`, `S_Sound`, and net registration are not hosted.

## Credit rationale

Credits invasion/coop registration wrapper, puff spawn jitter, victim link flags, crash/melee state selection, and hit sound policy from `p_mobj.cpp` (7095–7106, 7116–7260).
