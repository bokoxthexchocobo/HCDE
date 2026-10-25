# Chunk 408 — DFireFlicker, DFlicker construct, DLightFlash, DStrobe thinkers Retired (2026-10-23): out-of-body credit removed; honest **121** < **150**.

## Scope

DFireFlicker, DFlicker construct, DLightFlash, DStrobe thinkers Retired (2026-10-23): out-of-body credit removed; honest **121** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `e3cc909ede174f641c392bf983fe49e6ac765ff6`.

## Padding review (2026-10-21)



### Out-of-body uncredit (408)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **167** |
| **Out-of-body removed** | **46** |
| **Honest** | **121** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 408 --to 408` (0 hard / 0 soft).

## Seams

`sector_t`, `pr_*` RNG, and thinker allocation are not hosted.

## Credit rationale

Credits `DFireFlicker`, `DFlicker`, `DLightFlash`, and `DStrobe` logic from `a_lights.cpp` (bands avoid recognition-only `DFlicker::Tick`).
