# Chunk 1404 — Default menu colors and the menu console commands from doommenu.cpp (1636-1786).

## Scope

Default menu colors and the menu console commands from doommenu.cpp (1636-1786).

## Native ranges

- `src/menu/doommenu.cpp` **SetDefaultMenuColors** 1636–1786 (151 lines)

**Credited native LOC:** 151 (tally `nativeLoc` 151).

## Managed

`VmDoomMenu1404Slice.cs`
Managed LOC (tally): 129.
Conversion commit: `6c498491d3851cde8c94e85547ed85d302a70bc9`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| SetDefaultMenuColors (**1637–1659**) | yes |
| menu_main through menu_video (**1661–1786**) | yes |
| Band hull (**1636–1786**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| V_FindFontColor | callee |
| PClass::FindClass and CreateNew | callees |
| M_StartControlPanel and M_SetMenu | callees; the arguments are the result |
| cl_gfxlocalization and OkForLocalization | already outside this band |

SetDefaultMenuColors copies the seven gameinfo color names before it looks up a class. A missing or non-menu help class stops before the delegate lookup. A missing delegate or a class that is not a MenuDelegateBase stops before CreateNew. menu_main changes full-console state to the menu screen and then opens MainMenu. Every other command leaves the state alone. Each command starts the control panel with true and opens its named menu at -1.

| Managed (`VmDoomMenu1404Slice.cs`) | Native (`doommenu.cpp`) |
|-------------------------------------|-------------------------|
| Menu colors and the console commands | **1636–1786** |
| Band hull (exclusive slice) | **1636–1786** |

### Chunk **1404** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **151** |
| **Net headline** | **+151** vs the prior chunk |


**Post-review ratio:** 129/151 managed/native ≈ 85% (src file LOC at HEAD; see tally).

## Tests

- `VmDoomMenu1404SliceTests.DefaultColorsStopBeforeTheDelegateWhenTheHelpClassFails`
- `VmDoomMenu1404SliceTests.MenuCommandsOpenNamedMenusAndMainLeavesTheFullConsole`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1404 --to 1404` (0 hard / 0 soft).

## Seams

Font-color lookup, class lookup, and menu creation stay callees. The file ends at line 1786.

## Credit rationale

UNCredited doommenu.cpp 1636-1786 (+151). After chunk 1403 the larger codegen.cpp 3778-3933 gap is the tail of FxMulDiv::Emit and the head of FxCompareRel::Resolve, both already credited on either side, so the whole functions left there are under 150 lines and stay uncredited. This band is the next whole-function hole. It is shorter than 400 lines and is not sliced. It runs through end of file.
