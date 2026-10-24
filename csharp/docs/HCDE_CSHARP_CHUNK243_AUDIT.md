# Chunk 243 — STAT_StartNewGame and StoreLevelStats. Retired (2026-10-23): out-of-body credit removed; honest **60** < **150**.

## Scope

STAT_StartNewGame and StoreLevelStats. Retired (2026-10-23): out-of-body credit removed; honest **60** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `1f5d5cf5e60dea749c44b2825aff2c33b4629377`.

## Padding review (2026-10-21)



### Out-of-body uncredit (243)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **60** |
| **Out-of-body removed** | **0** |
| **Honest** | **60** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 243 --to 243` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
