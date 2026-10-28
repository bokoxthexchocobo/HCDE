# Chunk 418 — ACS execute/suspend, FS execute, LowerRaise boom gate, light line specials Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

ACS execute/suspend, FS execute, LowerRaise boom gate, light line specials Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppLumpGate.cs`
Managed LOC (tally): 0.
Conversion commit: `d03b1975134d73bde324dcadda001a6f2d95d6dd`.

## Padding review (2026-10-21)



### Out-of-body uncredit (418)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **205** |
| **Out-of-body removed** | **205** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 418 --to 418` (0 hard / 0 soft).

## Seams

`P_StartScript`, `T_RunScript`, `EV_*` light movers, and `ForceLightning` are not hosted.

## Credit rationale

Credits ACS map resolution, locked execute gates, FS execute checks, Boom `LowerRaise` floor fallback, and `LS_Light_*` argument wiring from `p_lnspec.cpp`.
