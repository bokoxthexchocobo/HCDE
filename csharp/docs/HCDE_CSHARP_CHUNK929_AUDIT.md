# Chunk 929 — `LS_Sector_SetColor` through `LS_Sector_SetCeilingScale2` (`p_lnspec.cpp` **2419–2568**). Follows **928** (**≤2418**); `LS_Sector_SetRotation` at **2568+** deferred.

## Scope

`LS_Sector_SetColor` through `LS_Sector_SetCeilingScale2` (`p_lnspec.cpp` **2419–2568**). Follows **928** (**≤2418**); `LS_Sector_SetRotation` at **2568+** deferred.

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Sector_SetColor** 2419–2568 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPLnspecScrollSector929Slice.cs`
Managed LOC (tally): 77.
Conversion commit: `b3094610a465cc1103c837c98ac005d4494069c4`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_lnspec.cpp`) |
|---------|-------------------------|
| `SectorSetColor` | `LS_Sector_SetColor` **2421–2432** |
| `SectorSetFade` | `LS_Sector_SetFade` **2434–2444** |
| `CeilingPanning` / `PanningOffset` | `LS_Sector_SetCeilingPanning` **2446–2460** |
| `FloorPanning` / `BuildFloorPanning` | `LS_Sector_SetFloorPanning` **2462–2476** |
| `BuildCeilingPanning` | `LS_Sector_SetCeilingPanning` invoke seam **2452–2458** |
| `InvertedScalePair` | scale prep **2493–2496**, **2516–2519** |
| `SectorSetFloorScale` / `InvertScaleComponent` | `LS_Sector_SetFloorScale` **2478–2499** |
| `SectorSetCeilingScale` | `LS_Sector_SetCeilingScale` **2501–2522** |
| `SectorSetFloorScale2` / `InvertScaleFactor65536` | `LS_Sector_SetFloorScale2` **2524–2544** |
| `SectorSetCeilingScale2` | `LS_Sector_SetCeilingScale2` **2546–2566** |
| `ShouldApplyScale2X` / `ShouldApplyScale2Y` | **2538–2541**, **2560–2563** |
| Gravity close seam | **2419** |

### Chunk **929** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**928** |


**Post-review ratio:** 77/150 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecScrollSector929SliceTests.Panning_CombinesIntAndFrac`
- `VmPLnspecScrollSector929SliceTests.FloorScale_InvertsNonZero`
- `VmPLnspecScrollSector929SliceTests.Scale2_Uses65536Factors`
- `VmPLnspecScrollSector929SliceTests.CeilingScale_MatchesFloorInvert`
- `VmPLnspecScrollSector929SliceTests.SectorSetColor_CarriesDesaturate`
- `VmPLnspecScrollSector929SliceTests.ShouldApplyScale2_SkipsZeroFactor`
- `VmPLnspecScrollSector929SliceTests.BuildFloorPanning_MarksFloorPlane`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 929 --to 929` (0 hard / 0 soft).

## Seams

Sector tag iterators, `PalEntry`, and `sector_t::SetColor`/`SetFade`/offset/scale application remain host seams.

## Credit rationale

Honest **150**-line slice in **2269–3129** corridor; exclusive `VmPLnspecScrollSector929Slice.cs`.
