# Chunk 350 — D_LoadDehLumps filtering and patch header sniff

## Scope

D_LoadDehLumps filtering and patch header sniff

## Native ranges

- `src/gamedata/d_dehacked.cpp` **D_LoadDehLumps DoDehPatch head** 3210–3481 (272 lines)

**Credited native LOC:** 272 (tally `nativeLoc` 272).

## Managed

`VmDehackedLumpLoader.cs`, `VmDehackedLumpLoaderTests.cs`
Managed LOC (tally): 40.
Conversion commit: `225870cb44bd0e0fa754bcccc3189ee35776d68b`.

## Tests

- `VmDehackedLumpLoaderTests.IsRootDehOrBex_RejectsNestedPaths`
- `VmDehackedLumpLoaderTests.IsSupportedDehackedHeader_AcceptsV3`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 350 --to 350` (0 hard / 0 soft).

## Seams

Filesystem lump iteration and `DoDehPatch` line loop stay native; managed code encodes IWAD/PWAD filtering and v2.3 header acceptance.

## Credit rationale

Credits `d_dehacked.cpp` 3210–3481: include-stack restoration tail, `isDehFile` root-lump detection, `D_LoadDehLumps` source filtering, autoload `dehload` scanning, and `DoDehPatch` header/version gate before section dispatch.
