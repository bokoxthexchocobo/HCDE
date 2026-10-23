# Chunk 232 — FDecalGroup, FDecalLib clear/tree, decal keyword schema, animator structs header. Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

FDecalGroup, FDecalLib clear/tree, decal keyword schema, animator structs header. Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `31b32139b8559972d8221c08aa287c0bfde453aa`.

## Padding review (2026-10-21)



### Out-of-body uncredit (232)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **223** |
| **Out-of-body removed** | **223** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 232 --to 232` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
