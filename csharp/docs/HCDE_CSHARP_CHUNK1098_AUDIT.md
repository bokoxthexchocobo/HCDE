# Chunk 1098 — HUD art index constants, `SBarInfoCoordinate` packing, `SBarInfoCommand::Offset` flags, and `GetCoordinates` loop prologue from `sbarinfo.cpp` (**1–150**); seam **150+** owned by chunk **1063**.

## Scope

HUD art index constants, `SBarInfoCoordinate` packing, `SBarInfoCommand::Offset` flags, and `GetCoordinates` loop prologue from `sbarinfo.cpp` (**1–150**); seam **150+** owned by chunk **1063**.

## Native ranges

- `src/g_statusbar/sbarinfo.cpp` **SBarInfo file head / coordinates prologue** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoFileHead1098Slice.cs`
Managed LOC (tally): 94.
Conversion commit: `b06f454e1df59b5bf31521112574acb317d1183d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Status-bar art index enum (**46–53**) | yes |
| `SBarInfoCoordinate` relCenter/value packing (**71–96**) | yes |
| `SBarInfoCommand::Offset` bitmask (**112–124**) | yes |
| `GetCoordinates` loop comma gate (**142–147**) | yes |
| Loop per-axis reset defaults (**144–145**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–45** | includes / forward decls |
| **55–56** | `EXTERN_CVAR` host |
| **126–135** | virtual draw/parse stubs |
| **150–163** | chunk **1063** signed coordinate parse |
| **151+** | remainder credited on **1063** / later bands |

| Managed (`VmSbarinfoFileHead1098Slice.cs`) | Native (`sbarinfo.cpp`) |
|---------------------------------------------|-------------------------|
| Art indices + offset anchors | **46–53**, **112–124** |
| Coordinate pack/unpack/add | **71–96** |
| GetCoordinates loop prologue | **137–149** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **1098** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1097** |


**Post-review ratio:** 94/150 managed/native ≈ 63% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoFileHead1098SliceTests.PackCoordinate_StoresRelativeCenterInSignBit`
- `VmSbarinfoFileHead1098SliceTests.AddCoordinate_PreservesRelativeCenterFlag`
- `VmSbarinfoFileHead1098SliceTests.DrawOffsetAnchor_MatchesNativeBitmask`
- `VmSbarinfoFileHead1098SliceTests.StatusBarArtFlashOffset_MatchesArtiflashMacro`
- `VmSbarinfoFileHead1098SliceTests.IsValidStatusBarArtIndex_AcceptsNativeRange`
- `VmSbarinfoFileHead1098SliceTests.CombineOffsetAnchors_MergesVerticalAndHorizontal`
- `VmSbarinfoFileHead1098SliceTests.GetCoordinatesLoopRequiresComma_OnlyAfterFirstAxis`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1098 --to 1098` (0 hard / 0 soft).

## Seams

Includes/host lines (**1–45**, **55–56**, **126–135**) excluded; band ends at **149** so **1063** retains signed-int + `center` parse (**150–163**). `FScanner` token reads remain host-injected.

## Credit rationale

UNCredited **`sbarinfo.cpp`** **1–150** (+150); first eligible ≥150 band after **1097** EOF on `sbarinfo_commands.cpp`.
