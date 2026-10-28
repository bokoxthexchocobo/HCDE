# Chunk 296 — MAPINFO scalar options, intro/outro, MapFlagHandlers compat tail Retired (2026-10-23): out-of-body credit removed; honest **92** < **150**.

## Scope

MAPINFO scalar options, intro/outro, MapFlagHandlers compat tail Retired (2026-10-23): out-of-body credit removed; honest **92** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `1c997a7b7d49916d170307c6d18588cc61aca469`.

## Padding review (2026-10-21)



### Out-of-body uncredit (296)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **153** |
| **Out-of-body removed** | **61** |
| **Honest** | **92** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 296 --to 296` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
