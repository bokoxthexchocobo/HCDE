# Chunk 434 — MODtoDamageType, CRUSHTYPE helpers, ACS name map, LS_NOP, polyobj movers Retired (2026-10-23): out-of-body credit removed; honest **38** < **150**.

## Scope

MODtoDamageType, CRUSHTYPE helpers, ACS name map, LS_NOP, polyobj movers Retired (2026-10-23): out-of-body credit removed; honest **38** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `7c513c38eb7ca9b98223a1f6a930282a1ae77310`.

## Padding review (2026-10-21)



### Out-of-body uncredit (434)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **161** |
| **Out-of-body removed** | **123** |
| **Honest** | **38** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 434 --to 434` (0 hard / 0 soft).

## Seams

`EV_RotatePoly`, `EV_MovePoly`, and `EV_OpenPolyDoor` are not hosted.

## Credit rationale

Credits damage-type name mapping, ceiling crush mode selection, named ACS opcode table, and polyobj rotate/move/door argument wiring from `p_lnspec.cpp`.
