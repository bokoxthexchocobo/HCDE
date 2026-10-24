# Chunk 233 — ReadAllDecals and decallib scattered gap lines. Retired (2026-10-23): out-of-body credit removed; honest **68** < **150**.

## Scope

ReadAllDecals and decallib scattered gap lines. Retired (2026-10-23): out-of-body credit removed; honest **68** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `3289c0d035f23b51c71ae942fba423a82597e7f2`.

## Padding review (2026-10-21)



### Out-of-body uncredit (233)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **95** |
| **Out-of-body removed** | **27** |
| **Honest** | **68** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 233 --to 233` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
