# Chunk 277 — PatchMisc key table Retired (2026-10-24): `--band` hull overlaps existing credit (d-dehacked-patch-misc-chunk513 src/gamedata/d_dehacked.cpp 2537-2730).

## Scope

PatchMisc key table Retired (2026-10-24): `--band` hull overlaps existing credit (d-dehacked-patch-misc-chunk513 src/gamedata/d_dehacked.cpp 2537-2730).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `e203b920b08ebe38097318494b5e67697a0534ff`.

## Padding review (2026-10-21)



### Out-of-body uncredit (277)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **0** |
| **Honest** | **150** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 277 --to 277` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
