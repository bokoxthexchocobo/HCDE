# Chunk 729 — Whole static `DamageMobj` (`p_interaction.cpp` 1098–1571). See also `csharp/docs/HCDE_CSHARP_CHUNK724_728_ACCOUNTING.md`.

## Scope

Whole static `DamageMobj` (`p_interaction.cpp` 1098–1571). See also `csharp/docs/HCDE_CSHARP_CHUNK724_728_ACCOUNTING.md`.

## Native ranges

- `src/playsim/p_interaction.cpp` **DamageMobj** 1098–1571 (474 lines)
- `src/playsim/p_interaction.cpp` **DoDamageMobj** 1573–1603 (31 lines)
- `src/playsim/p_interaction.cpp` **P_DamageMobj** 1617–1657 (41 lines)
- `src/playsim/p_interaction.cpp` **AActor::PoisonMobj** 1715–1726 (12 lines)

**Credited native LOC:** 558 (tally `nativeLoc` 558).

## Managed

`VmPInteractionDamageMobj.cs`
Managed LOC (tally): 432.
Conversion commit: `d50e23ce7000ef768b71f2450fa91da21acb20e7`.

## Padding review (2026-10-21)

| Managed | Native (`p_interaction.cpp`) |
|---------|---------------------------|
| `DamageMobj` | `DamageMobj` 1098–1571 |
| `ScaleSkillDamage` | skill/mobj/friendly scaling 1194–1207 |
| `ApplyReflection` / `PickReflection` | inventory reflection 1286–1311 |
| `KickbackEligible` / `ApplyKickback` | thrust gate 1317–1328 |
| `ApplyPlayerDamage` | player invuln/armor/health 1350–1458 |
| `ApplyDrain` | power drain 1484–1513 |
| `TryMonsterBuddha` / `FinishDeath` | buddha + death 1516–1567 |
| `Shootable` | MF_SHOOTABLE/MF6_VULNERABLE 1109 |
| `VmPInteractionVirtualDamage.EvaluateDoDamageMobjMirror` (seam) | `DoDamageMobj` 1573–1603 |
| `VmPInteractionVirtualDamage.EvaluatePDamageMobjEarly` (seam) | `P_DamageMobj` 1617–1657 |
| `VmPInteractionVirtualDamage.PoisonMobjRequiresInflictor` (seam) | `AActor::PoisonMobj` 1715–1726 |


**Post-review ratio:** 432/558 managed/native ≈ 77% (src file LOC at HEAD; see tally).

## Tests

- `VmPInteractionDamageMobjTests.DamageMobj_DormantTarget_ReturnsCancel`
- `VmPInteractionDamageMobjTests.DamageMobj_PlayerInvuln_ReturnsZero`
- `VmPInteractionDamageMobjTests.DamageMobj_MonsterBuddha_SavesAtOne`
- `VmPInteractionDamageMobjTests.DamageMobj_Death_CallsDie`
- `VmPInteractionVirtualDamageTests.EvaluateDoDamageMobjMirror_SkipsMirrorActors`
- `VmPInteractionVirtualDamageTests.EvaluatePDamageMobjEarly_SkipsMirror`
- `VmPInteractionVirtualDamageTests.PoisonMobjRequiresInflictor_ActionGate`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 729 --to 729` (0 hard / 0 soft).

## Seams

`VMCall` kickback/drain, `P_DamageMobj` reflection recursion, `P_GiveBody`, `S_Sound`, `CallDie`, and event managers are injected.

## Credit rationale

Single-function oversize exception (~474 native LOC): full damage pipeline from early gates through death tail.
