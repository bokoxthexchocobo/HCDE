# Chunk 705 — Whole `P_CheckPosition` (`p_map.cpp` 1817–2034). Trims recognition `P_CheckPosition` band.

## Scope

Whole `P_CheckPosition` (`p_map.cpp` 1817–2034). Trims recognition `P_CheckPosition` band.

## Native ranges

- `src/playsim/p_map.cpp` **P_CheckPosition** 1817–2034 (218 lines)

**Credited native LOC:** 218 (tally `nativeLoc` 218).

## Managed

`VmPMapCheckPosition.cs`
Managed LOC (tally): 280.
Conversion commit: `858d2464158103ac603253bef60da1bf4ed0f33c`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `CheckPosition` | `P_CheckPosition` 1817–2034 |

### Out-of-body uncredit (705)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **218** |
| **Out-of-body removed** | **0** |
| **Honest** | **218** |


**Post-review ratio:** 280/218 managed/native ≈ 128% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapCheckPositionTests.CheckPosition_NoclipReturnsTrue`
- `VmPMapCheckPositionTests.CheckPosition_HeightFailWhenCrushed`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 705 --to 705` (0 hard / 0 soft).

## Seams

`PIT_CheckThing`, `PIT_CheckLine`, `PIT_CheckPortal`, iterators, and `P_GetFloorCeilingZ` are injected.

## Credit rationale

Position validation orchestrator; retires chunk3 recognition overlap.
