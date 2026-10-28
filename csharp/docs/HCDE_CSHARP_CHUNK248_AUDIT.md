# Chunk 248 — Re-audit: DmgFactors::Apply, SummonActor net payload, ParseDamageDefinition subset. Retired (2026-10-23): out-of-body credit removed; honest **36** < **150**.

## Scope

Re-audit: DmgFactors::Apply, SummonActor net payload, ParseDamageDefinition subset. Retired (2026-10-23): out-of-body credit removed; honest **36** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `6276812ac96ebfe555a35a160b88f65d1c1fa469`.

## Padding review (2026-10-21)



### Out-of-body uncredit (248)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **63** |
| **Out-of-body removed** | **27** |
| **Honest** | **36** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 248 --to 248` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
