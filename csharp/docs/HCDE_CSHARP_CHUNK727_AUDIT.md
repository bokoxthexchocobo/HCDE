# Chunk 727 — Whole `AActor::Die` (`p_interaction.cpp` 330–796).

## Scope

Whole `AActor::Die` (`p_interaction.cpp` 330–796).

## Native ranges

- `src/playsim/p_interaction.cpp` **AActor::Die** 330–796 (467 lines)

**Credited native LOC:** 467 (tally `nativeLoc` 467).

## Managed

`VmPInteractionActorDie.cs`
Managed LOC (tally): 316.
Conversion commit: `6ccca2f95c828b73cee1eb322102c02c406f16a5`.

## Padding review (2026-10-21)

| Managed | Native (`p_interaction.cpp`) |
|---------|---------------------------|
| `Die` | `AActor::Die` 330–796 |
| `RunMorphedDeath` / `KillDummyMorph` | morph block 336–382 |
| `MissileExplodesOnDeath` | 401–404 |
| `RunKillScripts` / corpse / player branch | 416–620, 622–720 |
| `RandomizeDeathTics` / `ResolveExtremelyDead` | 783–787; extreme pick 648–720 |
| `AdjustFragCount` / `FraglimitReached` | frag/spree 520–600 |


**Post-review ratio:** 316/467 managed/native ≈ 68% (src file LOC at HEAD; see tally).

## Tests

- `VmPInteractionActorDieTests.Die_Missile_ExplodesEarly`
- `VmPInteractionActorDieTests.Die_NoDieState_NonPlayer_Destroyed`
- `VmPInteractionActorDieTests.KillDummyMorph_MatchesNativeGate`
- `VmPInteractionActorDieTests.RandomizeDeathTics_ClampsToOne`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 727 --to 727` (0 hard / 0 soft).

## Seams

`VMCall`, `P_ExplodeMissile`, inventory `OwnerDied`, `WorldThingDied`, kill scripts, player death UI, `SetState`, and `Destroy` are injected.

## Credit rationale

Consolidates prior split Die helpers into one faithful Die entry (chunk505 retired).
