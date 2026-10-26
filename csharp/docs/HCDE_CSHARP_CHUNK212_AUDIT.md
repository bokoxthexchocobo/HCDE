# Chunk 212 — R_InitSpriteDefs texture hash chain only (trimmed re-audit) Retired (2026-10-23): out-of-body credit removed; honest **14** < **150**.

## Scope

R_InitSpriteDefs texture hash chain only (trimmed re-audit) Retired (2026-10-23): out-of-body credit removed; honest **14** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `073ebcc45d88795befd134b17051f24beadba13c`.

## Padding review (2026-10-21)



### Out-of-body uncredit (212)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **14** |
| **Out-of-body removed** | **0** |
| **Honest** | **14** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 212 --to 212` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Credits `SpriteNameId` hash bucket linking in `R_InitSpriteDefs` — not the full voxel lump name scan loop.
