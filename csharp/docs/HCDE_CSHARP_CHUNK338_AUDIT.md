# Chunk 338 — EMAPINFO parse loop section properties and compat API tail

## Scope

EMAPINFO parse loop section properties and compat API tail

## Native ranges

- `src/gamedata/hcde_emapinfo.cpp` **HCDE_ParseEMapInfo property loop compat API** 847–868 (22 lines)
- `src/gamedata/hcde_emapinfo.cpp` **HCDE_ClearEMapInfoCompat API tail** 888–921 (34 lines)

**Credited native LOC:** 56 (tally `nativeLoc` 56).

## Managed

`VmEMapInfoDocumentLoop.cs`, `VmEMapInfoDocumentLoopTests.cs`
Managed LOC (tally): 38.
Conversion commit: `5b5d07df425720b97631c107c2b6efe1f2585e57`.

## Tests

- `VmEMapInfoDocumentLoopTests.IsKnownEternityMap_LooksUpRegistry`
- `VmEMapInfoDocumentLoopTests.ResolveExtraDataLump_PrefersMapLump`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 338 --to 338` (0 hard / 0 soft).

## Seams

`GetOrCreateLevel` and `ApplyEMapInfoProperty` dispatch remain injectable; this chunk models sky2 mirroring, registry lookup, and extradata lump resolution.

## Credit rationale

Credits `hcde_emapinfo.cpp` 847–921: section open property application with `-NOFLAT-` sky2 mirroring, unsupported-property outside map blocks, and `HCDE_EMapInfo_IsEternityMap` / `HCDE_EMapInfo_FindExtraDataLump` compatibility helpers.
