# Chunk 249 — Re-audit: NormalizeResourceName only. Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

Re-audit: NormalizeResourceName only. Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `4f08b22046b6278b34999b59281d31c5c221912b`.

## Padding review (2026-10-21)



### Out-of-body uncredit (249)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **48** |
| **Out-of-body removed** | **48** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 249 --to 249` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
