# Chunk 214 — ParseSpawnMap and InitClassMap. Retired (2026-10-23): out-of-body credit removed; honest **76** < **150**.

## Scope

ParseSpawnMap and InitClassMap. Retired (2026-10-23): out-of-body credit removed; honest **76** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `a4754442058387b337d505f08d422457f2cc2419`.

## Padding review (2026-10-21)



### Out-of-body uncredit (214)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **77** |
| **Out-of-body removed** | **1** |
| **Honest** | **76** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 214 --to 214` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
