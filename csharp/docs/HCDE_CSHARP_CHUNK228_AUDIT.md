# Chunk 228 — g_skill globals and TeamInfoOptions table (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **49** < **150**.

## Scope

g_skill globals and TeamInfoOptions table (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **49** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 322.
Conversion commit: `2c81a481b1c28b3a9ef91607036967b34c70f1c9`.

## Padding review (2026-10-21)



### Out-of-body uncredit (228)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **57** |
| **Out-of-body removed** | **8** |
| **Honest** | **49** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 228 --to 228` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Credits skill list globals, name token assign, and team option table — not `FTeam` accessor surface or `teamlist` CCMD.
