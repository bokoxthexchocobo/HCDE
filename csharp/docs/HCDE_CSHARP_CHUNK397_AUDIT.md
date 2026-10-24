# Chunk 397 — Automap CUSTOM_CVAR callbacks, color tables, colorset helpers Retired (2026-10-23): out-of-body credit removed; honest **30** < **150**.

## Scope

Automap CUSTOM_CVAR callbacks, color tables, colorset helpers Retired (2026-10-23): out-of-body credit removed; honest **30** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `434c938673ae43606db60b426aa8f38b26ae089c`.

## Padding review (2026-10-21)



### Out-of-body uncredit (397)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **161** |
| **Out-of-body removed** | **131** |
| **Honest** | **30** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 397 --to 397` (0 hard / 0 soft).

## Seams

Engine CVAR registration and `primaryLevel->automap` method calls are not hosted.

## Credit rationale

Credits automap CUSTOM_CVAR callbacks, standard/overlay CVAR defaults, `cv_standard`/`cv_overlay`, and `AMColorset` white/lookup helpers from `am_map.cpp` (no overlap with chunks 340/352/354/379/385/393).
