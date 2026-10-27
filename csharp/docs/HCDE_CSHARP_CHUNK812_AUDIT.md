# Chunk 812 — Special-line recording, portal floor/ceiling probe, rip level, and projectile combat helpers from `p_map.cpp` (1098–1359).

## Scope

Special-line recording, portal floor/ceiling probe, rip level, and projectile combat helpers from `p_map.cpp` (1098–1359).

## Native ranges

- `src/playsim/p_map.cpp` **PIT_CheckLine spechit** 1098–1115 (18 lines)
- `src/playsim/p_map.cpp` **PIT_CheckPortal** 1134–1206 (73 lines)
- `src/playsim/p_map.cpp` **CheckRipLevel** 1214–1219 (6 lines)
- `src/playsim/p_map.cpp` **P_ProjectileImmune** 1227–1248 (22 lines)
- `src/playsim/p_map.cpp` **CanAttackHurt** 1250–1316 (67 lines)
- `src/playsim/p_map.cpp` **P_DoMissileDamage** 1327–1359 (33 lines)

**Credited native LOC:** 219 (tally `nativeLoc` 219).

## Portfolio delta

**Gross CLI band:** **262** lines in the UNCredited-range probe; tally credits **219** whole-function native LOC only.
**43** uncovered lines in that band are inter-function gaps, comments, or non-function lines (not credited).

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **262** |
| **Credited** (whole functions, this row) | **219** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+219** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

`VmPMapProjectileCombat.cs`
Managed LOC (tally): 221.
Conversion commit: `f21529e3c3312472a5d2c15efc389ba635822d21`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|----------------------|
| `RecordSpecialAndPortalHits` | **1098–1115** |
| `CheckPortalOpening` | **1134–1206** |
| `CheckRipLevel` | **1214–1219** |
| `ProjectileImmune` | **1227–1248** |
| `CanAttackHurt` | **1250–1316** |
| `DoMissileDamage` | **1327–1359** |

### Out-of-body uncredit (812)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **219** |
| **Out-of-body removed** | **0** |
| **Honest** | **219** |


**Post-review ratio:** 221/219 managed/native ≈ 101% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapProjectileCombatTests.RecordSpecialHits_PushesSpechit`
- `VmPMapProjectileCombatTests.CheckPortalOpening_AdjustsFloor`
- `VmPMapProjectileCombatTests.CheckPortalOpening_LowersCeiling`
- `VmPMapProjectileCombatTests.CheckRipLevel_WithinBand`
- `VmPMapProjectileCombatTests.ProjectileImmune_SameSpeciesDefaultGroup`
- `VmPMapProjectileCombatTests.CanAttackHurt_BlocksFriendlyInfightZero`
- `VmPMapProjectileCombatTests.CanAttackHurt_AllowsHostileMonsterWhenInfightNegative`
- `VmPMapProjectileCombatTests.DoMissileDamage_NegativeDamageGivesBody`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 812 --to 812` (0 hard / 0 soft).

## Seams

`P_PoisonMobj`, `P_DamageMobj`, blood FX, and VM `CanCollideWith` are injected.

## Credit rationale

Ports isolated infight/projectile damage policy ahead of `PIT_CheckThing` hosting.
