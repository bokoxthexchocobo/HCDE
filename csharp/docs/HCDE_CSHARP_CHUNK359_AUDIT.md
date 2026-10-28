# Chunk 359 — fraglimit deathmatch exit and i_timescale minimum guard Retired (2026-10-23): out-of-body credit removed; honest **27** < **150**.

## Scope

fraglimit deathmatch exit and i_timescale minimum guard Retired (2026-10-23): out-of-body credit removed; honest **27** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `fd878cce151a8577ce266b43501ae15133ba7df2`.

## Padding review (2026-10-21)



### Out-of-body uncredit (359)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **27** |
| **Out-of-body removed** | **0** |
| **Honest** | **27** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 359 --to 359` (0 hard / 0 soft).

## Seams

`primaryLevel->ExitLevel` and `I_FreezeTime` are not re-hosted; only predicate guards are modeled.

## Credit rationale

Credits `CUSTOM_CVAR(fraglimit)` level-exit scan and the sub-0.05 `i_timescale` rejection branch.
