# Chunk 511 — P_CheckMusicChange, P_CheckEnvironment, P_CheckUse, FSafePosition::Update, P_PlayerThink

## Scope

P_CheckMusicChange, P_CheckEnvironment, P_CheckUse, FSafePosition::Update, P_PlayerThink

## Native ranges

- `src/playsim/p_user.cpp` **P_CheckMusicChange** 1682–1722 (41 lines)
- `src/playsim/p_user.cpp` **P_CheckEnvironment** 1723–1749 (27 lines)
- `src/playsim/p_user.cpp` **P_CheckUse** 1750–1784 (35 lines)
- `src/playsim/p_user.cpp` **FSafePosition::Update** 1785–1798 (14 lines)
- `src/playsim/p_user.cpp` **P_PlayerThink** 1830–1912 (83 lines)

**Credited native LOC:** 200 (tally `nativeLoc` 200).

## Managed

`VmPUserPlayerThink.cs`, `VmPUserPlayerThinkTests.cs`
Managed LOC (tally): 124.
Conversion commit: `9f64635dab73525389e1fa73ca536786d1b6c226`.

## Tests

- `VmPUserPlayerThinkTests.PlayerThink_SubtitleDecrements`
- `VmPUserPlayerThinkTests.PlayerThink_BotFrozenReturnsEarly`
- `VmPUserPlayerThinkTests.PlayerThink_RecordsVelocityOffline`
- `VmPUserPlayerThinkTests.PlayerThink_AppliesAngleOffset`
- `VmPUserPlayerThinkTests.PlayerThink_SkipsBobWhenPredicting`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 511 --to 511` (0 hard / 0 soft).

## Seams

`P_PlayerMove`, `P_UseLines`, `VMCall`, `P_BobWeapon`, and `NetworkEntityManager` are not hosted.

## Credit rationale

Credits music/environment/use hooks, safe-position tracking, angle-offset/subtitle/bob/presentation gates, and velocity bookkeeping from `p_user.cpp` (1682–1912).
