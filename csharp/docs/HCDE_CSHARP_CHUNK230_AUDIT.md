# Chunk 230 — Terrain outer keyword table (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

Terrain outer keyword table (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 71.
Conversion commit: `34f7a9e8eee36a64353b98ddbd8b86d05e09d8c1`.

## Padding review (2026-10-21)



### Out-of-body uncredit (230)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **78** |
| **Out-of-body removed** | **78** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 230 --to 230` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Credits the `OuterKeywords` catalog mirrored by `VmTerrainKeywordSchema` — not splash prototype bodies.
