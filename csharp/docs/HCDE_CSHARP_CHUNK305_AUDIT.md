# Chunk 305 — LoadDehSupp WeaponNames brace list Retired (2026-10-23): out-of-body credit removed; honest **16** < **150**.

## Scope

LoadDehSupp WeaponNames brace list Retired (2026-10-23): out-of-body credit removed; honest **16** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `28431b5e4bcab44e1206e4a762003bae11974052`.

## Padding review (2026-10-21)



### Out-of-body uncredit (305)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **17** |
| **Out-of-body removed** | **1** |
| **Honest** | **16** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 305 --to 305` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
