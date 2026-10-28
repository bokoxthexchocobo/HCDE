# Chunk 410 — DGlow, DGlow2, DPhased thinkers Retired (2026-10-23): out-of-body credit removed; honest **125** < **150**.

## Scope

DGlow, DGlow2, DPhased thinkers Retired (2026-10-23): out-of-body credit removed; honest **125** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `28eccd241f834aea2c3433c8e55b749d0763626c`.

## Padding review (2026-10-21)



### Out-of-body uncredit (410)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **154** |
| **Out-of-body removed** | **29** |
| **Honest** | **125** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 410 --to 410` (0 hard / 0 soft).

## Seams

Sector mutation, thinker allocation, and `PhaseHelper` graph walks are not hosted.

## Credit rationale

Credits `DGlow`, `DGlow2`, and `DPhased` tick/construct logic from `a_lights.cpp` (bands avoid recognition `EV_StartLightFlickering`).
