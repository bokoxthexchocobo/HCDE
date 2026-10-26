# Chunk 215 — UMapInfo multi-string, lump name, player action option. Retired (2026-10-23): out-of-body credit removed; honest **58** < **150**.

## Scope

UMapInfo multi-string, lump name, player action option. Retired (2026-10-23): out-of-body credit removed; honest **58** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppLumpGate.cs`
Managed LOC (tally): 0.
Conversion commit: `0de54e407fc6127b82c1c181ac30d754c0f281a4`.

## Padding review (2026-10-21)



### Out-of-body uncredit (215)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **58** |
| **Out-of-body removed** | **0** |
| **Honest** | **58** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 215 --to 215` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
