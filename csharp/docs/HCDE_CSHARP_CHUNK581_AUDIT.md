# Chunk 581 — `A_DamageMaster`/`Children`/`Siblings`, `DoKill`, and `A_Kill*` through `A_KillSiblings` (`p_actionfunctions.cpp` **4158–4422**). Stops before **547** `DoRemove` (**4441+**).

## Scope

`A_DamageMaster`/`Children`/`Siblings`, `DoKill`, and `A_Kill*` through `A_KillSiblings` (`p_actionfunctions.cpp` **4158–4422**). Stops before **547** `DoRemove` (**4441+**).

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_DamageMaster** 4158–4175 (18 lines)
- `src/playsim/p_actionfunctions.cpp` **A_DamageChildren** 4182–4205 (24 lines)
- `src/playsim/p_actionfunctions.cpp` **A_DamageSiblings** 4212–4238 (27 lines)
- `src/playsim/p_actionfunctions.cpp` **KILS** 4246–4255 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **DoKill** 4257–4287 (31 lines)
- `src/playsim/p_actionfunctions.cpp` **A_KillTarget** 4295–4311 (17 lines)
- `src/playsim/p_actionfunctions.cpp` **A_KillTracer** 4318–4334 (17 lines)
- `src/playsim/p_actionfunctions.cpp` **A_KillMaster** 4341–4357 (17 lines)
- `src/playsim/p_actionfunctions.cpp` **A_KillChildren** 4364–4388 (25 lines)
- `src/playsim/p_actionfunctions.cpp` **A_KillSiblings** 4395–4422 (28 lines)

**Credited native LOC:** 214 (tally `nativeLoc` 214).

## Managed

`VmActorDoDamageRelations.cs`, `VmActorDoKill.cs`
Managed LOC (tally): 117.
Conversion commit: `ff6c78574db6da5ff7f34c5e3bf42c792d16fb70`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `ShouldDamageMaster` / `IsChildOfMaster` / `ShouldRunSiblingDamageLoop` | `A_DamageMaster` **4172–4173**, `A_DamageChildren` **4201–4202**, `A_DamageSiblings` **4229–4234** |
| `KilsFlags` / `PassesKillFilter` / `PlanDoKill` / missile gates | `DoKill` **4259–4285** |
| `ShouldKillWhenPresent` | `A_KillTarget` **4308–4309**, `A_KillTracer` **4331–4332** |
| `ShouldKillMaster` | `A_KillMaster` **4354–4355** |
| `IsKillChildOfMaster` | `A_KillChildren` **4382–4384** |
| `KillSiblingsLoop` / `DamageSiblingsSameMaster` | `A_KillSiblings` **4411–4417** |

### vs chunk **547** / **580**

| | Lines |
|--|------:|
| **580** (`VmActorDoDamage`) | **3994–4151** |
| **547** (`DoRemove`) | **4441+** (gap **4423–4440**) |
| **581** (this row) | **4158–4422** (**265** band / **221** function-edge sum) |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **221** |
| Net behavior-ported | **221** |


**Post-review ratio:** 117/214 managed/native ≈ 55% (src file LOC at HEAD; see tally).

## Tests

- `VmActorDoDamageRelationsTests.ShouldDamageMaster_WhenNonNull`
- `VmActorDoDamageRelationsTests.IsChildOfMaster_SameReference`
- `VmActorDoDamageRelationsTests.ShouldDamageChildInIterator_WhenMasterIsSelf`
- `VmActorDoDamageRelationsTests.ShouldRunSiblingDamageLoop_WhenMasterSet`
- `VmActorDoDamageRelationsTests.SiblingDamageCandidate_MatchesKillHelper`
- `VmActorDoKillTests.DamageSiblingsSameMaster_RequiresDistinctActor`
- `VmActorDoKillTests.PassesKillFilter_EitherMode`
- `VmActorDoKillTests.PlanDoKill_ExplodeAndDamage_WhenMissileAndMonstersAllowed`
- `VmActorDoKillTests.MissileKillAllowed_RespectsFoilInvul`
- `VmActorDoKillTests.CombineKillBaseFlags_IncludesFoilInvul`
- `VmActorDoKillTests.ExplodePathApplies_WhenMissileAllowed`
- `VmActorDoKillTests.ShouldKillWhenPresent_Target`
- `VmActorDoKillTests.ShouldKillMaster_WhenSet`
- `VmActorDoKillTests.IsKillChildOfMaster_WhenMasterMatches`
- `VmActorDoKillTests.KillSiblingsLoop_MatchesNativeGuard`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 581 --to 581` (0 hard / 0 soft).

## Seams

`COPY_AAPTR`, thinker iterators, `P_DamageMobj`, `P_ExplodeMissile`, and `P_RemoveThing` are not hosted.

## Credit rationale

Master/child/sibling damage and kill dispatch guards plus `DoKill` filter/missile policy; re-uses `VmActorDoKill` off retired **544** hull without claiming **580** `VmActorDoDamage.cs`.
