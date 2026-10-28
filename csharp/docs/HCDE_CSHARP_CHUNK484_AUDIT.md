# Chunk 484 — P_RailAttack puff/damage loop and CanTalk probes

## Scope

P_RailAttack puff/damage loop and CanTalk probes

## Native ranges

- `src/playsim/p_map.cpp` **P_RailAttack** 5568–5638 (71 lines)
- `src/playsim/p_map.cpp` **P_RailAttack** 5640–5691 (52 lines)
- `src/playsim/p_map.cpp` **CanTalk** 5755–5764 (10 lines)
- `src/playsim/p_map.cpp` **HasConversation** 5766–5775 (10 lines)
- `src/playsim/p_map.cpp` **NativeStartConversation** 5777–5810 (34 lines)

**Credited native LOC:** 177 (tally `nativeLoc` 177).

## Managed

`VmPMapRailDamageTalk.cs`, `VmPMapRailDamageTalkTests.cs`
Managed LOC (tally): 92.
Conversion commit: `72c9ce6ea9955ab1475e5accb8b8552b05e7ad89`.

## Tests

- `VmPMapRailDamageTalkTests.RailHit_BleedsWhenAllowed`
- `VmPMapRailDamageTalkTests.RailHit_SkipsBleedWhenInvuln`
- `VmPMapRailDamageTalkTests.BuildDamageFlags_IncludesRailgun`
- `VmPMapRailDamageTalkTests.DeepSplash_EitherWaterCrossing`
- `VmPMapRailDamageTalkTests.CanTalk_BlocksInCombat`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 484 --to 484` (0 hard / 0 soft).

## Seams

`P_SpawnPuff`, `P_DamageMobj`, `P_DrawRailTrail`, and `Trace` are not hosted.

## Credit rationale

Credits rail hit puff/bleed/damage flags, wall/floor decal and splash tail, and conversation eligibility from `p_map.cpp` (5568–5810).
