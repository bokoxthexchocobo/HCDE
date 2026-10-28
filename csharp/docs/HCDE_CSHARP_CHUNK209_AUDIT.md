# Chunk 209 — VOXELDEF sprite name list, option block, ApplyVoxelToSprites. Retired (2026-10-23): out-of-body credit removed; honest **54** < **150**.

## Scope

VOXELDEF sprite name list, option block, ApplyVoxelToSprites. Retired (2026-10-23): out-of-body credit removed; honest **54** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `9b46a68616ea5b467244e664b99d8710a4535434`.

## Padding review (2026-10-21)



### Out-of-body uncredit (209)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **184** |
| **Out-of-body removed** | **130** |
| **Honest** | **54** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 209 --to 209` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
