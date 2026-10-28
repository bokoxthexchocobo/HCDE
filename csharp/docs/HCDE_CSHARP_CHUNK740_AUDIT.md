# Chunk 740 — Seeker/tracer steering plus CheckIfSeen (`p_actionfunctions.cpp` 742–892, 1854–1878). Retired (2026-10-24): `--band` hull overlaps existing credit (action-combat-port src/playsim/p_actionfunctions.cpp 1172-1208).

## Scope

Seeker/tracer steering plus CheckIfSeen (`p_actionfunctions.cpp` 742–892, 1854–1878). Retired (2026-10-24): `--band` hull overlaps existing credit (action-combat-port src/playsim/p_actionfunctions.cpp 1172-1208).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `7d8e3b623e76f45ab6874fd1a8d37030d3686079`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `SeekerMissile` | `DEFINE_ACTION_FUNCTION(AActor, A_SeekerMissile)` 742–781 |
| `DoTracer2` | static `DoTracer2` 791–837 |
| (native macro only) | `DEFINE_ACTION_FUNCTION(AActor, A_Tracer2)` 839–845 |
| `TracerAction` | `DEFINE_ACTION_FUNCTION(AActor, A_Tracer)` 854–887 |
| `CheckIfSeen` | `DEFINE_ACTION_FUNCTION(AActor, CheckIfSeen)` 1854–1878 |

### Out-of-body uncredit (740)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **153** |
| **Out-of-body removed** | **0** |
| **Honest** | **153** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 740 --to 740` (0 hard / 0 soft).

## Seams

`P_RoughMonsterSearch`, `P_SeekerMissile`, puff/smoke spawns, `P_CheckSight`, and player iteration are injected.

## Credit rationale

Base seeker/tracer band was under 150 native; `A_Tracer` and `CheckIfSeen` are whole-function padding without floor stuffing.
