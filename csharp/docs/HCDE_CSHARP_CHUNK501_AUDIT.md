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

`VmPMobjCrashRevive.cs`
Managed LOC (tally): 98.
Conversion commit: `f1cb60785f84851c25ac1086920decd98fe4d7f3`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `DamageTypeSet` | `CallTakeSpecialDamage` 8621–8634 (8630: `return retval;`; 8632: `else return TakeSpecialDamage(inflictor, source, damage, damagetype);`) |
| `SkipCrashWhenDontCorpse` / `CrashEligible` / `ExtremeCrashByHealth` / `PickExtremeTypedCrash` / `PickNormalTypedCrash` / `PickExtremeGenericCrash` / `PickNormalGenericCrash` / `MarkCrashedFlags3` | `Crash` 8636–8677 (8639: `if(!(flags6 & MF6_DONTCORPSE))`; 8641: `if (((flags & MF_CORPSE) || (flags6 & MF6_KILLED)) &&`; 8648: `if (DamageType != NAME_None)`) |
| `AActor::SetIdle` | `SetIdle` 8679–8684 (8682: `if (idle == NULL) idle = SpawnState;`) |
| `DefaultHealth` / `NonMonsterHealth` / `ScaleHealth` / `SpawnHealth` | `SpawnHealth` 8686–8703 (8689: `if (!(flags3 & MF3_ISMONSTER) || defhealth == 0)`; 8691: `return defhealth;`; 8693: `else if (flags & MF_FRIENDLY)`) |
| `AActor::GetMaxHealth` | `GetMaxHealth` 8705–8715 (8714: `return ret;`) |
| `RaiseRequiresCorpse` / `RaiseBlockedByTics` | `GetRaiseState` 8717–8736 (8719: `if (!(flags & MF_CORPSE))`; 8721: `return NULL; // not a monster`; 8724: `if (tics != -1 && // not lying still yet`) |
| `ReviveUnlinkNeeded` / `ReviveFriendlySpawnFlag` | `Revive` 8738–8786 (8745: `if (flagchange) UnlinkFromWorld(&ctx);`; 8747: `if (flagchange) LinkToWorld(&ctx);`; 8755: `if (SpawnFlags & MTF_FRIENDLY) flags |= MF_FRIENDLY;`) |
| `GibHealthDefault` | `GetGibHealth` 8788–8799 (8796: `return h;`; 8798: `return -SpawnHealth();`) |
| `IsSentient` | `IsSentient` 8804–8807 (8806: `return health > 0 && SeeState != NULL;`) |

### Out-of-body uncredit (501)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **176** |
| **Out-of-body removed** | **0** |
| **Honest** | **176** |


**Post-review ratio:** 98/176 managed/native ≈ 56% (src file LOC at HEAD; see tally).

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
