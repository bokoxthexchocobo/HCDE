# Chunk 292 — MBF defaults Retired (2026-10-23): out-of-body credit removed; honest **132** < **150**.

## Scope

MBF defaults Retired (2026-10-23): out-of-body credit removed; honest **132** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `cba8df2c38b037bfee6f85f1dda51ee15b07ae60`.

## Padding review (2026-10-21)



### Out-of-body uncredit (292)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **18** |
| **Honest** | **132** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 292 --to 292` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
