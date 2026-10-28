# Chunk 328 — MAPINFO hcde_shadow profile quality maxlights option bodies

## Scope

MAPINFO hcde_shadow profile quality maxlights option bodies

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **hcde_shadowprofile body** 1738–1778 (41 lines)
- `src/gamedata/g_mapinfo.cpp` **hcde_shadowqualitycap assign head** 1783–1791 (9 lines)
- `src/gamedata/g_mapinfo.cpp` **hcde_shadowmaxlightscap body** 1801–1826 (26 lines)

**Credited native LOC:** 76 (tally `nativeLoc` 76).

## Managed

`VmMapInfoHcdeShadowMapOptions.cs`, `VmMapInfoHcdeShadowMapOptionsTests.cs`
Managed LOC (tally): 40.
Conversion commit: `97caf3bcaa63ade7aeeea92ca687a5175bca3afa`.

## Tests

- `VmMapInfoHcdeShadowMapOptionsTests.ApplyShadowProfile_ClampsHigh`
- `VmMapInfoHcdeShadowMapOptionsTests.ApplyShadowQualityCap_ClearsWhenNegative`
- `VmMapInfoHcdeShadowMapOptionsTests.ApplyShadowMaxLightsCap_CapsAt1024`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 328 --to 328` (0 hard / 0 soft).

## Seams

Clamping helpers remain in `VmMapInfoHcdeShadowOptions`; this chunk models the DEFINE_MAP_OPTION bodies that assign map-level overrides.

## Credit rationale

Fills uncredited gaps in `g_mapinfo.cpp` 1738–1826 around chunk 271's spot rows: full `hcde_shadowprofile`, `hcde_shadowqualitycap`, and `hcde_shadowmaxlightscap` assignment paths including negative clear semantics and tier normalization calls.
