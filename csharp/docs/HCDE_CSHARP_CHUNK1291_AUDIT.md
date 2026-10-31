# Chunk 1291 — Player menu color, name, class, skin, and cvar setters from playermenu.cpp (1-246).

## Scope

Player menu color, name, class, skin, and cvar setters from playermenu.cpp (1-246).

## Native ranges

- `src/menu/playermenu.cpp` **DPlayerMenu::ColorChanged** 1–246 (246 lines)

**Credited native LOC:** 246 (tally `nativeLoc` 246).

## Managed

`VmPlayerMenu1291Slice.cs`
Managed LOC (tally): 165.
Conversion commit: `ce20161ddd4f07471e17afbed276cf98d2acaed2`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Color and name (**44–91**) | yes |
| Color set, class, and skin (**99–150**) | yes |
| Aim, team, and gender (**158–210**) | yes |
| Pickup and run (**218–246**) | yes |
| Band hull (**1–246**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Class display name | host string |
| Skin table | host name for the selected index |

Every setter returns 0. Nothing is written while the menu is closed. The color bits are the red, green, and blue channels shifted into one integer. The console color and color-set lines are limited to 23 characters. A name escapes quotes and backslashes. Class -1 stores Random. Team 0 stores 255, and any other team stores the value minus one. Gender values outside 0..3 do not write a cvar. A non-zero pickup or run value stores true.

| Managed (`VmPlayerMenu1291Slice.cs`) | Native (`playermenu.cpp`) |
|--------------------------------------|---------------------------|
| Menu setters | **1–246** |
| Band hull (exclusive slice) | **1–246** |

### Chunk **1291** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **246** |
| **Net headline** | **+246** vs post-**1290** |


**Post-review ratio:** 165/246 managed/native ≈ 67% (src file LOC at HEAD; see tally).

## Tests

- `VmPlayerMenu1291SliceTests.Menu_EscapesTheNameAndStoresTheTeam`
- `VmPlayerMenu1291SliceTests.Closed_LeavesTheCommandsUntouched`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1291 --to 1291` (0 hard / 0 soft).

## Seams

The class display name and the skin name are host inputs. A closed menu does not write.

## Credit rationale

UNCredited playermenu.cpp 1-246 (+246). A full tally line scan shows no overlap. The file ends at line 246.
