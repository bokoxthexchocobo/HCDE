# Chunk 362 — HCDE MAPINFO optional float legacy compat for sky speeds Retired (2026-10-23): out-of-body credit removed; honest **34** < **150**.

## Scope

HCDE MAPINFO optional float legacy compat for sky speeds Retired (2026-10-23): out-of-body credit removed; honest **34** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `217302b65017d6872814787653fbfd83c903b41d`.

## Padding review (2026-10-21)



### Out-of-body uncredit (362)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **34** |
| **Out-of-body removed** | **0** |
| **Honest** | **34** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 362 --to 362` (0 hard / 0 soft).

## Seams

Parser scanner calls are injected; one-time `Printf` warning uses a ref flag like native static `warned`.

## Credit rationale

Credits `HCDE_MapInfo_CheckOptionalFloatCompat` including legacy float/number promotion and mod-compat warning gate.
