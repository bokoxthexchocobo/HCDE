# Chunk 334 — EMAPINFO ApplyEMapInfoProperty music sky boss tail

## Scope

EMAPINFO ApplyEMapInfoProperty music sky boss tail

## Native ranges

- `src/gamedata/hcde_emapinfo.cpp` **ApplyEMapInfoProperty defaultenvironment through killstats** 665–730 (66 lines)
- `src/gamedata/hcde_emapinfo.cpp` **ApplyEMapInfoProperty music sky boss** 739–755 (17 lines)
- `src/gamedata/hcde_emapinfo.cpp` **ApplyEMapInfoProperty boss-specials** 760–777 (18 lines)

**Credited native LOC:** 101 (tally `nativeLoc` 101).

## Managed

`VmEMapInfoPropertyDispatch.cs`, `VmEMapInfoPropertyDispatchTests.cs`, `VmEMapInfoPropertyTail.cs`, `VmEMapInfoPropertyTailTests.cs`
Managed LOC (tally): 65.
Conversion commit: `73a3a7ffdd335a0394a9191c1d089af278ad7a1f`.

## Tests

- `VmEMapInfoPropertyDispatchTests.TryApplyDisableJump_SetsFlag`
- `VmEMapInfoPropertyDispatchTests.TryApplyMusic_PrefixesDoomTrack`
- `VmEMapInfoPropertyDispatchTests.TryApplySkyDelta_ConvertsTics`
- `VmEMapInfoPropertyTailTests.ApplyUnevenLight_ClearsWhenDisabled`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 334 --to 334` (0 hard / 0 soft).

## Seams

`ApplyBossSpecials` on native `level_info_t` remains a callback; music normalization reuses `VmEMapInfoMusicNormalize` and sky deltas reuse `VmEMapInfoPropertyTail` scaling.

## Credit rationale

Credits `hcde_emapinfo.cpp` 665–777: the property tail from `defaultenvironment`/`doublesky` through `boss-specials`, including music/intermusic normalization, sky name uppercasing, skydelta conversion, uneven-light clearing, and unsupported-property recording semantics.
