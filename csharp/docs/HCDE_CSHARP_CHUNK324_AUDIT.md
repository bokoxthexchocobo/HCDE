# Chunk 324 — MAPINFO mapbackground exit text music flat pic defaultenvironment hazard Retired (2026-10-23): out-of-body credit removed; honest **74** < **150**.

## Scope

MAPINFO mapbackground exit text music flat pic defaultenvironment hazard Retired (2026-10-23): out-of-body credit removed; honest **74** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `39fa278456ee79b539a84273f017aae6e46bef92`.

## Padding review (2026-10-21)



### Out-of-body uncredit (324)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **81** |
| **Out-of-body removed** | **7** |
| **Honest** | **74** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 324 --to 324` (0 hard / 0 soft).

## Seams

`ParseLumpOrTextureName`, `V_GetColor`, and `S_FindEnvironment` are injected; exit-table merges mirror `FExitText` without hosting `FMapInfoParser` scanner state.

## Credit rationale

Fills the MAPINFO gap between chunk 323 (`f1`) and the spot-credited fog/light scalars on chunk 264: `mapbackground` lump assignment, the four `text*` handlers that key `ExitMapTexts` by destination map, numeric/named `defaultenvironment` ID composition, and `hazardcolor`/`hazardflash` color assignment. Does not re-credit `teamdamage` or fog/light handlers already on chunk 264.
