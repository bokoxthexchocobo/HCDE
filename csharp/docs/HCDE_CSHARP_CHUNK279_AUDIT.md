# Chunk 279 — ParseMapDefinition flag handler switch Retired (2026-10-24): `--band` hull overlaps existing credit (g-mapinfo-option-dispatch-chunk280 src/gamedata/g_mapinfo.cpp 2123-2125).

## Scope

ParseMapDefinition flag handler switch Retired (2026-10-24): `--band` hull overlaps existing credit (g-mapinfo-option-dispatch-chunk280 src/gamedata/g_mapinfo.cpp 2123-2125).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `631edc8b2197bc545a9bea738fc7b5c3741c24ce`.

## Padding review (2026-10-21)



### Out-of-body uncredit (279)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **0** |
| **Honest** | **150** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 279 --to 279` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
