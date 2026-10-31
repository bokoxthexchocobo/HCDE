# Chunk 1146 — Missile range, friend blocking, lift, and crush direction from `p_enemy.cpp` (**301–450**); follows **1145**.

## Scope

Missile range, friend blocking, lift, and crush direction from `p_enemy.cpp` (**301–450**); follows **1145**.

## Native ranges

- `src/playsim/p_enemy.cpp` **Enemy missile range and lift** 301–450 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPEnemyMissile1146Slice.cs`
Managed LOC (tally): 111.
Conversion commit: `7008447324df9fa3e560102339a2822fea14ffe8`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `P_CheckMissileRange` (**307–365**) | yes |
| `P_HitFriend` (**384–399**) | yes |
| `P_IsOnLift` (**412–426**) | yes |
| Crushing-ceiling direction (**438–450**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **151–300** | sound lines and melee range already credited by **1145** |
| **367–371** | VM thunk forwarded to the host |
| **451–481** | sector-damage return and `P_Move` head; 31 lines, under the floor |

`SECF_NOATTACK` is **2048**. Sight uses `SF_SEEPASTBLOCKEVERYTHING` (**4**). `MF_JUSTHIT` is **0x40** and is cleared before the friend checks. An unfriendly monster always fights back. A friendly monster skips a dead target, then rolls `pr_defect() > 128` for a player friend and also requires the target not to be just-hit otherwise. Reaction time, friendship, and `P_HitFriend` apply only when the monster was not just hit. Distance starts at `Distance2D - 64`, then subtracts **128** when there is no melee state. `maxtargetrange` rejects a larger distance, and `meleethreshold` rejects a closer one when a melee state exists. The roll is `pr_checkmissilerange() >= min((int)dist, (int)(MinMissileChance * aggressiveness))`, truncating toward zero. `SKILLP_Aggressiveness` is enum index **5**; the multiplier itself is host-supplied. A different aimed friend blocks only a friendly monster that still has a target. An active `DPlat` is a lift, otherwise `SECMF_LIFT` (**2048**). Crush direction is **1** up, **0** waiting, **-1** down, and only a ceiling with `getCrush() > 0` is OR-ed in.

| Managed (`VmPEnemyMissile1146Slice.cs`) | Native (`p_enemy.cpp`) |
|-------------------------------------------|----------------------|
| Missile range, friend, lift, crush | **301–450** |
| Band hull (exclusive slice) | **301–450** |

### Chunk **1146** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1145** |


**Post-review ratio:** 111/150 managed/native ≈ 74% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyMissile1146SliceTests.CheckMissileRange_FightsBackOnceThenUsesChance`
- `VmPEnemyMissile1146SliceTests.HitFriend_StopsOnlyOnADifferentFriend`
- `VmPEnemyMissile1146SliceTests.LiftAndCrush_UsePlatAndCeilingDirection`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1146 --to 1146` (0 hard / 0 soft).

## Seams

Sight, the defect and missile-range rolls, aim-line friends, and floor thinkers stay host-supplied. Sector damage under the actor begins at **451**.

## Credit rationale

UNCredited **`p_enemy.cpp`** **301–450** (+150), inside the 3488-line file, with no conversion or recognition overlap.
