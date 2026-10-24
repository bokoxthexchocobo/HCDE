# Chunk 697 — Whole `EV_Teleport`, `EV_TeleportOther`, `DoGroupForOne`, and `EV_TeleportGroup` (`p_teleport.cpp` 376–457, 651–751).

## Scope

Whole `EV_Teleport`, `EV_TeleportOther`, `DoGroupForOne`, and `EV_TeleportGroup` (`p_teleport.cpp` 376–457, 651–751).

## Native ranges

- `src/playsim/p_teleport.cpp` **FLevelLocals::EV_Teleport** 376–457 (82 lines)
- `src/playsim/p_teleport.cpp` **FLevelLocals::EV_TeleportOther** 651–668 (18 lines)
- `src/playsim/p_teleport.cpp` **DoGroupForOne** 676–692 (17 lines)
- `src/playsim/p_teleport.cpp` **FLevelLocals::EV_TeleportGroup** 701–751 (51 lines)

**Credited native LOC:** 168 (tally `nativeLoc` 168).

## Managed

`VmPTeleportEv.cs`
Managed LOC (tally): 195.
Conversion commit: `1c58f3190736ce44e712311daf13fed4f35af31c`.

## Padding review (2026-10-21)

| Managed | Native (`p_teleport.cpp`) |
|---------|-------------------------|
| `EvTeleport` | `EV_Teleport` 376–457 |
| `EvTeleportOther` | `EV_TeleportOther` 651–668 |
| `DoGroupForOne` | `DoGroupForOne` 676–692 |
| `EvTeleportGroup` | `EV_TeleportGroup` 701–751 |


**Post-review ratio:** 195/168 managed/native ≈ 116% (src file LOC at HEAD; see tally).

## Tests

- `VmPTeleportEvTests.EvTeleport_RejectsBackSide`
- `VmPTeleportEvTests.EvTeleport_SucceedsWhenDestFound`
- `VmPTeleportEvTests.EvTeleportOther_TeleportsEachVictim`
- `VmPTeleportEvTests.EvTeleportGroup_FallsBackWhenNoSource`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 697 --to 697` (0 hard / 0 soft).

## Seams

`SelectTeleDest`, `P_Teleport`, `PlayIdle`, iterators, and `P_TeleportMove` are injected.

## Credit rationale

Line teleporter and TID group moves before sector group chunk (deferred).
