# Chunk 1134 — AM_FakeFlat floor transfer and drawSubsectors skip gates from `am_map.cpp` (**1951–2100**); follows **1133**.

## Scope

AM_FakeFlat floor transfer and drawSubsectors skip gates from `am_map.cpp` (**1951–2100**); follows **1133**.

## Native ranges

- `src/am_map.cpp` **Automap fake flat and subsector skips** 1951–2100 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapFakeFlat1134Slice.cs`
Managed LOC (tally): 127.
Conversion commit: `9e2ca12b476d80410ae5b9dfb6cf2db14efc652e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Viewer Z and in-area (**1951–1971**) | yes |
| Clip-fake-planes and fake-floor-only return (**1973–2007**) | yes |
| Below and above floor texture/light (**2009–2053**) | yes |
| Polyorg, undrawn, cheat-4 hidden, portal skips (**2080–2102**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1949** | null height-sector return already credited by **1133** |
| **2104+** | point fill and later drawing |

Flag values are the native ones: `SECMF_FAKEFLOORONLY` **2**, `SECMF_CLIPFAKEPLANES` **4**, `SECMF_NOFAKELIGHT` **8**, `SECMF_HIDDEN` **256**, `SSECMF_DRAWN` **2**, `SSECF_POLYORG` **4**, `SSECF_HOLE` **8**. Area 1 keeps the control ceiling texture when the control floor is sky.

| Managed (`VmAmMapFakeFlat1134Slice.cs`) | Native (`am_map.cpp`) |
|------------------------------------------|---------------------|
| Fake flat and subsector skips | **1951–2100** |
| Band hull (exclusive slice) | **1951–2100** |

### Chunk **1134** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1133** |


**Post-review ratio:** 127/150 managed/native ≈ 85% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapFakeFlat1134SliceTests.ViewerZAndArea_PlayerViewAndBelowOrAboveTheControl`
- `VmAmMapFakeFlat1134SliceTests.Apply_FakeFloorOnlyBelowCopiesLightAndOtherwiseKeepsTheSector`
- `VmAmMapFakeFlat1134SliceTests.Apply_BelowRestoresTheTargetFloorAndAboveKeepsSkyCeiling`
- `VmAmMapFakeFlat1134SliceTests.SkipSubsector_HidesPolyOrgUndrawnAndForeignPortals`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1134 --to 1134` (0 hard / 0 soft).

## Seams

Plane copy validity and texture ids stay host-supplied. Subsector point fill begins at **2104**.

## Credit rationale

UNCredited **`am_map.cpp`** **1951–2100** (+150), inside the 3594-line file, with no conversion or recognition overlap.
