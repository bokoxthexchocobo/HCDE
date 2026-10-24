# Chunk 323 — MAPINFO specialaction precache redirect snd f1 options Retired (2026-10-23): out-of-body credit removed; honest **125** < **150**.

## Scope

MAPINFO specialaction precache redirect snd f1 options Retired (2026-10-23): out-of-body credit removed; honest **125** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `8ad6f4ed2e4e339d294492216717d04717bec7c5`.

## Padding review (2026-10-21)



### Out-of-body uncredit (323)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **140** |
| **Out-of-body removed** | **15** |
| **Honest** | **125** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 323 --to 323` (0 hard / 0 soft).

## Seams

`P_FindLineSpecial`, `S_FindSound`, and `ParseLumpOrTextureName` are stubbed via delegates; comma-list scanners are modeled as token enumerables.

## Credit rationale

Credits the MAPINFO tail from `specialaction` argument resolution (1421+) through `f1` lump assignment: unknown special rejection, up to five numeric args, precache sound/class/texture/eventhandler comma lists, redirect/cvar_redirect pair parsing, and string/lump option setters exercised by the tests above. Lines 1410–1420 remain on chunk 322 (`exitanim`/`enteranim` and `specialaction` header).
