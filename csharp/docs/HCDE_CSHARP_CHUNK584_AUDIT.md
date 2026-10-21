# Chunk 584 — A_LineEffect linedone policy and A_WolfAttack target/sight guards (3549–3602)

## Scope

A_LineEffect linedone policy and A_WolfAttack target/sight guards (3549–3602)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_LineEffect** 3549–3571 (23 lines)
- `src/playsim/p_actionfunctions.cpp` **A_WolfAttack** 3584–3602 (19 lines)

**Credited native LOC:** 42 (tally `nativeLoc` 42).

## Managed

`VmActionLineEffect.cs`
Managed LOC (tally): 24.
Conversion commit: `c53c17784659eb01418f1ac4aeda6b179597b85d`.

## Padding review (2026-10-21)

**Fidelity review:** `A_LineEffect` credits `3558–3567`. `A_WolfAttack` credits `3597–3602`. PARAM/junk setup lines are seams.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `SkipWhenLinedone` / `MarkLinedoneWhenNonRepeating` | `A_LineEffect` 3558–3567 |
| `WolfAttackAbortsWithoutTarget` / `WolfAttackAbortsWithoutSight` | `A_WolfAttack` 3597–3602 |

**Post-review ratio:** 24/42 managed/native ≈ 57% (src file LOC at HEAD; see tally).

## Tests

- `VmActionLineEffectTests.SkipWhenLinedone_IsSet`
- `VmActionLineEffectTests.MarkLinedoneWhenNonRepeating`
- `VmActionLineEffectTests.WolfAttackAbortsWithoutTarget`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 584 --to 584` (0 hard / 0 soft).

## Seams

`P_ExecuteSpecial`, `P_CheckSight`, `A_FaceTarget`, and hitscan damage are not hosted.

## Credit rationale

Credits one-shot linedone merge and wolf attack early exits before facing/damage.
