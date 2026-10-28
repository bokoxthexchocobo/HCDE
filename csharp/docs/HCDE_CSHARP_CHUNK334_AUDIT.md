# Chunk 334 — EMAPINFO ApplyEMapInfoProperty music sky boss tail Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

EMAPINFO ApplyEMapInfoProperty music sky boss tail Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `616e46729d9c6abb727c2d884e51bdb320a54f26`.

## Padding review (2026-10-21)



### Out-of-body uncredit (334)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **101** |
| **Out-of-body removed** | **101** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 334 --to 334` (0 hard / 0 soft).

## Seams

`ApplyBossSpecials` on native `level_info_t` remains a callback; music normalization reuses `VmEMapInfoMusicNormalize` and sky deltas reuse `VmEMapInfoPropertyTail` scaling.

## Credit rationale

Credits `hcde_emapinfo.cpp` 665–777: the property tail from `defaultenvironment`/`doublesky` through `boss-specials`, including music/intermusic normalization, sky name uppercasing, skydelta conversion, uneven-light clearing, and unsupported-property recording semantics.
