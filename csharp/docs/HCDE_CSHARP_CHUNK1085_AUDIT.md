# Chunk 1085 — `CommandGameMode` parse/tick, ammo/inventory flow control, and `CommandAspectRatio` parse head from `sbarinfo_commands.cpp` (**1801–1950**); follows **1084** ctor seam (**1800**).

## Scope

`CommandGameMode` parse/tick, ammo/inventory flow control, and `CommandAspectRatio` parse head from `sbarinfo_commands.cpp` (**1801–1950**); follows **1084** ctor seam (**1800**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandGameMode / CommandUsesAmmo / CommandAspectRatio** 1801–1950 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandGameMode1085Slice.cs`
Managed LOC (tally): 116.
Conversion commit: `34b2a11025e630ac84ebbe1e7f2fd1cdbcbec429`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| GameMode keyword OR loop (**1807–1821**) | yes |
| Unknown gamemode warn-once (**1813–1818**) | yes |
| GameMode tick truth table (**1828–1831**) | yes |
| `GameModes` flag values (**1837–1840**) | yes |
| `UsesAmmo` tick OR (**1866**) | yes |
| `UsesSecondaryAmmo` distinct slot (**1883**) | yes |
| `InventoryBarNotVisible` tics/bar flag (**1900**) | yes |
| Aspect ratio string dispatch (**1917–1928**) | yes |
| `ratioMap[ratio]=ratio` self map (**1933**) | yes |
| Aspect ratio tick equality (**1941**) | yes (predicate) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1799–1802** | **1084** ctor close seam |
| **1822**, **1864**, **1881**, **1898**, **1935**, **1939** | host flow-control delegate |
| **1846–1853** | static `modeNames` table (wiring via classifier) |
| **1951+** | `ratioMap` static + `FindRatio` outside hull |

| Managed (`VmSbarinfoCommandGameMode1085Slice.cs` + nested types) | Native (`sbarinfo_commands.cpp`) |
|---------------------------------------------------------------------|----------------------------------|
| GameMode parse/tick | **1804–1832** |
| Ammo / inventory flow | **1862–1900** |
| Aspect ratio parse/tick policy | **1914–1941** |
| Band hull (exclusive slice) | **1801–1950** |

### Chunk **1085** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1084** |


**Post-review ratio:** 116/150 managed/native ≈ 77% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandGameMode1085SliceTests.TryResolveGameModeKeyword_MapsTeamgameIndex`
- `VmSbarinfoCommandGameMode1085SliceTests.MergeGameModeKeywordIntoMask_StacksBitsVia1084Helper`
- `VmSbarinfoCommandGameMode1085SliceTests.ResolveUnknownGameModeWarningState_EmitsOnce`
- `VmSbarinfoCommandGameMode1085SliceTests.ResolveGameModeFlowControlTruth_MatchesCooperativeCase`
- `VmSbarinfoCommandGameMode1085SliceTests.ResolveUsesAmmoFlowControlTruth_OrPrimarySecondary`
- `VmSbarinfoCommandGameMode1085SliceTests.ResolveUsesSecondaryAmmoFlowControlTruth_RequiresDistinctSecondary`
- `VmSbarinfoCommandGameMode1085SliceTests.ResolveInventoryBarNotVisibleTruth_WhenTicsExpired`
- `VmSbarinfoCommandGameMode1085SliceTests.TryParseAspectRatioKeyword_Maps169`
- `VmSbarinfoCommandGameMode1085SliceTests.RegisterAspectRatioSelfMapping_WritesDiagonalEntry`
- `VmSbarinfoCommandGameMode1085SliceTests.ResolveAspectRatioFlowControlTruth_ComparesMappedRatio`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1085 --to 1085` (0 hard / 0 soft).

## Seams

**1084** owns `ApplyCommandGameModeFlagBit` and selected-inventory band; **1085** delegates keyword OR to **1084**. Host `MatchString`, `FindRatio`, and `SetTruth` excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **1801–1950** (+150); gamemode comma list + tick truth + usesammo/inventorybar ticks + aspect ratio token map head; hull ends at **ASPECTRATIO_5_4** enum entry (**1950**).
