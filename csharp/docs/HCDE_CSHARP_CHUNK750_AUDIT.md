# Chunk 750 — Fillcolor shade, pitch clamp, actor/view angles, FOV (`p_mobj.cpp` 4021–4189).

## Scope

Fillcolor shade, pitch clamp, actor/view angles, FOV (`p_mobj.cpp` 4021–4189).

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::SetShade** 4021–4025 (5 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetShade** 4027–4030 (4 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, SetShade)** 4032–4038 (7 lines)
- `src/playsim/p_mobj.cpp` **AActor::ClampPitch** 4041–4058 (18 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetPitch** 4060–4089 (30 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetAngle** 4091–4115 (25 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetRoll** 4117–4140 (24 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetViewPitch** 4142–4158 (17 lines)
- `src/playsim/p_mobj.cpp` **AActor::SetViewAngle** 4160–4171 (12 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetFOV** 4173–4189 (17 lines)

**Credited native LOC:** 159 (tally `nativeLoc` 159).

## Managed

`VmPMobjOrientationShade.cs`
Managed LOC (tally): 184.
Conversion commit: `5835b2c437bbb5518b4c2eb560b1541b6a93c4bc`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `SetShade` / `PackShadeRgb` | `AActor::SetShade` 4021–4030 |
| `SetShade` | `DEFINE_ACTION_FUNCTION(AActor, SetShade)` 4032–4038 |
| `ClampPitch` | `AActor::ClampPitch` 4041–4058 |
| `SetPitch` | `AActor::SetPitch` 4060–4089 |
| `SetAngle` | `AActor::SetAngle` 4091–4115 |
| `SetRoll` | `AActor::SetRoll` 4117–4140 |
| `SetViewPitch` | `AActor::SetViewPitch` 4142–4158 |
| `SetViewAngle` | `AActor::SetViewAngle` 4160–4171 |
| `GetFov` | `AActor::GetFOV` 4173–4189 |

### Out-of-body uncredit (750)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **160** |
| **Out-of-body removed** | **1** |
| **Honest** | **159** |


**Post-review ratio:** 184/159 managed/native ≈ 116% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjOrientationShadeTests.SetShade_PacksRgb`
- `VmPMobjOrientationShadeTests.ClampPitch_NonPlayerUses89`
- `VmPMobjOrientationShadeTests.SetPitch_InterpSetsCheat`
- `VmPMobjOrientationShadeTests.SetViewPitch_UpdatesView`
- `VmPMobjOrientationShadeTests.GetFov_NoInterpCheat`
- `VmPMobjOrientationShadeTests.GetFov_LerpsForMonster`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 750 --to 750` (0 hard / 0 soft).

## Seams

`ColorMatcher`, `deltaangle`, and player cheat flags are injected via actor state.

## Credit rationale

Whole p_mobj orientation band; excludes SetViewRoll (chunk751).
