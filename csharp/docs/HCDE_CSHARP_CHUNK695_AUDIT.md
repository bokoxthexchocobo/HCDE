# Chunk 695 — Whole `P_SpawnTeleportFog` and `P_Teleport` (`p_teleport.cpp` 45–61, 81–255).

## Scope

Whole `P_SpawnTeleportFog` and `P_Teleport` (`p_teleport.cpp` 45–61, 81–255).

## Native ranges

- `src/playsim/p_teleport.cpp` **P_SpawnTeleportFog** 45–61 (17 lines)
- `src/playsim/p_teleport.cpp` **P_Teleport** 81–255 (175 lines)

**Credited native LOC:** 192 (tally `nativeLoc` 192).

## Managed

`VmPTeleportCore.cs`
Managed LOC (tally): 192.
Conversion commit: `d49cab282cf1517ac91224c01cc99b2245bbfb8a`.

## Padding review (2026-10-21)

| Managed | Native (`p_teleport.cpp`) |
|---------|-------------------------|
| `SpawnTeleportFog` | `P_SpawnTeleportFog` 45–61 |
| `Teleport` (Z resolve, pre/move/post, fog, freeze, velocity) | `P_Teleport` 81–255 |

### Out-of-body uncredit (695)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **192** |
| **Out-of-body removed** | **0** |
| **Honest** | **192** |


**Post-review ratio:** 192/192 managed/native ≈ 100% (src file LOC at HEAD; see tally).

## Tests

- `VmPTeleportCoreTests.SpawnTeleportFog_SkipsWhenTypeNull`
- `VmPTeleportCoreTests.Teleport_AbortsWhenPreTeleportCancels`
- `VmPTeleportCoreTests.Teleport_OnFloorZSnapsPlayerToFloor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 695 --to 695` (0 hard / 0 soft).

## Seams

`Spawn`, `P_TeleportMove`, VM `PreTeleport`/`PostTeleport`, fog classes, and player FOV/rumble are injected.

## Credit rationale

Core actor teleport after chunk6 `SelectTeleDest` tail.
