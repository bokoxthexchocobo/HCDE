# Chunk 1162 — Sight opening and check setup from `p_sight.cpp` (**1–209**).

## Scope

Sight opening and check setup from `p_sight.cpp` (**1–209**).

## Native ranges

- `src/playsim/p_sight.cpp` **Sight opening and check setup** 1–209 (209 lines)

**Credited native LOC:** 209 (tally `nativeLoc` 209).

## Managed

`VmPSightOpening1162Slice.cs`
Managed LOC (tally): 198.
Conversion commit: `9fc1feb65887c9a4445586239a99ca7289d8f21d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Opening flags and `SwapSides` (**57–77**) | yes |
| `SightCheck::init` (**126–145**) | yes |
| `P_SightOpening` (**156–199**) | yes |
| Band hull (**1–209**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **210–384** | `PTR_SightTraverse` is already credited |
| `PosRelative` | the displacement add stays the host offset |
| `PortalBlocksSight` and `ZatPoint` | sector queries stay host-supplied |

`SO_TOPFRONT` is **1**, `SO_TOPBACK` is **2**, `SO_BOTTOMFRONT` is **4**, and `SO_BOTTOMBACK` is **8**. `ML_PORTALCONNECT` is **0x80000000**. `FF_SEETHROUGH` is **0x2000000**. `LINEOPEN_MAX` is float max and `LINEOPEN_MIN` is its negation. The sight streams are named `BotCheckSight` and `CheckSight`. The intercept list reserves **128** and the portal list reserves **32**.

`SwapSides` moves the front bits one place left and the back bits one place right. Any other bit is dropped. Setup stores the viewer position plus the displacement from the viewer group to the task group, then adds three quarters of the viewer height to `Z`. The target contributes only `XY`, shifted from the target group to the same task group. A matching group does not ask the host and contributes **(0, 0)**. The trace runs from the raised eye to that target point. Both remembered `Z` values start at the raised eye. Slopes, fraction, direction, flags, and the target id are copied. No portal has been found, and the see-through mask is `FF_SEETHROUGH`.

A one-sided line with no portal connection leaves the opening at zero and the range at **0**. A portal that does not block a plane stores float max on the ceiling or float min on the floor and sets the front flag. A plane that stays **0** is then replaced from the other plane: the top reads the floor and the bottom reads the ceiling. The one-sided range is always **0**. A two-sided line applies the same portal substitution on both sides, using back flags for the back sector. An unset plane reads that side's own ceiling or floor. The bottom is the higher floor and the top is the lower ceiling. The range is **1** only when the bottom is strictly below the top, so a closed or equal window is **0**.

| Managed (`VmPSightOpening1162Slice.cs`) | Native (`p_sight.cpp`) |
|-----------------------------------------|------------------------|
| Opening and check setup | **1–209** |
| Band hull (exclusive slice) | **1–209** |

### Chunk **1162** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **209** |
| **Net headline** | **+209** vs post-**1161** |


**Post-review ratio:** 198/209 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmPSightOpening1162SliceTests.Begin_RaisesTheEyeAndShiftsOnlyAcrossGroups`
- `VmPSightOpening1162SliceTests.Swap_ExchangesFrontAndBackBits`
- `VmPSightOpening1162SliceTests.Open_OneSidedUsesTheSwappedPlanesAndAlwaysCloses`
- `VmPSightOpening1162SliceTests.Open_TwoSidedWindowIsTheOverlapNotTheDistance`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1162 --to 1162` (0 hard / 0 soft).

## Seams

Plane heights, portal sight blocks, and cross-group displacement stay host-supplied. This slice places the eye and classifies the opening.

## Credit rationale

UNCredited **`p_sight.cpp`** **1–209** (+209), a whole hole with no conversion or recognition overlap. `PTR_SightTraverse` starts at **210**.
