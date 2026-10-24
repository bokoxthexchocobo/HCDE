# Chunk 377 — Automap rotation cache, line-character transforms, and multiplayer player icon policy Retired (2026-10-24): thin managed at HEAD — surviving **0** < **85** (50% of honest **169**).

## Scope

Automap rotation cache, line-character transforms, and multiplayer player icon policy Retired (2026-10-24): thin managed at HEAD — surviving **0** < **85** (50% of honest **169**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `6376e8a2a90853aae3fe574c8c8f5bf373e3d0da`.

## Padding review (2026-10-21)



### Out-of-body uncredit (377)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **169** |
| **Out-of-body removed** | **0** |
| **Honest** | **169** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 377 --to 377` (0 hard / 0 soft).

## Seams

`drawMline`, `D_GetPlayerColor`, portal displacement, and cheat arrow lumps are not hosted.

## Credit rationale

Credits `DAutomap::rotate`, `rotatePoint`, `drawLineCharacter`, and `drawPlayers` from `am_map.cpp`.
