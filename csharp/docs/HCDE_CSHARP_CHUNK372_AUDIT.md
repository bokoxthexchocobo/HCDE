# Chunk 372 — startDisplay, LevelInit, scale, Responder, changeWindowScale, doFollowPlayer policy Retired (2026-10-24): thin managed at HEAD — surviving **0** < **97** (50% of honest **194**).

## Scope

startDisplay, LevelInit, scale, Responder, changeWindowScale, doFollowPlayer policy Retired (2026-10-24): thin managed at HEAD — surviving **0** < **97** (50% of honest **194**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `826b289bea6909edb96788b5036ec9cf1ee9531c`.

## Padding review (2026-10-21)



### Out-of-body uncredit (372)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **199** |
| **Out-of-body removed** | **5** |
| **Honest** | **194** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 372 --to 372` (0 hard / 0 soft).

## Seams

Engine object graph (`players`, `StatusBar`, `twod`) is not hosted; managed code mirrors numeric/session rules only.

## Credit rationale

Credits `startDisplay`, `clearMarks`, `LevelInit`, `minOutWindowScale`, `maxOutWindowScale`, `NewUIScale`, `NewResolution`, `Responder`, `changeWindowScale`, and `doFollowPlayer` from `am_map.cpp`.
