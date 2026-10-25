# Chunk 787 — `P_TeleportMove` and `P_PlayerStartStomp` (`p_map.cpp` 458–638).

## Scope

`P_TeleportMove` and `P_PlayerStartStomp` (`p_map.cpp` 458–638).

## Native ranges

- `src/playsim/p_map.cpp` **P_TeleportMove** 458–589 (132 lines)
- `src/playsim/p_map.cpp` **P_PlayerStartStomp** 600–638 (39 lines)

**Credited native LOC:** 171 (tally `nativeLoc` 171).

## Managed

`VmPMapTeleportMove.cs`
Managed LOC (tally): 235.
Conversion commit: `9376162dea1e7f21710c477b50889e206dea0252`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|---------------------|
| `TeleportMove` | `P_TeleportMove` **458–589** |
| `PlayerStartStomp` | `P_PlayerStartStomp` **600–638** |

### Out-of-body uncredit (787)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **171** |
| **Out-of-body removed** | **0** |
| **Honest** | **171** |


**Post-review ratio:** 235/171 managed/native ≈ 137% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapTeleportMoveTests.TeleportMove_FailsWhenSolidActorBlocks`
- `VmPMapTeleportMoveTests.TeleportMove_TelefragsWhenFlagged`
- `VmPMapTeleportMoveTests.PlayerStartStomp_DamagesMonsterUnderfoot`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 787 --to 787` (0 hard / 0 soft).

## Seams

`FMultiBlockLinesIterator`, `FMultiBlockThingsIterator`, `P_GetFloorCeilingZ`, `P_CanCollideWith`, and `P_DamageMobj` are injected on `TeleportContext`.

## Credit rationale

Whole-function teleport collision and player-start stomp after floor/ceiling chunk786.
