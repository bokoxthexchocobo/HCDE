# Chunk 425 — PickDeathmatchStart action and d_main screen/UI helpers Retired (2026-10-24): `--band` hull overlaps existing credit ((multiple native files; --band hull undefined)).

## Scope

PickDeathmatchStart action and d_main screen/UI helpers Retired (2026-10-24): `--band` hull overlaps existing credit ((multiple native files; --band hull undefined)).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `28c114f21d846a090493abad24fbbe1eb0726f08`.

## Padding review (2026-10-21)



### Out-of-body uncredit (425)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **160** |
| **Out-of-body removed** | **3** |
| **Honest** | **157** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 425 --to 425` (0 hard / 0 soft).

## Seams

`pr_dmspawn`, `StatusBar`, and crash buffer formatting are not hosted.

## Credit rationale

Credits deathmatch start selection, screen resize hooks, scene rect sizing, and console toggle HUD policy from `g_game.cpp` and `d_main.cpp`.
