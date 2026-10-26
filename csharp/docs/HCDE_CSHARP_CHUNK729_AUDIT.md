# Chunk 729 — Whole static `DamageMobj` (`p_interaction.cpp` 1098–1571). See `csharp/docs/HCDE_CSHARP_CHUNK724_728_ACCOUNTING.md`.

## Scope

Whole static `DamageMobj` (`p_interaction.cpp` 1098–1571). See `csharp/docs/HCDE_CSHARP_CHUNK724_728_ACCOUNTING.md`.

## Native ranges

- `src/playsim/p_interaction.cpp` **DamageMobj** 1098–1571 (474 lines)

**Credited native LOC:** 474 (tally `nativeLoc` 474).

## Managed

`VmPInteractionDamageMobj.cs`
Managed LOC (tally): 466.
Conversion commit: `615befb97d45c41bf4310866713c19bc93941107`.

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

### Out-of-body uncredit (729)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **474** |
| **Out-of-body removed** | **0** |
| **Honest** | **474** |


**Post-review ratio:** 466/474 managed/native ≈ 98% (src file LOC at HEAD; see tally).

## Tests

- `VmPInteractionDamageMobjTests.DamageMobj_DormantTarget_ReturnsCancel`
- `VmPInteractionDamageMobjTests.DamageMobj_PlayerInvuln_ReturnsZero`
- `VmPInteractionDamageMobjTests.DamageMobj_MonsterBuddha_SavesAtOne`
- `VmPInteractionDamageMobjTests.DamageMobj_Death_CallsDie`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 729 --to 729` (0 hard / 0 soft).

## Seams

`VMCall` kickback/drain, `P_DamageMobj` reflection recursion, `P_GiveBody`, `S_Sound`, `CallDie`, and event managers are injected.

## Credit rationale

Single-function oversize exception (~474 native LOC): full `DamageMobj` from early gates through death tail.
