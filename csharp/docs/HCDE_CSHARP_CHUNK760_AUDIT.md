# Chunk 760 — `P_RemoveThing`, raise/can-raise, velocity, `P_Thing_CheckProximity`, `P_Thing_Warp`, `DEFINE Warp` (`p_things.cpp` 422–535, 566–811, 818–835; excludes `P_Thing_CheckInputNum` 536–565).

## Scope

`P_RemoveThing`, raise/can-raise, velocity, `P_Thing_CheckProximity`, `P_Thing_Warp`, `DEFINE Warp` (`p_things.cpp` 422–535, 566–811, 818–835; excludes `P_Thing_CheckInputNum` 536–565).

## Native ranges

- `src/playsim/p_things.cpp` **P_RemoveThing** 422–435 (14 lines)
- `src/playsim/p_things.cpp` **P_Thing_Raise** 437–483 (47 lines)
- `src/playsim/p_things.cpp` **P_Thing_CanRaise** 485–517 (33 lines)
- `src/playsim/p_things.cpp` **P_Thing_SetVelocity** 519–534 (16 lines)
- `src/playsim/p_things.cpp` **P_Thing_CheckProximity** 566–690 (125 lines)
- `src/playsim/p_things.cpp` **P_Thing_Warp** 692–811 (120 lines)
- `src/playsim/p_things.cpp` **DEFINE_ACTION_FUNCTION(AActor, Warp)** 819–835 (17 lines)

**Credited native LOC:** 372 (tally `nativeLoc` 372).

## Managed

`VmPThingsRaiseRemoveWarpProximity.cs`
Managed LOC (tally): 533.
Conversion commit: `c2db57e528ee481b32d4d3f0e7879b8e4be517a9`.

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


**Post-review ratio:** 533/372 managed/native ≈ 143% (src file LOC at HEAD; see tally).

## Tests

- `VmPThingsRaiseRemoveWarpProximityTests.P_RemoveThing_SkipsLivePlayerMo`
- `VmPThingsRaiseRemoveWarpProximityTests.P_Thing_CanRaise_RestoresDimensions`
- `VmPThingsRaiseRemoveWarpProximityTests.P_Thing_SetVelocity_ClearsWhenNotAdd`
- `ActorProximityTests.GeometryUsesStrictHorizontalAndVerticalBodyGap`
- `ActorProximityTests.DeathFlagsAndRemovedActorsControlCounting`
- `VmPThingsRaiseRemoveWarpProximityTests.P_Thing_Warp_RestoresOnFailedFit`
- `VmPThingsRaiseRemoveWarpProximityTests.P_Thing_Warp_AppliesYawOnSuccess`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 760 --to 760` (0 hard / 0 soft).

## Seams

`P_CheckPosition`, `P_CanResurrect`, `Revive`, `COPY_AAPTR` / class-name resolution, `P_CheckSight`, thinker iteration, `P_TestMobjLocation`, `P_TryMove`, and portal displacements are injected.

## Credit rationale

Faithful VM ports for thing lifecycle, `P_Thing_CheckProximity` (replaces retired `ActorProximity.cs` — one code path via `VmPThingsRaiseRemoveWarpProximity.Check`), and warp control flow.
