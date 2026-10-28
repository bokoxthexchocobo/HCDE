# Chunk 784 — Force-field cleanup, floor clip, sprite/wall glow, sector actions, and overlap flag (`p_sectors.cpp` 1167–1362).

## Scope

Force-field cleanup, floor clip, sprite/wall glow, sector actions, and overlap flag (`p_sectors.cpp` 1167–1362).

## Native ranges

- `src/playsim/p_sectors.cpp` **RemoveForceField** 1167–1179 (13 lines)
- `src/playsim/p_sectors.cpp` **AdjustFloorClip** 1187–1198 (12 lines)
- `src/playsim/p_sectors.cpp` **sector_t::CheckSpriteGlow** 1206–1244 (39 lines)
- `src/playsim/p_sectors.cpp` **sector_t::GetWallGlow** 1251–1303 (53 lines)
- `src/playsim/p_sectors.cpp` **sector_t::TriggerSectorActions** 1312–1342 (31 lines)
- `src/playsim/p_sectors.cpp` **sector_t::CheckOverlap** 1352–1362 (11 lines)

**Credited native LOC:** 159 (tally `nativeLoc` 159).

## Managed

`VmPSectorGlowActions.cs`
Managed LOC (tally): 261.
Conversion commit: `2d6960c55e05ca999eb338fe6315eb1f6d7dd455`.

## Padding review (2026-10-21)

| Managed | Native (`p_sectors.cpp`) |
|---------|---------------------------|
| `RemoveForceField` | **1167–1179** |
| `AdjustFloorClip` | **1187–1198** |
| `CheckSpriteGlow` | **1206–1244** |
| `GetWallGlow` | **1251–1303** |
| `TriggerSectorActions` | **1312–1342** |
| `CheckOverlap` | **1352–1362** |

### Out-of-body uncredit (784)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **159** |
| **Out-of-body removed** | **0** |
| **Honest** | **159** |


**Post-review ratio:** 261/159 managed/native ≈ 164% (src file LOC at HEAD; see tally).

## Tests

- `VmPSectorGlowActionsTests.RemoveForceField_ClearsBlockingForceFieldLines`
- `VmPSectorGlowActionsTests.CheckSpriteGlow_BrightensNearFloorGlow`
- `VmPSectorGlowActionsTests.TriggerSectorActions_OrCombinesResults`
- `VmPSectorGlowActionsTests.CheckOverlap_SetsFlagWhenFloorAboveCeiling`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 784 --to 784` (0 hard / 0 soft).

## Seams

`TexMan` glow texture queries and `VMCall` sector-action triggers are injected delegates.

## Credit rationale

Whole-function ports for post-friction sector maintenance and renderer overlap signaling.
