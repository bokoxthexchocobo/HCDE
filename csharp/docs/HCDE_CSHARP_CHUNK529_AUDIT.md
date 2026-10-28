# Chunk 529 — AActor::CheckIfTargetInLOS, CheckIfInTargetLOS

## Scope

AActor::CheckIfTargetInLOS, CheckIfInTargetLOS

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **AActor::CheckIfTargetInLOS** 2551–2677 (127 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::CheckIfTargetInLOS** 2688–2759 (72 lines)

**Credited native LOC:** 199 (tally `nativeLoc` 199).

## Managed

`VmActionCheckTargetInLos.cs`
Managed LOC (tally): 204.
Conversion commit: `e218d2e8396a1ffa4a4785034ab56918dc5bee29`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-21):** Credits `CheckIfTargetInLOS` **2551–2677** and `CheckIfInTargetLOS` **2688–2759** (`p_actionfunctions.cpp`). Excluded `P_AimLineAttack`/`P_CheckSight`/`Distance3D`.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `UseMaster` / `ResolveNonPlayerHasTarget` | **2566–2568** |
| `UseTracerOrProjectile` / `SeekerOrCheckTracer` | **2570–2575** |
| `FailNullTarget` | **2582–2584**, **2714–2716** |
| `FailDeadTarget` | **2586–2588**, **2719–2721** |
| `InitialDoCheckSight` | **2591**, **2731** |
| `PlayerUsesNoAutoaim` / `PlayerAimBranch` | **2596**, **2604–2619** |
| `FailCombatantOnly` / `FailAlly` | **2624–2631** |
| `FailMaxDistance` | **2635–2637**, **2726–2728** |
| `FailCloseNoJump` / `ApplyCloseDistanceAdjustments` | **2639–2649**, **2733–2743** |
| `SightRequired` | **2655–2657**, **2755–2757** |
| `ClampFov` / `OutsideFov` | **2666–2675** |
| `ResolveInTargetLosHasTarget` | **2698–2711** |
| `InTargetLosFovCheck` | **2746–2753** |
| `PlanCheckIfTargetInLos` / `PlanCheckIfInTargetLos` | composites above (no single native function) |

### Out-of-body uncredit (529)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **199** |
| **Out-of-body removed** | **0** |
| **Honest** | **199** |


**Post-review ratio:** 204/199 managed/native ≈ 103% (src file LOC at HEAD; see tally).

## Tests

- `VmActionCheckTargetInLosTests.Plan_NonPlayer_NoTarget_Fails`
- `VmActionCheckTargetInLosTests.Plan_CombatantOnly_RejectsNonMonster`
- `VmActionCheckTargetInLosTests.Plan_AllyNoJump_BlocksFriends`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 529 --to 529` (0 hard / 0 soft).

## Seams

`P_AimLineAttack`, `P_CheckSight`, `Distance3D`, and `ACTION_RETURN_BOOL` are not hosted.

## Credit rationale

Credits JLOSF target resolution, player aim branches, distance/close gates, viewport swap, and FOV/sight planning from `p_actionfunctions.cpp` (2551–2678, 2688–2760).
