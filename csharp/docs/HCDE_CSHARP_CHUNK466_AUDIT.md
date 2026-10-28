# Chunk 466 — Slam, SpecialMissileHit, reflection, IsOkayToAttack

## Scope

Slam, SpecialMissileHit, reflection, IsOkayToAttack

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::Slam** 3769–3805 (37 lines)
- `src/playsim/p_mobj.cpp` **AActor::CallSlam** 3814–3827 (14 lines)
- `src/playsim/p_mobj.cpp` **AActor::SpecialMissileHit** 3830–3842 (13 lines)
- `src/playsim/p_mobj.cpp` **AActor::SpecialBounceHit** 3845–3857 (13 lines)
- `src/playsim/p_mobj.cpp` **AActor::AdjustReflectionAngle** 3859–3902 (44 lines)
- `src/playsim/p_mobj.cpp` **AActor::IsOkayToAttack** 3971–4019 (49 lines)

**Credited native LOC:** 170 (tally `nativeLoc` 170).

## Managed

`VmPMobjSlamReflect.cs`, `VmPMobjSlamReflectTests.cs`
Managed LOC (tally): 186.
Conversion commit: `0252d7300a884e4a5260fc7328596e0b84c2f2de`.

## Tests

- `VmPMobjSlamReflectTests.EvaluateSlamPass_WhenOnlySolid`
- `VmPMobjSlamReflectTests.OnlySlamSolid_PassesNonSolid`
- `VmPMobjSlamReflectTests.ShieldReflect_BlocksWideAngle`
- `VmPMobjSlamReflectTests.IsOkayToAttack_RejectsSelf`
- `VmPMobjSlamReflectTests.ScreenSeeker_AllowsNarrowFov`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 466 --to 466` (0 hard / 0 soft).

## Seams

`P_DamageMobj`, `P_CheckSight`, `SetState`, and `VMCall` are not hosted.

## Credit rationale

Credits skullfly slam policy, virtual missile/bounce hit hooks, reflective deflection, and attack eligibility gates from `p_mobj.cpp` after hash/missile band.
