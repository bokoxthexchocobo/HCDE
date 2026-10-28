# Chunk 1073 — `CommandDrawImage` draw-scale and parse keyword head from `sbarinfo_commands.cpp` (**1–150**). Primary `sbarinfo.cpp` **1641–1790** blocked (file EOF **1640**); pivot to included commands TU.

## Scope

`CommandDrawImage` draw-scale and parse keyword head from `sbarinfo_commands.cpp` (**1–150**). Primary `sbarinfo.cpp` **1641–1790** blocked (file EOF **1640**); pivot to included commands TU.

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawImage::Draw / Parse** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawImage1073Slice.cs`
Managed LOC (tally): 115.
Conversion commit: `96717c4357a8662d402159ba32a78bf61f72db35`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `DI_ALTERNATEONFAIL` pre-draw gate (**44–45**) | yes |
| Null texture early out (**47–48**) | yes |
| Frame alpha product (**53**) | yes |
| `DI_DRAWINBOX` scale policy (**55–79**) | yes |
| Spawn scale dimension path (**81–85**) | yes |
| Draw-image keyword dispatch (**96–107**) | yes |
| Hexen armor piece names (**113–125**) | yes |
| `translatable` + inventory icon branch (**129–147**) | yes |
| `getImage` string token gate (**149–150**) | yes (predicate) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–30** | license banner / include comment |
| **34–40** | ctor initializer list host |
| **45**, **86–87** | host `Draw` / `DrawGraphic` |
| **93–94**, **112**, **126–127**, **137–145**, **151** | host scanner / class lookup |
| **1641+** | `sbarinfo.cpp` out of body |

| Managed (`VmSbarinfoCommandDrawImage1073Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|------------------------------------------------------|----------------------------------|
| Alternate-on-fail + null texture gates | **44–48** |
| Draw-in-box + spawn scale sizing | **53–85** |
| Keyword + hexen armor classification | **96–125** |
| Translatable / inventory parse policy | **129–147** |
| Image token requirement | **149–150** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **1073** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1072** |


**Post-review ratio:** 115/150 managed/native ≈ 77% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawImage1073SliceTests.ShouldInvokeParentDrawBeforeTexture_DetectsAlternateOnFailFlag`
- `VmSbarinfoCommandDrawImage1073SliceTests.ComputeDrawInBoxDimensions_ForceScalePicksHeightLimiter`
- `VmSbarinfoCommandDrawImage1073SliceTests.ComputeDrawInBoxDimensions_WithoutForceScaleUsesMinScale`
- `VmSbarinfoCommandDrawImage1073SliceTests.ClassifyDrawImageKeyword_MapsNativeKeywords`
- `VmSbarinfoCommandDrawImage1073SliceTests.ClassifyDrawImageKeyword_TranslatableSetsFlags`
- `VmSbarinfoCommandDrawImage1073SliceTests.ResolveHexenArmorPiece_DefaultsUnknownToArmor`
- `VmSbarinfoCommandDrawImage1073SliceTests.ComputeFrameAlpha_MultipliesBlockAndCommandAlpha`
- `VmSbarinfoCommandDrawImage1073SliceTests.ResolveSpawnScaledDimensions_AppliesSpawnScales`
- `VmSbarinfoCommandDrawImage1073SliceTests.ShouldCloseHexenArmorParen_RequiresParenAndImageToken`
- `VmSbarinfoCommandDrawImage1073SliceTests.ShouldUseDrawInBoxPath_DetectsDrawInBoxFlag`
- `VmSbarinfoCommandDrawImage1073SliceTests.ResolveInventoryImageIndex_UsesNativeSentinel`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1073 --to 1073` (0 hard / 0 soft).

## Seams

`SBarInfoCommandFlowControl::Draw`, `DrawGraphic`, `FScanner`, `PClass`, and **149–150** `MustGetToken` remain host; **1501–1640** `sbarinfo.cpp` tail still deferred (140, no pad).

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **1–150** (+150); zero conversion overlap; no Vm* line collision; honest continuation after **1072** / deferred sbarinfo tail.
