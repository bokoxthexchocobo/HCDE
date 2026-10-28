# Chunk 265 — PatchThing setup and numeric property dispatch. Retired (2026-10-24): `--band` hull overlaps existing credit (d-dehacked-patchthing-loop-primary-chunk520 src/gamedata/d_dehacked.cpp 1400-1550).

## Scope

PatchThing setup and numeric property dispatch. Retired (2026-10-24): `--band` hull overlaps existing credit (d-dehacked-patchthing-loop-primary-chunk520 src/gamedata/d_dehacked.cpp 1400-1550).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `4c2d1ab11cff6881ba95e35fa401b4999be973e4`.

## Padding review (2026-10-21)



### Out-of-body uncredit (265)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **151** |
| **Out-of-body removed** | **0** |
| **Honest** | **151** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 265 --to 265` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
