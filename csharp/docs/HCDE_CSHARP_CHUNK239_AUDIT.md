# Chunk 239 — a_keys uncredited gap lines and LOCKDEF tail. Retired (2026-10-24): `uncredited_range_check.py --chunk 239 --band` OVERLAP **173** (`lockdef-parse-chunk173`).

## Scope

a_keys uncredited gap lines and LOCKDEF tail. Retired (2026-10-24): `uncredited_range_check.py --chunk 239 --band` OVERLAP **173** (`lockdef-parse-chunk173`).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `c2845641abbcd3d0dae0a3e954ffdbc3cdb386ee`.

## Padding review (2026-10-21)



### Out-of-body uncredit (239)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **243** |
| **Out-of-body removed** | **75** |
| **Honest** | **168** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 239 --to 239` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
