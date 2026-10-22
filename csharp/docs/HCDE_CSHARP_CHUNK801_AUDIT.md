# Chunk 801 — Whole noise alert (`NoiseMarkSector`–`P_HitFriend` 122–399) plus lift/hazard/tag, `A_FaceTarget`, `P_DropItem`, and `FLevelLocals::Massacre` (`p_enemy.cpp` 412–474, 3142–3215, 3454–3474). Absorbs retired chunk508 noise bands.

## Scope

Whole noise alert (`NoiseMarkSector`–`P_HitFriend` 122–399) plus lift/hazard/tag, `A_FaceTarget`, `P_DropItem`, and `FLevelLocals::Massacre` (`p_enemy.cpp` 412–474, 3142–3215, 3454–3474). Absorbs retired chunk508 noise bands.

## Native ranges

- `src/playsim/p_enemy.cpp` **NoiseMarkSector** 122–145 (24 lines)
- `src/playsim/p_enemy.cpp` **P_RecursiveSound** 147–219 (73 lines)
- `src/playsim/p_enemy.cpp` **P_NoiseAlert** 230–245 (16 lines)
- `src/playsim/p_enemy.cpp` **P_HitFriend** 384–399 (16 lines)
- `src/playsim/p_enemy.cpp` **P_IsOnLift** 412–426 (15 lines)
- `src/playsim/p_enemy.cpp` **P_IsUnderDamage** 438–460 (23 lines)
- `src/playsim/p_enemy.cpp` **P_CheckTags** 467–474 (8 lines)
- `src/playsim/p_enemy.cpp` **FLevelLocals::Massacre** 3454–3474 (21 lines)

**Credited native LOC:** 196 (tally `nativeLoc` 196).

## Managed

`VmPEnemyNoiseMove.cs`, `VmPEnemySmartAssist.cs`
Managed LOC (tally): 206.
Conversion commit: `398f9e188ec4e2b9c33686bd6ee3acfb562fa9ca`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|----------------------|
| Noise flood helpers | `NoiseMarkSector` **122–145** |
| Recursive sound gates | `P_RecursiveSound` **147–219** |
| `NoiseAlertNoTarget` / splash | `P_NoiseAlert` **230–245** |
| `HitFriendRequiresFriendly` | `P_HitFriend` **384–399** |
| `IsOnLift` | `P_IsOnLift` **412–426** |
| `IsUnderDamage` | `P_IsUnderDamage` **438–460** |
| `CheckTags` | `P_CheckTags` **467–474** |
| `FaceTarget` / `DropItem` | chunk507 (`VmPEnemyFacePain`); wrappers in `VmPEnemySmartAssist` |
| `Massacre` | `FLevelLocals::Massacre` **3454–3474** |


**Post-review ratio:** 206/196 managed/native ≈ 105% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyNoiseMoveTests.NoiseFlood_SkipsWhenAlreadyFlooded`
- `VmPEnemyNoiseMoveTests.RecursiveSound_TraversesSoundPortal`
- `VmPEnemyNoiseMoveTests.RecursiveSound_ClosedDoorBlocksTraversal`
- `VmPEnemyNoiseMoveTests.NoiseAlert_SkipsNullEmitter`
- `VmPEnemyNoiseMoveTests.HitFriend_BlocksFriendlyLineTarget`
- `VmPEnemySmartAssistTests.IsOnLift_DetectsPlatThinker`
- `VmPEnemySmartAssistTests.IsOnLift_DetectsSectorFlag`
- `VmPEnemySmartAssistTests.IsUnderDamage_PlayerHarmReturnsSerious`
- `VmPEnemySmartAssistTests.CheckTags_MatchesPrimaryTag`
- `VmPEnemySmartAssistTests.Massacre_CountsMonsterKills`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 801 --to 801` (0 hard / 0 soft).

## Seams

Sector graphs, plat/ceiling thinkers, `A_Face`, VM `A_DropItem`, and per-actor `Massacre` are injected.

## Credit rationale

Completes p_enemy smart-move and noise support in one TU band after chunk800 look actions.
