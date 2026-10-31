# Chunk 1145 — Recursive sound lines, noise alert, and melee range from `p_enemy.cpp` (**151–300**); follows **1144**.

## Scope

Recursive sound lines, noise alert, and melee range from `p_enemy.cpp` (**151–300**); follows **1144**.

## Native ranges

- `src/playsim/p_enemy.cpp` **Enemy sound lines and melee range** 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPEnemyMelee1145Slice.cs`
Managed LOC (tally): 108.
Conversion commit: `a2b593dcfb5056f0deef17d1e90b1864274675d4`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Portal plane and line-portal marks (**152–175**) | yes |
| Two-sided, closed-door, and sound-block cuts (**177–217**) | yes |
| `P_NoiseAlert` (**230–245**) | yes |
| `P_CheckMeleeRange` (**253–299**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–150** | tables and sector mark already credited by **1144** |
| **307+** | `P_CheckMissileRange` |

`ML_TWOSIDED` is **4** and `ML_SOUNDBLOCK` is **0x40**. A sound-block line is entered only while `soundblocks` is **0**, and the neighbor is then marked with **1**. Equal floor and ceiling planes block sound (`>=`). `CF_NOTARGET` is **8**. `SECF_NOATTACK` is **2048**. `MF5_NOVERTICALMELEERANGE` is **0x04000000**. A reached goal returns before the no-attack, vertical, friend, and sight checks. A negative melee range uses the actor's `meleerange`.

| Managed (`VmPEnemyMelee1145Slice.cs`) | Native (`p_enemy.cpp`) |
|------------------------------------------|----------------------|
| Sound lines, alert, melee | **151–300** |
| Band hull (exclusive slice) | **151–300** |

### Chunk **1145** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1144** |


**Post-review ratio:** 108/150 managed/native ≈ 72% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyMelee1145SliceTests.SoundLines_StopAtClosedDoorsAndASecondSoundBlock`
- `VmPEnemyMelee1145SliceTests.BeginNoiseAlert_IgnoresAMissingEmitterAndANoTargetPlayer`
- `VmPEnemyMelee1145SliceTests.CheckMeleeRange_ReachesAGoalBeforeSectorAndFriendChecks`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1145 --to 1145` (0 hard / 0 soft).

## Seams

Portal sectors and sight checks stay host-supplied. Missile range begins at **307**.

## Credit rationale

UNCredited **`p_enemy.cpp`** **151–300** (+150), inside the 3488-line file, with no conversion or recognition overlap.
