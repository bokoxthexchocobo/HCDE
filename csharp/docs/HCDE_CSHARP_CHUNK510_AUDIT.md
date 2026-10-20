# Chunk 510 — P_UnPredictClient, A_BossDeath exit tail, FLevelLocals::Massacre

## Scope

P_UnPredictClient, A_BossDeath exit tail, FLevelLocals::Massacre

## Native ranges

- `src/playsim/p_user.cpp` **P_UnPredictClient** 2396–2474 (79 lines)
- `src/playsim/p_enemy.cpp` **A_BossDeath** 3396–3444 (49 lines)
- `src/playsim/p_enemy.cpp` **FLevelLocals::Massacre** 3454–3474 (21 lines)

**Credited native LOC:** 149 (tally `nativeLoc` 149).

## Managed

`VmPUserUnpredictBoss.cs`, `VmPUserUnpredictBossTests.cs`
Managed LOC (tally): 84.
Conversion commit: `47552c4022a2cb563b61f68bcc6a232737eb670e`.

## Tests

- `VmPUserUnpredictBossTests.Unpredict_ClearsCheatWhenAlreadyDisabled`
- `VmPUserUnpredictBossTests.Unpredict_ConsolePlayerInRange`
- `VmPUserUnpredictBossTests.Unpredict_RestoresWeaponWhenPredicting`
- `VmPUserUnpredictBossTests.BossExit_BlocksNoExitDmflags`
- `VmPUserUnpredictBossTests.BossExit_PicksLowerFloorSpec`
- `VmPUserUnpredictBossTests.Massacre_SkipsFriendlyWhenBaddiesOnly`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 510 --to 510` (0 hard / 0 soft).

## Seams

`NetworkEntityManager`, serializer rollback, `P_ExecuteSpecial`, `EV_DoFloor`, thinker iterators, and `ExitLevel` are not hosted.

## Credit rationale

Credits unpredict teardown gates, boss victory speckill/map07/spec-action/no-exit policy, and massacre eligibility from `p_user.cpp` (2396–2474) and `p_enemy.cpp` (3396–3444, 3454–3474).
