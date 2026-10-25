# Chunk 437 — CheckIfExitIsGood and P_ActivateLine through RemoteActivate Retired (2026-10-23): out-of-body credit removed; honest **149** < **150**.

## Scope

CheckIfExitIsGood and P_ActivateLine through RemoteActivate Retired (2026-10-23): out-of-body credit removed; honest **149** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `c643166fd344a680f73f98d84ab61161d9d8f617`.

## Padding review (2026-10-21)



### Out-of-body uncredit (437)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **156** |
| **Out-of-body removed** | **7** |
| **Honest** | **149** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 437 --to 437` (0 hard / 0 soft).

## Seams

`P_ExecuteSpecial`, `P_CheckKeys`, and VM rumble hooks are not hosted.

## Credit rationale

Credits exit-good DM/monster gates, remote line lock policy, non-repeat special clearing, dummy-switch compat, and remote activate optional position from `p_spec.cpp`.
