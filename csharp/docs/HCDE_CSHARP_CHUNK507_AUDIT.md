# Chunk 507 — A_Face, A_FaceTarget, A_Pain

## Scope

A_Face, A_FaceTarget, A_Pain

## Native ranges

- `src/playsim/p_enemy.cpp` **A_Face** 3046–3140 (95 lines)
- `src/playsim/p_enemy.cpp` **A_FaceTarget** 3142–3145 (4 lines)
- `src/playsim/p_enemy.cpp` **A_Pain** 3217–3269 (53 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Managed

`VmPEnemyFacePain.cs`, `VmPEnemyFacePainTests.cs`
Managed LOC (tally): 88.
Conversion commit: `8aa1d7e21a72aea0882b04b2c9f3a59b9fd2568f`.

## Tests

- `VmPEnemyFacePainTests.Face_SkipsNullOther`
- `VmPEnemyFacePainTests.Face_LimitsYaw`
- `VmPEnemyFacePainTests.Pain_LowHealthSuffix`
- `VmPEnemyFacePainTests.Pain_PlayerBranch`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 507 --to 507` (0 hard / 0 soft).

## Seams

`P_AimLineAttack`, `A_Face_ShadowHandling`, `S_FindSound`, and `VMCall` are not hosted.

## Credit rationale

Credits stealth/ambush facing, yaw/pitch clamps, FAF Z targets, and health-typed player pain sounds from `p_enemy.cpp` (3046–3269).
