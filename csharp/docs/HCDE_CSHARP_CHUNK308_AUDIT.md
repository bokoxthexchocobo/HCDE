# Chunk 308 — G_ParseMapInfo FindLumpMulti skip/precedence Retired (2026-10-23): out-of-body credit removed; honest **35** < **150**.

## Scope

G_ParseMapInfo FindLumpMulti skip/precedence Retired (2026-10-23): out-of-body credit removed; honest **35** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppLumpGate.cs`
Managed LOC (tally): 0.
Conversion commit: `cc6bd7b876179429aac0f1acf9c9476604b51ca5`.

## Padding review (2026-10-21)



### Out-of-body uncredit (308)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **35** |
| **Out-of-body removed** | **0** |
| **Honest** | **35** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 308 --to 308` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
