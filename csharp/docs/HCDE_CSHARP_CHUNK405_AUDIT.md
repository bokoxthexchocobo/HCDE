# Chunk 405 — G_BuildTiccmd tail, LookAdjust, view pitch/yaw, ChangeSpy Retired (2026-10-24): thin managed at HEAD — surviving **0** < **86** (50% of honest **172**).

## Scope

G_BuildTiccmd tail, LookAdjust, view pitch/yaw, ChangeSpy Retired (2026-10-24): thin managed at HEAD — surviving **0** < **86** (50% of honest **172**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `48fa0b3776d2121529abfc63b60b5bd0896dd55c`.

## Padding review (2026-10-21)



### Out-of-body uncredit (405)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **174** |
| **Out-of-body removed** | **2** |
| **Honest** | **172** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 405 --to 405` (0 hard / 0 soft).

## Seams

`players`, `StatusBar`, and demo net messages are not hosted.

## Credit rationale

Credits the `G_BuildTiccmd` net tail, `LookAdjust`, `G_AddViewPitch`, `G_AddViewAngle`, and `ChangeSpy` from `g_game.cpp`.
