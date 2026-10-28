# Chunk 955 — `SetDynamicLights` tail, attached-light teardown, `FindUserLight`, `AttachLightDef`/`AttachLightDirect`, and `RemoveLight` head from `a_dynlight.cpp` (**751–900**). Pivot from dry `p_acs` queue.

## Scope

`SetDynamicLights` tail, attached-light teardown, `FindUserLight`, `AttachLightDef`/`AttachLightDirect`, and `RemoveLight` head from `a_dynlight.cpp` (**751–900**). Pivot from dry `p_acs` queue.

## Native ranges

- `src/playsim/a_dynlight.cpp` **AActor::SetDynamicLights** 751–900 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmADynLightAttach955Slice.cs`
Managed LOC (tally): 76.
Conversion commit: `cf53534058d7c350ff7e5fabad98975961f1a46e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `SetDynamicLights` state-light tail + deactivate extras (**751–759**) | yes |
| `DeleteAttachedLights` (**768–776**) | yes (seam: host unlink) |
| `FindUserLight` (**785–797**) | yes |
| `AttachLightDef` (**805–825**) | yes |
| `A_AttachLightDef` export (**827–833**) | seam (VM export) |
| `AttachLightDirect` (**841–871**) | yes |
| `A_AttachLight` export (**873–891**) | seam (VM export) |
| `RemoveLight` head (**899–900**) | yes |

| Managed (`VmADynLightAttach955Slice.cs`) | Native (`a_dynlight.cpp`) |
|------------------------------------------|-------------------------|
| `ShouldAttachStateLightEntry` / `ShouldFallbackToStateLights` / `ShouldDeactivateExtraLight` | **751–759** |
| `LightAssociationMatches` | **738–742** |
| `DeleteAttachedLights` host seam | **768–776** |
| `FindUserLightIndex` | **785–797** |
| `AbortLightAttachWhenEuthanizing` / `ShouldCopyLightDefaults` / `UserLightSlotAfterCreate` | **805–825** |
| `A_AttachLightDef` VM export | **827–833** |
| `ExtractColor*` / `AttachLightOffsetComponents` / `AttachLightTypeParameter` / `SpotPitchInRange` | **841–871** |
| `ApplyRecreateLights*` | **820–821**, **868–869** |
| `A_AttachLight` VM export | **873–891** |
| `RemoveLightIndexValid` | **899–900** |
| Band hull (exclusive slice) | **751–900** |

### Chunk **955** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**954** |


**Post-review ratio:** 76/150 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmADynLightAttach955SliceTests.LightAssociationMatches_AnyFrameWildcard`
- `VmADynLightAttach955SliceTests.ShouldFallbackToStateLights_WhenNoAttachmentsYet`
- `VmADynLightAttach955SliceTests.FindUserLightIndex_NotFoundWithoutCreate`
- `VmADynLightAttach955SliceTests.AttachLightTypeParameter_ScalesPulseWithTicrate`
- `VmADynLightAttach955SliceTests.SpotPitchInRange_AcceptsPlusMinusNinety`
- `VmADynLightAttach955SliceTests.ExtractColorComponents_SplitsRgb`
- `VmADynLightAttach955SliceTests.AttachLightOffsetComponents_SwapsYIntoMiddleSlot`
- `VmADynLightAttach955SliceTests.AbortLightAttachWhenEuthanizing_WhenFlagSet`
- `VmADynLightAttach955SliceTests.RemoveLightIndexValid_BeforeCount`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 955 --to 955` (0 hard / 0 soft).

## Seams

`AttachLight`, `GetLight`, `LightDefaults`, `DEFINE_ACTION_FUNCTION_NATIVE`, and `delete`/`UserLights` storage remain host seams.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmADynLightAttach955Slice.cs` (dynamic light tick/render from chunk **823** is not matrix credit).
