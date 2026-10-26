# Chunk 218 — ParseStandardProperty subset for UMAPINFO. Retired (2026-10-23): out-of-body credit removed; honest **62** < **150**.

## Scope

ParseStandardProperty subset for UMAPINFO. Retired (2026-10-23): out-of-body credit removed; honest **62** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `16948f78a48571061658c8eba44cd573f69038ff`.

## Padding review (2026-10-21)



### Out-of-body uncredit (218)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **62** |
| **Out-of-body removed** | **0** |
| **Honest** | **62** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 218 --to 218` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
