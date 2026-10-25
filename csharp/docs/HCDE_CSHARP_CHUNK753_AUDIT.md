# Chunk 753 — Explode DEFINE tail, bounce sound, seeker helpers, object matrix, and water-level refresh (`p_mobj.cpp` 2098–2411, 4501–4518, 5402–5445). Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-poison-kill-sector-chunk57 src/playsim/p_mobj.cpp 5098-5108).

## Scope

Explode DEFINE tail, bounce sound, seeker helpers, object matrix, and water-level refresh (`p_mobj.cpp` 2098–2411, 4501–4518, 5402–5445). Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-poison-kill-sector-chunk57 src/playsim/p_mobj.cpp 5098-5108).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `f56788a20f038cadc6cbeb212fb92bf27aef94e8`.

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

### Out-of-body uncredit (753)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **190** |
| **Out-of-body removed** | **0** |
| **Honest** | **190** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 753 --to 753` (0 hard / 0 soft).

## Seams

`P_ExplodeMissile`, `S_Sound`, `VMCall`, `FindModelFrame`, `SplashCheck`, and `UpdateWaterDepth` are injected.

## Credit rationale

Whole uncredited helpers after chunk726 partial explode/bounce credits; matrix and player dive hooks sit in honest sub-200 bands.
