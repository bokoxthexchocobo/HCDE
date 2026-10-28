# Chunk 467 — SetShade, ClampPitch, SetPitch/Angle/Roll, SetViewPitch/Angle

## Scope

SetShade, ClampPitch, SetPitch/Angle/Roll, SetViewPitch/Angle

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::SetShade** 4021–4038 (18 lines)
- `src/playsim/p_mobj.cpp` **AActor::ClampPitch** 4041–4058 (18 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetPitch** 4060–4089 (30 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetAngle** 4091–4115 (25 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetRoll** 4117–4140 (24 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetViewPitch** 4142–4158 (17 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetViewAngle** 4160–4171 (12 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetFOV** 4173–4189 (17 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetViewRoll** 4191–4201 (11 lines)

**Credited native LOC:** 172 (tally `nativeLoc` 172).

## Managed

`VmPEnemyLookEx.cs`, `VmPEnemyLookExTests.cs`, `VmPInteractionDamageEarly.cs`, `VmPInteractionDamageEarlyTests.cs`, `VmPMapBounceTraverse.cs`, `VmPMapBounceTraverseTests.cs`, `VmPMobjShadePitch.cs`, `VmPMobjShadePitchTests.cs`
Managed LOC (tally): 126.
Conversion commit: `4d6fead63aeae227fdcfd7c8e0f87d411e66042a`.

## Tests

- `VmPMobjShadePitchTests.ClampPitch_NonPlayerUses89`
- `VmPMobjShadePitchTests.SetPitch_DetectsChange`
- `VmPMobjShadePitchTests.SetAngle_ScaledNoLerp`
- `VmPMobjShadePitchTests.SetViewPitch_InterpAngles`
- `VmPMobjShadePitchTests.GetFov_LerpsWhenInterp`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 467 --to 467` (0 hard / 0 soft).

## Seams

`ColorMatcher`, `deltaangle`, and player cheat flags are not hosted.

## Credit rationale

Credits fillcolor packing, pitch clamp policy, and SPF interpolation/scaled-no-lerp branches from `p_mobj.cpp` (4019+).
