# Chunk 323 — MAPINFO specialaction precache redirect snd f1 options

## Scope

MAPINFO specialaction precache redirect snd f1 options

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **MAPINFO specialaction through f1** 1421–1560 (140 lines)

**Credited native LOC:** 140 (tally `nativeLoc` 140).

## Managed

`VmMapInfoMapOptionsLate.cs`, `VmMapInfoMapOptionsLateTests.cs`
Managed LOC (tally): 95.
Conversion commit: `69cea7ef8972685bfbb3da78e46df7abc7b4c905`.

## Tests

- `VmMapInfoMapOptionsLateTests.TryResolveSpecialAction_RejectsUnknown`
- `VmMapInfoMapOptionsLateTests.ParsePrecacheSounds_SkipsNoSound`
- `VmMapInfoMapOptionsLateTests.ParseCommaSeparatedStrings_ReadsEventHandlers`
- `VmMapInfoMapOptionsLateTests.ApplyRedirect_SetsTypeAndMap`
- `VmMapInfoMapOptionsLateTests.ApplyStringOption_SetsTranslator`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 323 --to 323` (0 hard / 0 soft).

## Seams

`P_FindLineSpecial`, `S_FindSound`, and `ParseLumpOrTextureName` are stubbed via delegates; comma-list scanners are modeled as token enumerables.

## Credit rationale

Credits the MAPINFO tail from `specialaction` argument resolution (1421+) through `f1` lump assignment: unknown special rejection, up to five numeric args, precache sound/class/texture/eventhandler comma lists, redirect/cvar_redirect pair parsing, and string/lump option setters exercised by the tests above. Lines 1410–1420 remain on chunk 322 (`exitanim`/`enteranim` and `specialaction` header).
