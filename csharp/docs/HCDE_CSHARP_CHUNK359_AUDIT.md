# Chunk 359 — fraglimit deathmatch exit and i_timescale minimum guard

## Scope

fraglimit deathmatch exit and i_timescale minimum guard

## Native ranges

- `src/d_main.cpp` **i_timescale minimum** 429–438 (10 lines)
- `src/d_main.cpp` **CUSTOM_CVAR fraglimit** 466–482 (17 lines)

**Credited native LOC:** 27 (tally `nativeLoc` 27).

## Managed

`VmDMainCvarPolicyTests.cs`, `VmDMainFragLimitPolicy.cs`, `VmDMainFragLimitPolicyTests.cs`
Managed LOC (tally): 11.
Conversion commit: `268c8ede9028637b6c52c25f2aa90beb7bbb1f75`.

## Tests

- `VmDMainCvarPolicyTests.IsTimeScaleAllowed_RejectsTooSmall`
- `VmDMainFragLimitPolicyTests.ShouldExitLevel_WhenAtOrAboveLimit`
- `VmDMainFragLimitPolicyTests.ShouldCheckFragLimit_RequiresDeathmatch`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 359 --to 359` (0 hard / 0 soft).

## Seams

`primaryLevel->ExitLevel` and `I_FreezeTime` are not re-hosted; only predicate guards are modeled.

## Credit rationale

Credits `CUSTOM_CVAR(fraglimit)` level-exit scan and the sub-0.05 `i_timescale` rejection branch.
