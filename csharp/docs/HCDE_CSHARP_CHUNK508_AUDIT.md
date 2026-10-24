# Chunk 508 — NoiseMarkSector, P_RecursiveSound, P_NoiseAlert, P_HitFriend, P_IsOnLift, P_IsUnderDamage, P_CheckTags

## Scope

NoiseMarkSector, P_RecursiveSound, P_NoiseAlert, P_HitFriend, P_IsOnLift, P_IsUnderDamage, P_CheckTags

## Native ranges

- `src/playsim/p_enemy.cpp` **NoiseMarkSector** 122–145 (24 lines)
- `src/playsim/p_enemy.cpp` **P_RecursiveSound** 147–219 (73 lines)
- `src/playsim/p_enemy.cpp` **P_NoiseAlert** 230–245 (16 lines)
- `src/playsim/p_enemy.cpp` **P_HitFriend** 384–399 (16 lines)
- `src/playsim/p_enemy.cpp` **P_IsOnLift** 412–426 (15 lines)
- `src/playsim/p_enemy.cpp` **P_IsUnderDamage** 438–460 (23 lines)
- `src/playsim/p_enemy.cpp` **P_CheckTags** 467–474 (8 lines)

**Credited native LOC:** 175 (tally `nativeLoc` 175).

## Managed

`VmPEnemyNoiseMove.cs`, `VmPEnemyNoiseMoveTests.cs`
Managed LOC (tally): 103.
Conversion commit: `9efd183ae6ccf9c1d4760f2ce65bda9508233ed4`.

## Tests

- `VmPEnemyNoiseMoveTests.NoiseFlood_SkipsWhenAlreadyFlooded`
- `VmPEnemyNoiseMoveTests.NoiseSplash_RespectsNosplashalert`
- `VmPEnemyNoiseMoveTests.NoiseAlert_SkipsNullEmitter`
- `VmPEnemyNoiseMoveTests.NoiseAlert_SkipsNoTargetCheat`
- `VmPEnemyNoiseMoveTests.HitFriend_BlocksFriendlyLineTarget`
- `VmPEnemyNoiseMoveTests.OnLift_DetectsSectorFlag`
- `VmPEnemyNoiseMoveTests.UnderDamage_PlayerHarmReturnsSerious`
- `VmPEnemyNoiseMoveTests.CheckTags_PrimaryMatch`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 508 --to 508` (0 hard / 0 soft).

## Seams

`validcount`, `P_AimLineAttack`, sector portal displacement, thinker iterators, and `DSectorEffect` types are not hosted.

## Credit rationale

Credits sound-alert flooding, recursive sector traversal gates, friendly fire line tests, lift/damage AI probes, and sector tag matching from `p_enemy.cpp` (122–245, 384–474).
