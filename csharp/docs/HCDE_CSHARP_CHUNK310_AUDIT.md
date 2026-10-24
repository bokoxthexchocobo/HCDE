# Chunk 310 — LoadDehSupp ActionList Retired (2026-10-23): out-of-body credit removed; honest **36** < **150**.

## Scope

LoadDehSupp ActionList Retired (2026-10-23): out-of-body credit removed; honest **36** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppActionList.cs`
Managed LOC (tally): 0.
Conversion commit: `9b24de6a65a75f30b855624b5c792c15f86b946e`.

## Padding review (2026-10-21)



### Out-of-body uncredit (310)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **36** |
| **Out-of-body removed** | **0** |
| **Honest** | **36** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 310 --to 310` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
