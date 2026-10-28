# Chunk 261 — DEFINE_ACTION_FUNCTION _DamageTypeDefinition.IgnoreArmor Retired (2026-10-23): out-of-body credit removed; honest **6** < **150**.

## Scope

DEFINE_ACTION_FUNCTION _DamageTypeDefinition.IgnoreArmor Retired (2026-10-23): out-of-body credit removed; honest **6** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `fa9ae14c7146952387f20678ca65731b8f1c7d3e`.

## Padding review (2026-10-21)



### Out-of-body uncredit (261)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **6** |
| **Out-of-body removed** | **0** |
| **Honest** | **6** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 261 --to 261` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
