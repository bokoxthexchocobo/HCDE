# Chunk 781 — Dynamic 3D-floor cleanup, attached recalc/light hooks, `P_LineOpening_XFloors`, and plane/floor queries (`p_3dfloors.cpp` 544–854; follows chunk699 `P_Recalculate3DFloors` 291–535). Retired (2026-10-23): out-of-body credit removed; honest **144** < **150**.

## Scope

Dynamic 3D-floor cleanup, attached recalc/light hooks, `P_LineOpening_XFloors`, and plane/floor queries (`p_3dfloors.cpp` 544–854; follows chunk699 `P_Recalculate3DFloors` 291–535). Retired (2026-10-23): out-of-body credit removed; honest **144** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `75660c4916061488bd25f6e0737623544febad62`.

## Padding review (2026-10-21)

**Trim (2026-10-22):** removed summary XML and `FFloor` colormap/blend wrappers (inlined in `RecalculateLights`); dropped `Vector3`/`ThingActor.Top` shims.

| Managed | Native (`p_3dfloors.cpp`) |
|---------|-------------------------|
| `ClearDynamic3DFloorData` | `FLevelLocals::ClearDynamic3DFloorData` **544–569** |
| `RecalculateAttached3DFloors` | `P_RecalculateAttached3DFloors` **577–586** |
| `RecalculateLights` | `P_RecalculateLights` **594–612** |
| `RecalculateAttachedLights` | `P_RecalculateAttachedLights` **620–629** |
| `GetPlaneLight` | `P_GetPlaneLight` **637–650** |
| `LineOpeningXFloors` | `P_LineOpening_XFloors` **658–773** |
| `FindFloorPlane` | `P_FindFloorPlane` **782–800** |
| `Find3DFloor` | `P_Find3DFloor` **809–854** |

### Out-of-body uncredit (781)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **260** |
| **Out-of-body removed** | **116** |
| **Honest** | **144** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 781 --to 781` (0 hard / 0 soft).

## Seams

`P_Recalculate3DFloors` is injected for attached-sector refresh; `F3DFloor` deletion and terrain/colormap lookups remain host callbacks.

## Credit rationale

Faithful whole-function ports for post-recalc maintenance and movement line-opening bands adjacent to chunk699 `P_Recalculate3DFloors`.
