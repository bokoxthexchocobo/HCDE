# Chunk 281 — FWeaponSlots locate and defaults Retired (2026-10-23): out-of-body credit removed; honest **106** < **150**.

## Scope

FWeaponSlots locate and defaults Retired (2026-10-23): out-of-body credit removed; honest **106** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `981e809e70feec91415862a523a9672edd19c539`.

## Padding review (2026-10-21)



### Out-of-body uncredit (281)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **44** |
| **Honest** | **106** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 281 --to 281` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
