# Chunk 727 — Whole `AActor::Die` (`p_interaction.cpp` 330–796).

## Scope

Whole `AActor::Die` (`p_interaction.cpp` 330–796).

## Native ranges

- `src/playsim/p_interaction.cpp` **AActor::Die** 330–796 (467 lines)

**Credited native LOC:** 467 (tally `nativeLoc` 467).

## Managed

`VmPInteractionActorDie.cs`
Managed LOC (tally): 555.
Conversion commit: `35cfb7771d28be536905b08d8828efa7e6ce75d7`.

## Padding review (2026-10-21)

| Managed | Native (`p_interaction.cpp`) |
|---------|---------------------------|
| `Die` / `TryMorphedDeathHandoff` | `AActor::Die` 330–796; morph 336–382 |
| `MissileExplodesOnDeath` / `SetTargetFromSource` | 401–410 |
| `RunKillScripts` / `ClearShootableFloatSkullfly` / `DropGravityUnlessDontFall` / `ApplyCorpseFlag` / `MarkKilledFlags6` | 416–431 |
| `ActivateDeathSpecial` / `IncrementKilledMonsters` | 446–453 |
| `ApplySourceKillCredit` / `AdjustFragCount` / `ApplyMultikill` / `FraglimitReached` / `MaybeAnnounceSpree` | 455–614 |
| `ApplyEnvironmentKillFrag` / `ApplyPlayerDeathFlags` / `UnmorphDestroyEarly` | 622–690 |
| `DamageTypeIsExtreme` / `PickDieStateFromProbe` / `ApplyExtremeDeathHealth` / `RandomizeDeathTics` | 695–788 |
| `RunMorphedDeath` / `KillDummyMorph` / `ShouldCallRealDie` / `ClampRealMoHealthForGib` | morph gates 336–381 |
| `ResolveExtremelyDead` / `PickDieState` / `IceGenericFreeze` / `NormalizeDamageTypeForBarrel` | typed/generic death 716–760 |

### Out-of-body uncredit (727)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **467** |
| **Out-of-body removed** | **0** |
| **Honest** | **467** |


**Post-review ratio:** 555/467 managed/native ≈ 119% (src file LOC at HEAD; see tally).

## Tests

- `VmPInteractionActorDieTests.Die_Missile_ExplodesEarly`
- `VmPInteractionActorDieTests.Die_NoDieState_NonPlayer_Destroyed`
- `VmPInteractionActorDieTests.Die_ClearsShootableFloatSkullfly_AndMarksKilled`
- `VmPInteractionActorDieTests.Die_Unmorphed_DestroysBeforeDieState`
- `VmPInteractionActorDieTests.Die_Fraglimit_ExitCallback`
- `VmPInteractionActorDieTests.KillDummyMorph_MatchesNativeGate`
- `VmPInteractionActorDieTests.RandomizeDeathTics_ClampsToOne`
- `VmPInteractionActorDieTests.ClearShootableFloatSkullfly_PreservesOtherBits`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 727 --to 727` (0 hard / 0 soft).

## Seams

`VMCall`, `P_ExplodeMissile`, inventory `OwnerDied`, `WorldThingDied`, kill scripts, player death UI, `SetState`, and `Destroy` are injected.

## Credit rationale

Consolidates prior split Die helpers into one faithful Die entry (chunk505 retired).
