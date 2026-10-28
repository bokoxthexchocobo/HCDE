# Chunk 340 — Automap constants cheat gate and margin clamp (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **13** < **150**.

## Scope

Automap constants cheat gate and margin clamp (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **13** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppLumpGate.cs`
Managed LOC (tally): 0.
Conversion commit: `5d56fd43eca73fef03f3e3d53818573e72b5b4ed`.

## Padding review (2026-10-21)



### Out-of-body uncredit (340)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **24** |
| **Out-of-body removed** | **11** |
| **Honest** | **13** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 340 --to 340` (0 hard / 0 soft).

## Seams

Engine `CVAR`/`CUSTOM_CVAR` registration and `primaryLevel->automap` callbacks are not re-hosted; only numeric policy from the opening `am_map.cpp` block is modeled.

## Credit rationale

Credits automap numeric constants, net-game `am_cheat` gate, and `am_emptyspacemargin` clamp — not CVAR registration blocks.
