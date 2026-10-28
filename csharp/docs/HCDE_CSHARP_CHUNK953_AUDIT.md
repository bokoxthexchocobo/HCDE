# Chunk 953 — `DLevelScript::RunScript` HUD message color/type tail and ST/network PCDs from `p_acs.cpp` (**9195–9344**). `p_acs` EOF tail **11250–11388** (139 lines) deferred sub-150 without padding.

## Scope

`DLevelScript::RunScript` HUD message color/type tail and ST/network PCDs from `p_acs.cpp` (**9195–9344**). `p_acs` EOF tail **11250–11388** (139 lines) deferred sub-150 without padding.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::RunScript** 9195–9344 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsVmOpcode953Slice.cs`
Managed LOC (tally): 99.
Conversion commit: `d9c87190daf9b1441dca63690cad6a2ff4bfd005`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| HUD color clamp / `HUDMSG_COLORSTRING` (**9192–9195**) | yes |
| HUD subtype switch normal/fade/type-on/fade-in-out (**9197–9233**) | yes |
| HUD flags: wrap, alpha, blend, visibility, layer, log (**9240–9258**) | yes |
| `PCD_SETFONT` / `PCD_SETFONTDIRECT` stack (**9266–9274**) | seam (host font) |
| `PCD_PLAYERCOUNT` / `PCD_GAMETYPE` / `PCD_GAMESKILL` (**9276–9293**) | yes |
| ST PCDs: network, team, health, armor, frags (**9296–9331**) | yes |
| `PCD_MUSICCHANGE` / `PCD_SINGLEPLAYER` (**9333–9344**) | yes |

| Managed (`VmPAcsVmOpcode953Slice.cs`) | Native (`p_acs.cpp`) |
|----------------------------------------|---------------------|
| `ClampHudColorIndex` / `HudMessageUsesColorString` | **9187–9195** |
| `HudMessageDisplaySubtype` / fade compat helpers | **9197–9233** |
| `HudMessageLayer` / `HudMessageVisibility` / flag tests | **9240–9255** |
| `HudMessageConsoleColor` / `EndHudMessageStackPointer` | **9255–9263** |
| `PushGameType` / player push helpers | **9276–9331** |
| `ShouldChangeMusic` / `PushSinglePlayerFlag` | **9333–9344** |

### Chunk **953** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**952** |

### Deferred (no chunk row)

| Band | Lines | Reason |
|------|------:|--------|
| `p_acs.cpp` **11250–11388** | **139** | EOF; sub-150 floor |


**Post-review ratio:** 99/150 managed/native ≈ 66% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsVmOpcode953SliceTests.ClampHudColorIndex_ReturnsUntranslatedWhenOutOfRange`
- `VmPAcsVmOpcode953SliceTests.FadeOutCompatDuration_AppliesMinFrameWhenBothZero`
- `VmPAcsVmOpcode953SliceTests.HudMessageAttachId_OrMasksNonZeroId`
- `VmPAcsVmOpcode953SliceTests.PushGameType_SelectsTitleDeathmatchCoop`
- `VmPAcsVmOpcode953SliceTests.PushActivatorHealth_ZeroWithoutActivator`
- `VmPAcsVmOpcode953SliceTests.ShouldChangeMusic_RequiresLookup`
- `VmPAcsVmOpcode953SliceTests.HudMessageLayer_ExtractsShiftedBits`
- `VmPAcsVmOpcode953SliceTests.EndHudMessageStackPointer_SubtractsSixOperands`
- `VmPAcsVmOpcode953SliceTests.PushSinglePlayerFlag_InvertsMultiplayer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 953 --to 953` (0 hard / 0 soft).

## Seams

`Create<DHUDMessage*>`, `StatusBar->AttachMessage`, `S_ChangeMusic`, `V_FindFontColor`, and full `AcsVm` dispatch remain host seams.

## Credit rationale

Honest **150**-line UNCredited band with zero tally overlap; exclusive `VmPAcsVmOpcode953Slice.cs` (pivot from deferred `ShowProfileData` EOF tail).
