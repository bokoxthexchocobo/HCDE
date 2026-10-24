# Chunk 329 — MAPINFO edata/intro and e1m8special row (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **10** < **150**.

## Scope

MAPINFO edata/intro and e1m8special row (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **10** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `53a34fca33de615c40401d65a7d37fb50bf97646`.

## Padding review (2026-10-21)



### Out-of-body uncredit (329)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **11** |
| **Out-of-body removed** | **1** |
| **Honest** | **10** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 329 --to 329` (0 hard / 0 soft).

## Seams

`ParseCutscene` is reduced to lump string assignment; `MapFlagHandlers` rows keep placeholder flag constants matching the native table layout without loading `level_info_t` flag words.

## Credit rationale

Credits `edata`/`intro` string assigns and the `e1m8special` handler catalog row — not the full `MapFlagHandlers` head.
