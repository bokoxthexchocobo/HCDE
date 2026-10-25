# Chunk 415 — EV strobe/light-on/glow, Strife teaser voices, DoomButtons table Retired (2026-10-23): out-of-body credit removed; honest **122** < **150**.

## Scope

EV strobe/light-on/glow, Strife teaser voices, DoomButtons table Retired (2026-10-23): out-of-body credit removed; honest **122** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `01e86310d7e571a226decd986e81dfcf37a673a5`.

## Padding review (2026-10-21)



### Out-of-body uncredit (415)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **157** |
| **Out-of-body removed** | **35** |
| **Honest** | **122** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 415 --to 415` (0 hard / 0 soft).

## Seams

Sector iterators, filesystem namespaces, and bind registration are not hosted.

## Credit rationale

Credits `EV_StartLightStrobing`, `EV_LightTurnOn`, `EV_StartLightGlowing`, `FindStrifeTeaserVoices`, and the `DoomButtons` table from `a_lights.cpp` and `d_main.cpp`.
