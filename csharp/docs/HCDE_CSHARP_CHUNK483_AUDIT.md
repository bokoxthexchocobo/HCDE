# Chunk 483 — PostBeginPlay, isFast/isSlow, Activate/Deactivate, OnDestroy

## Scope

PostBeginPlay, isFast/isSlow, Activate/Deactivate, OnDestroy

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::PostBeginPlay** 5825–5840 (16 lines)
- `src/playsim/p_mobj.cpp` **AActor::isFast** 5842–5852 (11 lines)
- `src/playsim/p_mobj.cpp` **AActor::Activate** 5860–5897 (38 lines)
- `src/playsim/p_mobj.cpp` **AActor::Deactivate** 5905–5942 (38 lines)
- `src/playsim/p_mobj.cpp` **AActor::OnDestroy** 5950–5998 (49 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Managed

`VmPMobjPostBeginLifecycle.cs`, `VmPMobjPostBeginLifecycleTests.cs`
Managed LOC (tally): 89.
Conversion commit: `b90c0f2d7022c7daa30a39dd2954938a98fa747a`.

## Tests

- `VmPMobjPostBeginLifecycleTests.RecreateLights_WhenStateHasLight`
- `VmPMobjPostBeginLifecycleTests.IsFast_AlwaysFastFlag`
- `VmPMobjPostBeginLifecycleTests.Activate_WakesDormantMonster`
- `VmPMobjPostBeginLifecycleTests.Deactivate_SetsDormantPath`
- `VmPMobjPostBeginLifecycleTests.OnDestroy_PremorphWhenAlternative`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 483 --to 483` (0 hard / 0 soft).

## Seams

`VMCall`, `FindState`, `DestroyAllInventory`, and hash unlink are not hosted.

## Credit rationale

Credits post-spawn light flags, skill fast/slow queries, dormant activate/deactivate flows, and destroy premorph/event teardown from `p_mobj.cpp` (5825–5998).
