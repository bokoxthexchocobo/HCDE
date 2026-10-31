# Chunk 1151 — Chase dispatch, facing, monster rail, drop, and pain from `p_enemy.cpp` (**2999–3270**). Last in-file hole of at least 150 lines.

## Scope

Chase dispatch, facing, monster rail, drop, and pain from `p_enemy.cpp` (**2999–3270**). Last in-file hole of at least 150 lines.

## Native ranges

- `src/playsim/p_enemy.cpp` **Enemy chase face rail and pain** 2999–3270 (272 lines)

**Credited native LOC:** 272 (tally `nativeLoc` 272).

## Managed

`VmPEnemyPain1151Slice.cs`
Managed LOC (tally): 248.
Conversion commit: `093f58231546f67f0f35179182fa60e6b8dd6b08`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `A_DoChaseNative` (**3012–3016**) | yes |
| `A_Face` / `A_FaceTarget` (**3046–3145**) | yes |
| `A_MonsterRail` (**3155–3196**) | yes |
| `P_DropItem` (**3204–3215**) | yes |
| `A_Pain` (**3217–3269**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2245–2396** | wander already credited by **1150** |
| **2397–2998** | chase body already credited |
| **3000–3009** and **3019–3028** | VM thunks forwarded to the host |
| **3271+** | `CheckBossDeath` already credited |
| **451–481** | 31-line gap under the floor |

`CHF_FASTCHASE` is **1**, `CHF_NOPLAYACTIVE` is **2**, `CHF_NIGHTMAREFAST` is **4**, `CHF_RESURRECT` is **8**, and `CHF_DONTMOVE` is **16**. A resurrection hit returns before the chase. Otherwise play-active is the inverse of the no-play bit, and the flags passed on are masked with **0x3fffffff**. `A_Face` returns immediately with no other actor. Stealth sets `visdir` to **1** and `MF_AMBUSH` is cleared. The turn delta is the negated BAM `deltaangle`. A non-zero max turn smaller than that delta steps by `max_turn + ang_offset` in the delta's direction; otherwise yaw becomes the target angle plus the offset. Pitch runs only when max pitch is at most **180** (the face-target default is **270** and skips it). The default body points are **32** above each foot, then the feet, center, or top flags replace the target in that order, and `z_add` is applied last. A point at or above the actor's top uses the center. Pitch is `-asin(dz / distance)`. A non-zero max pitch steps by the smaller of that limit and the **360**-normalized difference. A shadow yaw offset is `Random2() * 45/256 * aim * penalty`, and only when the turn is unlimited and yaw already equals the target angle. Vertical shadow aim (`MF9_SHADOWAIMVERT`, **8**) does the same for pitch only when max pitch is zero. `A_MonsterRail` needs a target. It clears ambush, uses the lead angle, keeps an aimed pitch when a line target exists, and otherwise uses `-atan2(zdiff, xy)` where `zdiff` is the target center minus the shooter center minus floor clip. `MISSILERANGE` is **2048** and the aim window is **60** degrees. The saved pitch is restored after the shot. `P_DropItem` returns an actor only when the VM does. Player pain, with no alternative skin, uses `*pain25`, `*pain50`, `*pain75`, or `*pain100` at health **25**, **50**, and **75**. A damage type tries the amount plus the type, then `*pain-` plus the type, then the amount. That path rumbles and plays on `CHAN_VOICE` (**2**) with `CHANF_NORUMBLE` (**0x80000**) at attenuation **1**. Any other actor plays `PainSound` only when it is valid, without the no-rumble flag.

| Managed (`VmPEnemyPain1151Slice.cs`) | Native (`p_enemy.cpp`) |
|----------------------------------------|----------------------|
| Chase, face, rail, pain | **2999–3270** |
| Band hull (exclusive slice) | **2999–3270** |

### Chunk **1151** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **272** |
| **Net headline** | **+272** vs post-**1150** |


**Post-review ratio:** 248/272 managed/native ≈ 91% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyPain1151SliceTests.DoChase_StopsForAResurrectionAndMasksTheHighFlag`
- `VmPEnemyPain1151SliceTests.Turn_LimitsYawAndAimsPitchFromTheChosenBodyPoint`
- `VmPEnemyPain1151SliceTests.MonsterRail_LeadsTheShotAndRestoresPitch`
- `VmPEnemyPain1151SliceTests.Pain_PrefersADamageTypeAndUsesHealthBands`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1151 --to 1151` (0 hard / 0 soft).

## Seams

Resurrection, aim, shadow checks, missile damage, sound lookup, and the drop VM stay host-supplied. Facing uses the native BAM normalizer.

## Credit rationale

UNCredited **`p_enemy.cpp`** **2999–3270** (+272), inside the 3488-line file, with no conversion or recognition overlap.
