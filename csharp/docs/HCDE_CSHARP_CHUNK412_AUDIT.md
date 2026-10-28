# Chunk 412 — QueueBody, DoPlayerPop, ScreenShot, LoadGame, save WAD check Retired (2026-10-24): `--band` hull overlaps existing credit (g-game-headless-ticker-chunk9 src/g_game.cpp 2360-2448).

## Scope

QueueBody, DoPlayerPop, ScreenShot, LoadGame, save WAD check Retired (2026-10-24): `--band` hull overlaps existing credit (g-game-headless-ticker-chunk9 src/g_game.cpp 2360-2448).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `7e5d9dcc4e3268531b635e48ad90605274964921`.

## Padding review (2026-10-21)



### Out-of-body uncredit (412)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **162** |
| **Out-of-body removed** | **1** |
| **Honest** | **161** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 412 --to 412` (0 hard / 0 soft).

## Seams

Filesystem, serializer, and screenshot/menu IO are not hosted.

## Credit rationale

Credits corpse queue, disconnect pop, screenshot gate, and save-game WAD validation from `g_game.cpp` (bands avoid chunk 9 deathmatch spawn).
