# Chunk 388 — Map format flags, node signature selection, seg sidedef fixups, spawn ordering Retired (2026-10-23): out-of-body credit removed; honest **98** < **150**.

## Scope

Map format flags, node signature selection, seg sidedef fixups, spawn ordering Retired (2026-10-23): out-of-body credit removed; honest **98** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `d7519bd63540f693cf2c6bb8363abc7be5d748ee`.

## Padding review (2026-10-21)



### Out-of-body uncredit (388)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **98** |
| **Out-of-body removed** | **0** |
| **Honest** | **98** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 388 --to 388` (0 hard / 0 soft).

## Seams

`LoadExtendedNodes`, `FNodeBuilder`, and `SpawnSlopeMakers` are not hosted.

## Credit rationale

Credits map format activation, node-load orchestration, and post-node seg fixups from `maploader.cpp`.
