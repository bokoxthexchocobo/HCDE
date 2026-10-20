# Chunk 501 — CallTakeSpecialDamage, Crash, SetIdle, SpawnHealth, raise/revive, gib, sentience

## Scope

CallTakeSpecialDamage, Crash, SetIdle, SpawnHealth, raise/revive, gib, sentience

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::CallTakeSpecialDamage** 8621–8634 (14 lines)
- `src/playsim/p_mobj.cpp` **AActor::Crash** 8636–8677 (42 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetIdle** 8679–8684 (6 lines)
- `src/playsim/p_mobj.cpp` **AActor::SpawnHealth** 8686–8703 (18 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetMaxHealth** 8705–8715 (11 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetRaiseState** 8717–8736 (20 lines)
- `src/playsim/p_mobj.cpp` **AActor::Revive** 8738–8786 (49 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetGibHealth** 8788–8799 (12 lines)
- `src/playsim/p_mobj.cpp` **AActor::IsSentient** 8804–8807 (4 lines)

**Credited native LOC:** 176 (tally `nativeLoc` 176).

## Managed

`VmPMobjCrashRevive.cs`, `VmPMobjCrashReviveTests.cs`
Managed LOC (tally): 116.
Conversion commit: `05bfebbe295faae4bae1afb3bf9f776aa8dca66c`.

## Tests

- `VmPMobjCrashReviveTests.Crash_SkipsDontCorpse`
- `VmPMobjCrashReviveTests.Crash_EligibleCorpseNotCrashed`
- `VmPMobjCrashReviveTests.Crash_PicksExtremeTyped`
- `VmPMobjCrashReviveTests.SpawnHealth_FriendlySkillFloor`
- `VmPMobjCrashReviveTests.Raise_BlocksPlayerPawn`
- `VmPMobjCrashReviveTests.Revive_FlagUnlink`
- `VmPMobjCrashReviveTests.GibHealth_NegatesSpawnHealth`
- `VmPMobjCrashReviveTests.Sentient_RequiresSeeState`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 501 --to 501` (0 hard / 0 soft).

## Seams

`FindState`, `SetState`, `UnlinkFromWorld`, `VMCall`, and `WorldThingRevived` are not hosted.

## Credit rationale

Credits crash state selection and MF3_CRASHED policy, idle fallback, skill-scaled spawn health, raise gating, revive flag/poison reset, default gib health, and sentience check from `p_mobj.cpp` (8621–8807).
