# Chunk 545 — DoRemove, A_Remove*, A_SetTeleFog, A_SwapTeleFog

## Scope

DoRemove, A_Remove*, A_SetTeleFog, A_SwapTeleFog

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **RMVF_flags** 4430–4439 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **DoRemove** 4441–4464 (24 lines)
- `src/playsim/p_actionfunctions.cpp` **A_RemoveTarget** 4471–4483 (13 lines)
- `src/playsim/p_actionfunctions.cpp` **A_RemoveTracer** 4490–4502 (13 lines)
- `src/playsim/p_actionfunctions.cpp` **A_RemoveMaster** 4509–4521 (13 lines)
- `src/playsim/p_actionfunctions.cpp` **A_RemoveChildren** 4528–4547 (20 lines)
- `src/playsim/p_actionfunctions.cpp` **A_RemoveSiblings** 4554–4576 (23 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Remove** 4583–4597 (15 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetTeleFog** 4607–4616 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SwapTeleFog** 4625–4635 (11 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Managed

`VmActorDoRemove.cs`, `VmActorDoRemoveTests.cs`
Managed LOC (tally): 94.
Conversion commit: `b990c221b6ab2658408d928de336a897c8d08035`.

## Tests

- `VmActorDoRemoveTests.RemoveMisc_SkipsMonsterMissileCombo`
- `VmActorDoRemoveTests.RemoveTargetPresent_WhenSet`
- `VmActorDoRemoveTests.RemoveChildEligible_RemoveAll`
- `VmActorDoRemoveTests.RemoveSiblingEligible_RequiresDistinctActor`
- `VmActorDoRemoveTests.RemoveReference_WhenNonNull`
- `VmActorDoRemoveTests.AssignTeleFogTypes_MatchesNativeFields`
- `VmActorDoRemoveTests.SwapTeleFogWhenDistinct_SkipsWhenEqual`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 545 --to 545` (0 hard / 0 soft).

## Seams

`P_RemoveThing` and thinker iteration are not hosted.

## Credit rationale

Credits native bands mirrored by `VmActorDoRemove` and named tests.
