# Chunk 423 — LS_SetPlayerProperty power grants and cheat flag masks

## Scope

LS_SetPlayerProperty power grants and cheat flag masks

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_SetPlayerProperty** 2911–3128 (218 lines)

**Credited native LOC:** 218 (tally `nativeLoc` 218).

## Managed

`VmPLnspecSetPlayerProperty.cs`
Managed LOC (tally): 116.
Conversion commit: `a145dd15b637aaffe40d80bb8f0c62817d0d4df2`.

## Tests

- `VmPLnspecSetPlayerPropertyTests.PowerBranch_RejectsNoneEntry`
- `VmPLnspecSetPlayerPropertyTests.FrozenCompat_ClearsBothFlags`
- `VmPLnspecSetPlayerPropertyTests.Noclip2_RequiresBothBits`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 423 --to 423` (0 hard / 0 soft).

## Seams

`GiveInventoryType`, power actors, and live `player_t` mutation are not hosted.

## Credit rationale

Credits power-table indexing, ALLMAP console-player gate, `CF_*` mask mapping, and frozen-prop compat clearing from `p_lnspec.cpp`.
