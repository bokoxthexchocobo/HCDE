# Chunk 749 — Missile/bounce hit virtuals, reflection, inventory walks, sounds, attack gate (`p_mobj.cpp` 3830–4019).

## Scope

Missile/bounce hit virtuals, reflection, inventory walks, sounds, attack gate (`p_mobj.cpp` 3830–4019).

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::SpecialMissileHit** 3830–3842 (13 lines)
- `src/playsim/p_mobj.cpp` **AActor::SpecialBounceHit** 3845–3857 (13 lines)
- `src/playsim/p_mobj.cpp` **AActor::AdjustReflectionAngle** 3859–3902 (44 lines)
- `src/playsim/p_mobj.cpp` **AActor::AbsorbDamage** 3904–3918 (15 lines)
- `src/playsim/p_mobj.cpp` **AActor::AlterWeaponSprite** 3920–3937 (18 lines)
- `src/playsim/p_mobj.cpp` **AActor::PlayActiveSound** 3939–3946 (8 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, PlayActiveSound)** 3948–3953 (6 lines)
- `src/playsim/p_mobj.cpp` **AActor::PlayPushSound** 3955–3962 (8 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, PlayPushSound)** 3964–3969 (6 lines)
- `src/playsim/p_mobj.cpp` **AActor::IsOkayToAttack** 3971–4019 (49 lines)

**Credited native LOC:** 180 (tally `nativeLoc` 180).

## Managed

`VmPMobjHitReflectAttack.cs`
Managed LOC (tally): 231.
Conversion commit: `731fc75dcd901680448a6cd0bc4ce4720d0f228e`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `SpecialMissileHit` | `AActor::SpecialMissileHit` 3830–3842 |
| `SpecialBounceHit` | `AActor::SpecialBounceHit` 3845–3857 |
| `AdjustReflectionAngle` | `AActor::AdjustReflectionAngle` 3859–3902 |
| `AbsorbDamage` | `AActor::AbsorbDamage` 3904–3918 |
| `AlterWeaponSprite` / `CollectInventoryForward` | `AActor::AlterWeaponSprite` 3920–3937 |
| `PlayActiveSound` | `AActor::PlayActiveSound` 3939–3953 |
| `PlayActiveSound` | `DEFINE_ACTION_FUNCTION(AActor, PlayActiveSound)` 3948–3953 |
| `PlayPushSound` | `AActor::PlayPushSound` 3955–3962 |
| `PlayPushSound` | `DEFINE_ACTION_FUNCTION(AActor, PlayPushSound)` 3964–3969 |
| `IsOkayToAttack` / `ResolveAttackFriend` | `AActor::IsOkayToAttack` 3971–4019 |

### Out-of-body uncredit (749)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **180** |
| **Out-of-body removed** | **0** |
| **Honest** | **180** |


**Post-review ratio:** 231/180 managed/native ≈ 128% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjHitReflectAttackTests.SpecialMissileHit_DefaultWhenNoVirtual`
- `VmPMobjHitReflectAttackTests.AdjustReflectionAngle_DontReflectExplodes`
- `VmPMobjHitReflectAttackTests.AdjustReflectionAngle_ShieldAdds45`
- `VmPMobjHitReflectAttackTests.AbsorbDamage_WalksInventoryChain`
- `VmPMobjHitReflectAttackTests.PlayActiveSound_SkipsBusyVoice`
- `VmPMobjHitReflectAttackTests.IsOkayToAttack_RejectsSelf`
- `VmPMobjHitReflectAttackTests.IsOkayToAttack_ScreenSeekerFov`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 749 --to 749` (0 hard / 0 soft).

## Seams

`VMCall`, `P_CheckSight`, `A_Face`, and `S_Sound` are injected.

## Credit rationale

Whole p_mobj band after slam (chunk748); ends before SetShade.
