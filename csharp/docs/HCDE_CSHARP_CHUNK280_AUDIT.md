# Chunk 280 — MAPINFO registered option dispatch Retired (2026-10-23): out-of-body credit removed; honest **133** < **150**.

## Scope

MAPINFO registered option dispatch Retired (2026-10-23): out-of-body credit removed; honest **133** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `a30c2511b42acaf3ed38cb17d2ebeef92d04d6b2`.

## Padding review (2026-10-21)



### Out-of-body uncredit (280)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **17** |
| **Honest** | **133** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 280 --to 280` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
