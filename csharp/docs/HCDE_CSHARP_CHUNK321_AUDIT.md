# Chunk 321 — MAPINFO scalar/environment option clamps (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **23** < **150**.

## Scope

MAPINFO scalar/environment option clamps (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **23** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `95cabb6d76243d668eede6b80b1e0705e13f62be`.

## Padding review (2026-10-21)



### Out-of-body uncredit (321)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **23** |
| **Out-of-body removed** | **0** |
| **Honest** | **23** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 321 --to 321` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Credits skymist/skymistyscale/wavelimit/music and environment scalars exercised by map-option tests — not full DEFINE blocks through sucktime.
