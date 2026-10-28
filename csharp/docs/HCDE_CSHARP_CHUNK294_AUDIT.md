# Chunk 294 — EMAPINFO tail Retired (2026-10-23): out-of-body credit removed; honest **20** < **150**.

## Scope

EMAPINFO tail Retired (2026-10-23): out-of-body credit removed; honest **20** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `0c1749af2c96e4cc5e62ad13ad1c9d2174a13a79`.

## Padding review (2026-10-21)



### Out-of-body uncredit (294)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **130** |
| **Honest** | **20** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 294 --to 294` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
