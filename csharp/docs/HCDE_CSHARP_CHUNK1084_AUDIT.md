# Chunk 1084 — `CommandDrawSelectedInventory` draw/parse/tick and `CommandGameMode` ctor head from `sbarinfo_commands.cpp` (**1651–1800**); follows **1083** mugshot parse **`;` (**1650**).

## Scope

`CommandDrawSelectedInventory` draw/parse/tick and `CommandGameMode` ctor head from `sbarinfo_commands.cpp` (**1651–1800**); follows **1083** mugshot parse **`;` (**1650**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawSelectedInventory / CommandGameMode** 1651–1800 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawSelectedInventory1084Slice.cs`
Managed LOC (tally): 130.
Conversion commit: `1ae0b070d98459d8a18ef8b3fa3498f5ed7861af`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Selected inventory ctor defaults + `INT_MAX` length (**1669–1674**) | yes |
| Draw visibility gate (**1681**) | yes |
| Artiflash branch + image index (**1683–1686**) | yes |
| Itemflash alpha + cursor path (**1690–1694**) | yes |
| Counter draw when amount ≠ 1 (**1698–1699**) | yes |
| Parse flag keywords + font loop exit (**1713–1740**) | yes |
| `center` / `centerbottom` offsets (**1721–1724**) | yes |
| Default counter coords (**1756–1758**) | yes |
| `alternateOnEmpty` parse tail vs `;` (**1772–1775**) | yes |
| Flow-control truth on tick (**1781**) | yes |
| **CommandGameMode** ctor `modes=0` (**1799–1800**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1651–1662** | empty **Reset** + mugshot member layout |
| **1678–1679**, **1696–1699** | host flow/image/number draw delegates |
| **1743–1748**, **1761–1768** | host font lookup / translations |
| **1787–1791** | selected-inventory member fields |
| **1801+** | **CommandGameMode** parse outside hull |

| Managed (`VmSbarinfoCommandDrawSelectedInventory1084Slice.cs`, `VmSbarinfoCommandGameMode1084Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|------------------------------------------------------------------------------------------------|----------------------------------|
| Draw + parse + tick policy | **1676–1785** |
| GameMode ctor default | **1799–1800** |
| Band hull (exclusive slice) | **1651–1800** |

### Chunk **1084** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1083** |


**Post-review ratio:** 130/150 managed/native ≈ 87% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawSelectedInventory1084SliceTests.CreateSelectedInventoryCtorDefaults_MatchesNativeInit`
- `VmSbarinfoCommandDrawSelectedInventory1084SliceTests.ShouldDrawSelectedInventoryBar_RequiresSelectionAndBarAllowed`
- `VmSbarinfoCommandDrawSelectedInventory1084SliceTests.ComputeArtiflashImageIndex_MatchesNativeMacro`
- `VmSbarinfoCommandDrawSelectedInventory1084SliceTests.ShouldApplyItemFlashCursor_RequiresNonZeroFade`
- `VmSbarinfoCommandDrawSelectedInventory1084SliceTests.ShouldDrawSelectedInventoryAmountCounter_HidesSingletonUnlessForced`
- `VmSbarinfoCommandDrawSelectedInventory1084SliceTests.ResolveSelectedInventoryImageOffset_MapsCenterBottomBits`
- `VmSbarinfoCommandDrawSelectedInventory1084SliceTests.ShouldTreatSelectedInventoryTokenAsFont_WhenUnknownIdentifier`
- `VmSbarinfoCommandDrawSelectedInventory1084SliceTests.ResolveSelectedInventoryDefaultCounterPosition_Applies3030And24`
- `VmSbarinfoCommandDrawSelectedInventory1084SliceTests.ResolveSelectedInventoryParseTail_SelectsFlowControlOrSemicolon`
- `VmSbarinfoCommandDrawSelectedInventory1084SliceTests.ResolveSelectedInventoryFlowControlTruth_WhenMissingSelection`
- `VmSbarinfoCommandDrawSelectedInventory1084SliceTests.ApplyCommandGameModeFlagBit_StacksFromCtorZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1084 --to 1084` (0 hard / 0 soft).

## Seams

**1083** owns **CommandDrawMugShot** parse through **1650**; **1082** owns selected-inventory tick amount optional update. Host `DrawGraphic`, `V_GetFont`, `GetCoordinates`, and `SetTruth` wiring excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **1651–1800** (+150); selected-inventory bar/flash/counter policy + parse flags + flow-control truth; hull ends at **CommandGameMode** ctor init (**1800**).
