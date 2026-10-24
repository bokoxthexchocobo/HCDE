# Chunk 399 — ParseGameInfo, GetReserved, CheckGameInfo, SetMapxxFlag Retired (2026-10-24): thin managed at HEAD — surviving **0** < **81** (50% of honest **161**).

## Scope

ParseGameInfo, GetReserved, CheckGameInfo, SetMapxxFlag Retired (2026-10-24): thin managed at HEAD — surviving **0** < **81** (50% of honest **161**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `f6c78f296a89d92b19cfa97f0cea0e29e885b7ef`.

## Padding review (2026-10-21)



### Out-of-body uncredit (399)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **161** |
| **Out-of-body removed** | **0** |
| **Honest** | **161** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 399 --to 399` (0 hard / 0 soft).

## Seams

`FScanner`, `FileSystem::InitMultipleFiles`, and `gameinfo` mutation are not hosted.

## Credit rationale

Credits static `ParseGameInfo`, `GetReserved`, `CheckGameInfo`, and `SetMapxxFlag` from `d_main.cpp`.
