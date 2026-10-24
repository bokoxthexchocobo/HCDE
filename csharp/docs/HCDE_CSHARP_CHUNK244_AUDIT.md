# Chunk 244 — statistics.cpp remainder through file tail. Retired (2026-10-24): thin managed at HEAD — surviving **0** < **84** (50% of honest **168**).

## Scope

statistics.cpp remainder through file tail. Retired (2026-10-24): thin managed at HEAD — surviving **0** < **84** (50% of honest **168**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `d70bce8ddd1d0de3352d8cad9a9cff2e57622c4e`.

## Padding review (2026-10-21)



### Out-of-body uncredit (244)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **199** |
| **Out-of-body removed** | **31** |
| **Honest** | **168** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 244 --to 244` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
