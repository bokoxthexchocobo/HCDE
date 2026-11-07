# Chunk 1427 — CalcSectorSoundOrg through ValidatePosVel in s_doomsound.cpp (1015-1254).

## Scope

CalcSectorSoundOrg through ValidatePosVel in s_doomsound.cpp (1015-1254).

## Native ranges

- `src/sound/s_doomsound.cpp` **CalcSectorSoundOrg** 1015–1254 (240 lines)

**Credited native LOC:** 240 (tally `nativeLoc` 240).

## Managed

`VmDoomSound1427Slice.cs`
Managed LOC (tally): 230.
Conversion commit: `219984c3112aed0e8b4799bec44029688af25d4d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Sector origin (**1026–1065**) | yes |
| Polyobject origin (**1078–1089**) | yes |
| CalcPosVel (**1099–1204**) | yes |
| Position and velocity limits (**1212–1253**) | yes |
| Band hull (**1015–1254**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Sound saving | ends at line 1014 |
| Lump reads | start at line 1255 |
| Point, closest-point, and displacement bodies | callees |

Without the old sector-sound compat, a listener inside the sector uses the listener X and Y as the horizontal origin. Otherwise the closest boundary point is used. The compat path uses the sector center and does not need the primary level. A null sector or a null sector level crashes. Floor uses the lower of the floor and the listener Z. Ceiling uses the higher of the ceiling and the listener Z. Interior clamps the listener Z between them. Any other channel leaves Y alone. A polyobject origin is the closest wall point, and its height is clamped between that wall's floor and ceiling. A null poly, side, or sector crashes. A null output position skips the updater, including velocity. A camera copies its sound position and portal group. A missing camera zeroes the position and uses group 0. A velocity pointer is cleared. An unattached sound uses the point's X and Z minus the displacement, and its Y is the point Y unless the listener-Z flag selects the listener height. It does not fall through to the later listener-Z assignment. A null point, level, or hit sector crashes. An actor swaps map Y and Z into the sound vector and scales velocity by 35 with the same swap. A null actor leaves the camera position. A null actor sector crashes. A sector with the area flag uses the sector origin around the displaced listener, then removes that displacement from X and Z. Without the flag, the origin is the center minus the displacement and the local listener-Z flag is set. A polyobject uses its wall origin and then removes the displacement. After those sources, a listener-Z flag and a live camera replace Y with the listener height. The caller's flag word is not modified. Limits apply only to an actor source. A coordinate is valid from the negative limit through the positive limit. Position limit is 1048576 and velocity limit is 73400320. An invalid vector prints the red invalid-sound line with X, Z, and Y in that order. A null actor then prints a newline. A live actor prints its class and crashes when the class is missing. Both vectors are checked, and the result is the velocity check combined with the position check. Any other source type is valid without a print.

| Managed (`VmDoomSound1427Slice.cs`) | Native (`s_doomsound.cpp`) |
|-------------------------------------|---------------------------|
| Origins and limits | **1015–1254** |
| Band hull (exclusive slice) | **1015–1254** |

### Chunk **1427** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **240** |
| **Net headline** | **+240** vs the prior chunk |


**Post-review ratio:** 230/240 managed/native ≈ 96% (src file LOC at HEAD; see tally).

## Tests

- `VmDoomSound1427SliceTests.SectorAndPolyOrigins`
- `VmDoomSound1427SliceTests.PositionVelocityAndLimits`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1427 --to 1427` (0 hard / 0 soft).

## Seams

Point-in-sector, closest-point, and displacement calls stay callees. Sound saving ends at line 1014. Lump reads start at line 1255.

## Credit rationale

UNCredited s_doomsound.cpp 1015-1254 (+240). This continues the sound engine after serialization and stops before ReadSound. The band is shorter than 400 lines and is not sliced.
