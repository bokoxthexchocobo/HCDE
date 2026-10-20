# Chunk 340 — Automap globals constants and CVAR clamp/cheat gates

## Scope

Automap globals constants and CVAR clamp/cheat gates

## Native ranges

- `src/am_map.cpp` **Automap constants and CVAR head** 66–183 (118 lines)

**Credited native LOC:** 118 (tally `nativeLoc` 118).

## Managed

`VmAutomapCvarModel.cs`, `VmAutomapCvarModelTests.cs`
Managed LOC (tally): 24.
Conversion commit: `d567e408bbdc4a5c120cd82526bca36c5c05fe95`.

## Tests

- `VmAutomapCvarModelTests.ClampEmptySpaceMargin_Bounds`
- `VmAutomapCvarModelTests.ShouldBlockAutomapCheat_WhenNetNoCheats`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 340 --to 340` (0 hard / 0 soft).

## Seams

Engine `CVAR`/`CUSTOM_CVAR` registration and `primaryLevel->automap` callbacks are not re-hosted; only numeric policy from the opening `am_map.cpp` block is modeled.

## Credit rationale

First whole-file port slice for untouched `am_map.cpp`: automap mark-point count, player radius, zoom rates, `am_emptyspacemargin` clamp 0–90, and net-game `am_cheat` rejection when `sv_cheats` is off.
