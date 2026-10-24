# Chunk 271 — MAPINFO lightblend, HCDE shadow, skyrotate, toggles, edname/acs. Retired (2026-10-24): `--band` hull overlaps existing credit (g-mapinfo-script-flags-head-chunk329 src/gamedata/g_mapinfo.cpp 1892-1897).

## Scope

MAPINFO lightblend, HCDE shadow, skyrotate, toggles, edname/acs. Retired (2026-10-24): `--band` hull overlaps existing credit (g-mapinfo-script-flags-head-chunk329 src/gamedata/g_mapinfo.cpp 1892-1897).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `724e489af1a47c5f1b29fc9c4ce978dfa14d35fc`.

## Padding review (2026-10-21)



### Out-of-body uncredit (271)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **173** |
| **Out-of-body removed** | **0** |
| **Honest** | **173** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 271 --to 271` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
