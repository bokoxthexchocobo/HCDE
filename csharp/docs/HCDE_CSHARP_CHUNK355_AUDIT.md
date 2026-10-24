# Chunk 355 — FMapInfoParser ParseAMColors MAPINFO automap color blocks Retired (2026-10-23): out-of-body credit removed; honest **72** < **150**.

## Scope

FMapInfoParser ParseAMColors MAPINFO automap color blocks Retired (2026-10-23): out-of-body credit removed; honest **72** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `4781b7ac58812f5016eea3186435efac3f5b4de6`.

## Padding review (2026-10-21)



### Out-of-body uncredit (355)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **72** |
| **Out-of-body removed** | **0** |
| **Honest** | **72** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 355 --to 355` (0 hard / 0 soft).

## Seams

Scanner tokens and `V_GetColorFromString` are injected; `AMColorset` storage stays native.

## Credit rationale

Credits the full `ParseAMColors` loop: base preset selection, `showlocks`, and per-slot color name dispatch.
