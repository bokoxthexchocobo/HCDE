# Chunk 697 — Whole `EV_Teleport`, `EV_TeleportOther`, `DoGroupForOne`, and `EV_TeleportGroup` (`p_teleport.cpp` 376–457, 651–751). Retired (2026-10-24): `--band` hull overlaps existing credit (p-teleport-silent-line-chunk696 src/playsim/p_teleport.cpp 471-643).

## Scope

Whole `EV_Teleport`, `EV_TeleportOther`, `DoGroupForOne`, and `EV_TeleportGroup` (`p_teleport.cpp` 376–457, 651–751). Retired (2026-10-24): `--band` hull overlaps existing credit (p-teleport-silent-line-chunk696 src/playsim/p_teleport.cpp 471-643).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `e7c0e42c39054bb5ff3cae969137c65a520b5bb5`.

## Padding review (2026-10-21)

| Managed | Native (`p_teleport.cpp`) |
|---------|-------------------------|
| `EvTeleport` | `EV_Teleport` 376–457 |
| `EvTeleportOther` | `EV_TeleportOther` 651–668 |
| `DoGroupForOne` | `DoGroupForOne` 676–692 |
| `EvTeleportGroup` | `EV_TeleportGroup` 701–751 |

### Out-of-body uncredit (697)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **168** |
| **Out-of-body removed** | **0** |
| **Honest** | **168** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 697 --to 697` (0 hard / 0 soft).

## Seams

`SelectTeleDest`, `P_Teleport`, `PlayIdle`, iterators, and `P_TeleportMove` are injected.

## Credit rationale

Line teleporter and TID group moves before sector group chunk (deferred).
