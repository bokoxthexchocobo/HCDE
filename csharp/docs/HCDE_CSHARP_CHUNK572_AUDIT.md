# Chunk 572 — A_Jump, A_RadiusDamageSelf damage selection, A_CustomRailgun through spread setup (923–934, 947–1002, 1267–1374)

## Scope

A_Jump, A_RadiusDamageSelf damage selection, A_CustomRailgun through spread setup (923–934, 947–1002, 1267–1374)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_Jump** 923–934 (12 lines)
- `src/playsim/p_actionfunctions.cpp` **A_RadiusDamageSelf** 947–1002 (56 lines)
- `src/playsim/p_actionfunctions.cpp` **A_CustomRailgun** 1267–1374 (108 lines)

**Credited native LOC:** 176 (tally `nativeLoc` 176).

## Managed

`VmActionJumpRadiusRail.cs`
Managed LOC (tally): 103.
Conversion commit: `9a592960aea4728b400b32f35bac12671554c66d`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `JumpShouldChangeState` | `A_Jump` 923–934 (`maxchance >= 256 \|\| pr_cajump() < maxchance`) |
| `RadiusDamageSteps` / `BfgDamagePerRoll` / `SumBfgDamage` / `SelectActualDamage` | `A_RadiusDamageSelf` 960–978: BFG self-damage sums `((pr_bfgselfdamage() & 7) + 1)` per step (`damageSteps=2`, `pr=2` → 6; `pr=3` → 8) |
| `CustomRailgunShouldAbort` / `NormalizeRailRange` / `NormalizeRailSparsity` / `StealthVisDirOnFire` / `SpreadJitter` | `A_CustomRailgun` 1290–1347 (defaults, aim abort, stealth, spread) |

**Post-review ratio:** 103/176 managed/native ≈ 59% (src file LOC at HEAD; see tally).

## Tests

- `VmActionJumpRadiusRailTests.JumpShouldChangeState_AlwaysWhenMaxChance256`
- `VmActionJumpRadiusRailTests.SumBfgDamage_MatchesNativePrFormula`
- `VmActionJumpRadiusRailTests.CustomRailgunShouldAbort_WhenAimWithoutTarget`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 572 --to 572` (0 hard / 0 soft).

## Seams

`P_RailAttack`, `P_DamageMobj`, `P_AimLineAttack`, flash spawn, and `FRailParams` fill are not hosted.

## Credit rationale

Whole-function ports for jump and the damage-selection prefix of radius-self; railgun credit stops before `P_RailAttack` setup.
