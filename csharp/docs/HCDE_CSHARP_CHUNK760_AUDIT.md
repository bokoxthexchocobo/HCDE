# Chunk 760 — `P_RemoveThing`, raise/can-raise, velocity, `P_Thing_CheckProximity`, `P_Thing_Warp`, `DEFINE Warp` (`p_things.cpp` 422–535, 566–811, 818–835; excludes `P_Thing_CheckInputNum` 536–565). Retired (2026-10-24): `--band` hull overlaps existing credit (p-acs-player-input-fade-to-chunk50 src/playsim/p_things.cpp 536-565).

## Scope

`P_RemoveThing`, raise/can-raise, velocity, `P_Thing_CheckProximity`, `P_Thing_Warp`, `DEFINE Warp` (`p_things.cpp` 422–535, 566–811, 818–835; excludes `P_Thing_CheckInputNum` 536–565). Retired (2026-10-24): `--band` hull overlaps existing credit (p-acs-player-input-fade-to-chunk50 src/playsim/p_things.cpp 536-565).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `6e9e358133a0f669ea3b74288db41f166201ad56`.

## Padding review (2026-10-21)

**Trim (2026-10-22):** removed non-native classname catalog pre-filter and XML summaries; shared raise dimension helpers; dropped unused flag constants.

| Managed | Native (`p_things.cpp`) |
|---------|----------------------|
| `P_RemoveThing` | **422–435** |
| `P_Thing_Raise` / `ApplyRaiseDimensions` | **437–483** |
| `P_Thing_CanRaise` | **485–517** |
| `P_Thing_SetVelocity` | **519–534** |
| `P_Thing_CheckProximity` / `ProximityInRange` | **566–690** |
| `P_Thing_Warp` | **692–811** |
| `WarpAction` | `DEFINE_ACTION_FUNCTION(AActor, Warp)` **819–835** |

### Out-of-body uncredit (760)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **372** |
| **Out-of-body removed** | **0** |
| **Honest** | **372** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 760 --to 760` (0 hard / 0 soft).

## Seams

`P_CheckPosition`, `P_CanResurrect`, `Revive`, `COPY_AAPTR` / class-name resolution, `P_CheckSight`, thinker iteration, `P_TestMobjLocation`, `P_TryMove`, and portal displacements are injected.

## Credit rationale

Faithful VM ports for thing lifecycle, `P_Thing_CheckProximity` (replaces retired `ActorProximity.cs` — one code path via `VmPThingsRaiseRemoveWarpProximity.Check`), and warp control flow.
