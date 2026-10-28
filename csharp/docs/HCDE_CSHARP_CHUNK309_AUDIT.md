# Chunk 309 — LoadDehSupp lump gate Retired (2026-10-23): out-of-body credit removed; honest **18** < **150**.

## Scope

LoadDehSupp lump gate Retired (2026-10-23): out-of-body credit removed; honest **18** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppLumpGate.cs`
Managed LOC (tally): 0.
Conversion commit: `0acb373ba1d7a444c1cf3142b3953a9e3bc4d4a9`.

## Padding review (2026-10-21)



### Out-of-body uncredit (309)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **18** |
| **Out-of-body removed** | **0** |
| **Honest** | **18** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 309 --to 309` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
