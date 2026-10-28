# Chunk 930 — `LS_Sector_SetRotation` through `LS_Line_SetTextureScale` (`p_lnspec.cpp` **2569–2718**). Follows **929** (**≤2568**); `LS_Line_SetBlocking` at **2721** deferred.

## Scope

`LS_Sector_SetRotation` through `LS_Line_SetTextureScale` (`p_lnspec.cpp` **2569–2718**). Follows **929** (**≤2568**); `LS_Line_SetBlocking` at **2721** deferred.

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Sector_SetRotation** 2569–2718 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPLnspecRotationAlign930Slice.cs`
Managed LOC (tally): 90.
Conversion commit: `299a5becd4cf362b2cd21673e95449614c676234`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_lnspec.cpp`) |
|---------|-------------------------|
| `SectorSetRotation` | `LS_Sector_SetRotation` **2568–2582** |
| `LineAlignFlatPlane` / `CombineLineAlignResults` | `LS_Line_AlignCeiling` / `LS_Line_AlignFloor` **2584–2610** |
| `LineTextureReject` / `TextureNoChange` | `LS_Line_SetTextureOffset` guards **2615–2620** |
| `TextureArgFrom65536` / `BuildTextureOffset` | offset set/add **2616–2664** |
| `TextureOffsetIsSetMode` / `TextureAffects*` | flag decode **2630–2660** |
| `ShouldApplyTextureAxis` | `NO_CHANGE` checks **2633–2659** |
| `BuildTextureScale` / `UsesMultiplyScaleMode` | `LS_Line_SetTextureScale` **2667–2718** |
| `CountAffectedTextureParts` | part mask **2635–2637**, **2689–2697** |
| `AlignCeilingFromSideArg` / `AlignFloorFromSideArg` | `!!arg1` **2593**, **2607** |
| `BuildLineAlignCeiling` / `BuildLineAlignFloor` | align invoke **2589–2608** |
| `ForEachAffectedTexturePart` / `TextureAxisApplyMask` | per-part loops **2635–2713** |

### Chunk **930** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**929** |


**Post-review ratio:** 90/150 managed/native ≈ 60% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecRotationAlign930SliceTests.SectorRotation_PacksFloorAndCeilingAngles`
- `VmPLnspecRotationAlign930SliceTests.LineAlign_CeilingUsesTopPlane`
- `VmPLnspecRotationAlign930SliceTests.TextureOffset_RejectsInvalidSide`
- `VmPLnspecRotationAlign930SliceTests.TextureScale_Uses65536Factors`
- `VmPLnspecRotationAlign930SliceTests.BuildTextureOffset_DetectsAddMode`
- `VmPLnspecRotationAlign930SliceTests.CombineLineAlignResults_OrAccumulates`
- `VmPLnspecRotationAlign930SliceTests.CountAffectedTextureParts_CountsFlagBits`
- `VmPLnspecRotationAlign930SliceTests.TextureAxisApplyMask_SkipsNoChangeRaw`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 930 --to 930` (0 hard / 0 soft).

## Seams

Sector/line iterators, `DAngle::fromDeg`, `AlignFlat`, and `side_t` texture mutation remain host seams.

## Credit rationale

Honest **150**-line slice in **2269–3129** corridor; exclusive `VmPLnspecRotationAlign930Slice.cs`.
