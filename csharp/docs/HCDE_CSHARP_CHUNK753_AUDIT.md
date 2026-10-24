# Chunk 753 — Explode DEFINE tail, bounce sound, seeker helpers, object matrix, and water-level refresh (`p_mobj.cpp` 2098–2411, 4501–4518, 5402–5445).

## Scope

Explode DEFINE tail, bounce sound, seeker helpers, object matrix, and water-level refresh (`p_mobj.cpp` 2098–2411, 4501–4518, 5402–5445).

## Native ranges

- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, ExplodeMissile)** 2098–2105 (8 lines)
- `src/playsim/p_mobj.cpp` **AActor::PlayBounceSound** 2107–2130 (24 lines)
- `src/playsim/p_mobj.cpp` **P_FaceMobj** 2286–2301 (16 lines)
- `src/playsim/p_mobj.cpp` **AActor::CanSeek** 2311–2322 (12 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, CanSeek)** 2324–2329 (6 lines)
- `src/playsim/p_mobj.cpp` **P_SeekerMissile** 2340–2411 (72 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetObjectToWorldMatrix** 4501–4518 (18 lines)
- `src/playsim/p_mobj.cpp` **AActor::PlayDiveOrSurfaceSounds** 5402–5409 (8 lines)
- `src/playsim/p_mobj.cpp` **AActor::UpdateWaterLevel** 5419–5438 (20 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, UpdateWaterLevel)** 5440–5445 (6 lines)

**Credited native LOC:** 190 (tally `nativeLoc` 190).

## Managed

`VmPMobjSeekBounceSound.cs`, `VmPMobjObjectToWorldMatrix.cs`, `VmPMobjWaterLevelRefresh.cs`
Managed LOC (tally): 138.
Conversion commit: `fc617aa404d410e3cdcd2bf9f4958c5fef096052`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| (native macro only) | `DEFINE_ACTION_FUNCTION(AActor, ExplodeMissile)` 2098–2105 |
| `RunPlayBounceSound` | `AActor::PlayBounceSound` 2107–2130 |
| `FaceMobjDir` / `FaceMobjDelta` | `P_FaceMobj` 2286–2301 |
| `CanSeek` | `AActor::CanSeek` 2311–2322 |
| (native macro only) | `DEFINE_ACTION_FUNCTION(AActor, CanSeek)` 2324–2329 |
| `SeekerMissile` | `P_SeekerMissile` 2340–2411 |
| `GetObjectToWorldMatrix` | `AActor::GetObjectToWorldMatrix` 4501–4518 |
| `CallPlayDiveOrSurfaceSounds` | `AActor::PlayDiveOrSurfaceSounds` 5402–5409 |
| `UpdateWaterLevel` | `AActor::UpdateWaterLevel` 5419–5438 |
| (native macro only) | `DEFINE_ACTION_FUNCTION(AActor, UpdateWaterLevel)` 5440–5445 |


**Post-review ratio:** 138/190 managed/native ≈ 73% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjSeekBounceSoundTests.PlayBounceSound_SkipsQuiet`
- `VmPMobjSeekBounceSoundTests.PlayBounceSound_UsesSeeSound`
- `VmPMobjSeekBounceSoundTests.FaceMobjDir_ClockwiseWhenPositive`
- `VmPMobjSeekBounceSoundTests.CanSeek_BlocksCantSeek`
- `VmPMobjSeekBounceSoundTests.SeekerMissile_TracksWhenEligible`
- `VmPMobjSeekBounceSoundTests.SeekerMissile_ClearsTracerWhenUnshootable`
- `VmPMobjObjectToWorldMatrixTests.ObjectToWorldMatrix_GateRequiresBoneFlag`
- `VmPMobjObjectToWorldMatrixTests.GetObjectToWorldMatrix_FillsMatrix`
- `VmPMobjWaterLevelRefreshTests.PlayDiveOrSurfaceSounds_InvokesVirtual`
- `VmPMobjWaterLevelRefreshTests.UpdateWaterLevel_RunsSplashWhenRequested`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 753 --to 753` (0 hard / 0 soft).

## Seams

`P_ExplodeMissile`, `S_Sound`, `VMCall`, `FindModelFrame`, `SplashCheck`, and `UpdateWaterDepth` are injected.

## Credit rationale

Whole uncredited helpers after chunk726 partial explode/bounce credits; matrix and player dive hooks sit in honest sub-200 bands.
