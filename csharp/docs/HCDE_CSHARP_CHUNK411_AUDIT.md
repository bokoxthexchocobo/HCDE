# Chunk 411 — G_FullConsole, cutscene tick/advance, CheckSpot and DM start pick Retired (2026-10-24): `--band` hull overlaps existing credit (g-game-headless-ticker-chunk9 src/g_game.cpp 1479-1505).

## Scope

G_FullConsole, cutscene tick/advance, CheckSpot and DM start pick Retired (2026-10-24): `--band` hull overlaps existing credit (g-game-headless-ticker-chunk9 src/g_game.cpp 1479-1505).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `db56816c8747e32e4c84a955815f1a8cb9f5330f`.

## Padding review (2026-10-21)



### Out-of-body uncredit (411)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **190** |
| **Out-of-body removed** | **0** |
| **Honest** | **190** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 411 --to 411` (0 hard / 0 soft).

## Seams

`ScreenJobRunner` VM, `P_CheckPosition`, and live player structs are not hosted.

## Credit rationale

Credits `G_FullConsole`, cutscene progression helpers, `G_SpawnPlayer` title tail, and deathmatch start selection from `g_game.cpp`.
