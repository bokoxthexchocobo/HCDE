# Chunk 696 — Whole `FLevelLocals::EV_SilentLineTeleport` (`p_teleport.cpp` 471–643).

## Scope

Whole `FLevelLocals::EV_SilentLineTeleport` (`p_teleport.cpp` 471–643).

## Native ranges

- `src/playsim/p_teleport.cpp` **FLevelLocals::EV_SilentLineTeleport** 471–643 (173 lines)

**Credited native LOC:** 173 (tally `nativeLoc` 173).

## Managed

`VmPTeleportSilentLine.cs`
Managed LOC (tally): 137.
Conversion commit: `ac79e5ec4efe4bf6aafe7ce5e54525da75ae3c84`.

## Padding review (2026-10-21)

| Managed | Native (`p_teleport.cpp`) |
|---------|-------------------------|
| `VmPTeleportSilentLine.EvSilentLineTeleport` | `EV_SilentLineTeleport` 471–643 |


**Post-review ratio:** 137/173 managed/native ≈ 79% (src file LOC at HEAD; see tally).

## Tests

- `VmPTeleportSilentLineTests.EvSilentLineTeleport_RejectsWhenNoBackSidedef`
- `VmPTeleportSilentLineTests.EvSilentLineTeleport_TeleportsWhenCandidateMatches`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 696 --to 696` (0 hard / 0 soft).

## Seams

Line iterators, `P_PointOnLineSidePrecise`, `P_TeleportMove`, and `CalcHeight` VM are injected.

## Credit rationale

Silent line-preserving teleporter after `EV_Teleport` chunk 697.
