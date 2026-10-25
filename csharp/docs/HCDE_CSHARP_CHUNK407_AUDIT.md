# Chunk 407 — Startup progress UI, CheckDefaultSkill, RenameSprites policy Retired (2026-10-24): thin managed at HEAD — surviving **0** < **95** (50% of honest **190**).

## Scope

Startup progress UI, CheckDefaultSkill, RenameSprites policy Retired (2026-10-24): thin managed at HEAD — surviving **0** < **95** (50% of honest **190**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `a00bf47792800947f2cdcd53dcbd2786addff464`.

## Padding review (2026-10-21)



### Out-of-body uncredit (407)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **194** |
| **Out-of-body removed** | **4** |
| **Honest** | **190** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 407 --to 407` (0 hard / 0 soft).

## Seams

`I_ShowStartupStatus`, `gameskill` cvar, and filesystem rename IO are not hosted.

## Credit rationale

Credits startup progress helpers, `CheckDefaultSkill`, and the `RenameSprites` gametype rename tables from `d_main.cpp`.
