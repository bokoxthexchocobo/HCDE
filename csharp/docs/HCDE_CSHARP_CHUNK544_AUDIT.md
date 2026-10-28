# Chunk 544 — A_DamageSiblings, DoKill, A_KillTarget/Tracer/Master/Children/Siblings

## Scope

A_DamageSiblings, DoKill, A_KillTarget/Tracer/Master/Children/Siblings

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_DamageSiblings** 4212–4238 (27 lines)
- `src/playsim/p_actionfunctions.cpp` **KILS** 4246–4255 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **DoKill** 4257–4287 (31 lines)
- `src/playsim/p_actionfunctions.cpp` **A_KillTarget** 4295–4311 (17 lines)
- `src/playsim/p_actionfunctions.cpp` **A_KillTracer** 4318–4334 (17 lines)
- `src/playsim/p_actionfunctions.cpp` **A_KillMaster** 4341–4357 (17 lines)
- `src/playsim/p_actionfunctions.cpp` **A_KillChildren** 4364–4388 (25 lines)
- `src/playsim/p_actionfunctions.cpp` **A_KillSiblings** 4395–4422 (28 lines)

**Credited native LOC:** 172 (tally `nativeLoc` 172).

## Managed

`VmActorDoKill.cs`, `VmActorDoKillTests.cs`
Managed LOC (tally): 92.
Conversion commit: `89acd0773dea76e1e85388e5f1d0602d2a41172b`.

## Tests

- `VmActorDoKillTests.DamageSiblingsSameMaster_RequiresDistinctActor`
- `VmActorDoKillTests.PassesKillFilter_EitherMode`
- `VmActorDoKillTests.MissileKillAllowed_RespectsFoilInvul`
- `VmActorDoKillTests.KillTargetPresent_WhenSet`
- `VmActorDoKillTests.KillChildrenMasterMatch_WhenChild`
- `VmActorDoKillTests.KillSiblingsLoop_MatchesNativeGuard`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 544 --to 544` (0 hard / 0 soft).

## Seams

`P_DamageMobj`, `P_ExplodeMissile`, `COPY_AAPTR`, and thinker iteration are not hosted.

## Credit rationale

Credits native bands mirrored by `VmActorDoKill` and named tests — not full `DEFINE_ACTION_FUNCTION` bodies.
