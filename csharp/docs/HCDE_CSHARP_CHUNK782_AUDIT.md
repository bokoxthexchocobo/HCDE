# Chunk 782 — Ceiling low point, colormap/fog, closest point on lines, plane motion/light, portal validate, and special transfer (`p_sectors.cpp` 717–961; follows chunk779 surround band ending 711).

## Scope

Ceiling low point, colormap/fog, closest point on lines, plane motion/light, portal validate, and special transfer (`p_sectors.cpp` 717–961; follows chunk779 surround band ending 711).

## Native ranges

- `src/playsim/p_sectors.cpp` **FindLowestCeilingPoint** 717–751 (35 lines)
- `src/playsim/p_sectors.cpp` **SetColor** 760–765 (6 lines)
- `src/playsim/p_sectors.cpp` **SetFade** 773–777 (5 lines)
- `src/playsim/p_sectors.cpp` **sector_t::SetFogDensity** 789–792 (4 lines)
- `src/playsim/p_sectors.cpp` **sector_t::ClosestPoint** 803–854 (52 lines)
- `src/playsim/p_sectors.cpp` **PlaneMoving** 862–868 (7 lines)
- `src/playsim/p_sectors.cpp` **GetFloorLight** 875–885 (11 lines)
- `src/playsim/p_sectors.cpp` **GetCeilingLight** 892–902 (11 lines)
- `src/playsim/p_sectors.cpp` **sector_t::ValidatePortal** 909–916 (8 lines)
- `src/playsim/p_sectors.cpp` **GetSpecial** 923–931 (9 lines)
- `src/playsim/p_sectors.cpp` **SetSpecial** 938–946 (9 lines)
- `src/playsim/p_sectors.cpp` **TransferSpecial** 953–961 (9 lines)

**Credited native LOC:** 166 (tally `nativeLoc` 166).

## Managed

`VmPSectorPointColorSpecial.cs`
Managed LOC (tally): 255.
Conversion commit: `3f824699d9ad39eb8b12ce4bd107c43d3e658764`.

## Padding review (2026-10-21)

| Managed | Native (`p_sectors.cpp`) |
|---------|---------------------------|
| `FindLowestCeilingPoint` | **717–751** |
| `SetColor` | **760–765** |
| `SetFade` | **773–777** |
| `SetFogDensity` | `sector_t::SetFogDensity` **789–792** |
| `ClosestPoint` | `sector_t::ClosestPoint` **803–854** |
| `PlaneMoving` | **862–868** |
| `GetFloorLight` / `GetCeilingLight` | **875–885**, **892–902** |
| `ValidatePortal` | `sector_t::ValidatePortal` **909–916** |
| `GetSpecial` / `SetSpecial` / `TransferSpecial` | **923–931**, **938–946**, **953–961** |

### Out-of-body uncredit (782)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **166** |
| **Out-of-body removed** | **0** |
| **Honest** | **166** |


**Post-review ratio:** 255/166 managed/native ≈ 154% (src file LOC at HEAD; see tally).

## Tests

- `VmPSectorPointColorSpecialTests.FindLowestCeilingPoint_FlatCeilingUsesFd`
- `VmPSectorPointColorSpecialTests.SetColor_WritesColormapAndInvokesRecalc`
- `VmPSectorPointColorSpecialTests.ClosestPoint_PicksNearestOnSegment`
- `VmPSectorPointColorSpecialTests.PlaneMoving_DetectsFloorData`
- `VmPSectorPointColorSpecialTests.GetFloorLight_UsesAbsoluteWhenFlagged`
- `VmPSectorPointColorSpecialTests.ValidatePortal_RejectsMissingSkybox`
- `VmPSectorPointColorSpecialTests.TransferSpecial_CopiesDamageFields`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 782 --to 782` (0 hard / 0 soft).

## Seams

`P_RecalculateAttachedLights`, `TexMan`/`skyflatnum`, and portal type constants are injected; `sector_t` plane slots are modeled as `PlaneSlot[]`.

## Credit rationale

Faithful whole-function ports for the post-surround sector utility cluster; replaces rejected sub-150 `a_specialspot.cpp` top-up.
