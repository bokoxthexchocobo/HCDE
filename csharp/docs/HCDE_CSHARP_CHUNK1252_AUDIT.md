# Chunk 1252 — Plane rejection and the 16-float dynamic light record from hw_dynlightdata.cpp (1-152), through end of file.

## Scope

Plane rejection and the 16-float dynamic light record from hw_dynlightdata.cpp (1-152), through end of file.

## Native ranges

- `src/rendering/hwrenderer/hw_dynlightdata.cpp` **GetLight** 1–152 (152 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Managed

`VmDynLight1252Slice.cs`
Managed LOC (tally): 171.
Conversion commit: `21adaec306810fae6785d264d4db2993f9dbd503`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Light cvars (**35–36**) | yes |
| GetLight (**45–60**) | yes |
| AddLightToList (**67–152**) | yes |
| Band hull (**1–152**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Light class methods | other files |
| Shadow map object | host enabled flag |

A radius of zero or less is rejected. A distance greater than the radius is rejected. The side test uses the signed distance and runs only when requested. World Y and Z are swapped into the plane. Additive lights use scale 0.2 and list 2. Subtractive lights replace that list with 1 and fold each channel around the color length. A target with the alpha-multiply flag scales the color. The shadow index is one past the light index when the shadow map is on, otherwise 1025, and attenuation stores it as a negative value. A spot writes the cosines of the inner and outer angles and a direction from yaw and negated pitch. The record is 16 floats, with world X, Z, Y, then radius.

| Managed (`VmDynLight1252Slice.cs`) | Native (`hw_dynlightdata.cpp`) |
|------------------------------------|--------------------------------|
| Rejection and the light record | **1–152** |
| Band hull (exclusive slice) | **1–152** |

### Chunk **1252** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **152** |
| **Net headline** | **+152** vs post-**1251** |


**Post-review ratio:** 171/152 managed/native ≈ 112% (src file LOC at HEAD; see tally).

## Tests

- `VmDynLight1252SliceTests.Get_RejectsADarkSideAndKeepsALightOnThePlane`
- `VmDynLight1252SliceTests.Add_FoldsSubtractiveColorAndASpotDirection`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1252 --to 1252` (0 hard / 0 soft).

## Seams

Portal-relative position is a host offset added when the group is not zero. Degree sine and cosine are the CRT functions. The sprite and particle light switches are stored and are not read by these two functions.

## Credit rationale

UNCredited hw_dynlightdata.cpp 1-152 (+152). A full tally line scan shows no overlap. The file ends at line 152.
