# Chunk 529 — CheckIfTargetInLOS, CheckIfInTargetLOS

## Scope

`AActor::CheckIfTargetInLOS`, `AActor::CheckIfInTargetLOS`

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **CheckIfTargetInLOS** 2551–2678 (128 lines)
- `src/playsim/p_actionfunctions.cpp` **CheckIfInTargetLOS** 2688–2760 (73 lines)

**Credited native LOC:** 201.

## Managed

`csharp/src/HCDE.Playsim/VmActionCheckTargetInLos.cs` (src-only credit; tests not in `managedLoc`).

## Tests

See `chunk529.json` test list.

## Seams

`P_AimLineAttack`, `P_CheckSight`, `Distance3D`, and `ACTION_RETURN_BOOL` are not hosted.

## Credit rationale

Credits JLOSF target resolution, player aim branches, distance/close gates, viewport swap, and FOV/sight planning from `p_actionfunctions.cpp` (2551–2678, 2688–2760).
