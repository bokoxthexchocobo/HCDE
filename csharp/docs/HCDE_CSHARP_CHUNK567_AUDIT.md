# Chunk 567 — P_MarkGlobalVarStrings, P_CollectACSGlobalStrings, ScriptPresentation, P_ClearACSVars, DoUseInv/UseInventory/HCDE player resolve, CheckInventory (1444–1558, 1723–1866)

## Scope

P_MarkGlobalVarStrings, P_CollectACSGlobalStrings, ScriptPresentation, P_ClearACSVars, DoUseInv/UseInventory/HCDE player resolve, CheckInventory (1444–1558, 1723–1866)

## Native ranges

- `src/playsim/p_acs.cpp` **P_MarkGlobalVarStrings** 1444–1451 (8 lines)
- `src/playsim/p_acs.cpp` **P_CollectACSGlobalStrings** 1461–1487 (27 lines)
- `src/playsim/p_acs.cpp` **ScriptPresentation** 1508–1523 (16 lines)
- `src/playsim/p_acs.cpp` **P_ClearACSVars** 1531–1558 (28 lines)
- `src/playsim/p_acs.cpp` **DoUseInv** 1723–1748 (26 lines)
- `src/playsim/p_acs.cpp` **UseInventory** 1758–1785 (28 lines)
- `src/playsim/p_acs.cpp` **HCDE_ResolveACSPlayer** 1801–1810 (10 lines)
- `src/playsim/p_acs.cpp` **CheckInventory** 1821–1866 (46 lines)

**Credited native LOC:** 189 (tally `nativeLoc` 189).

## Managed

`VmAcsMarkGlobalVarStrings.cs`, `VmAcsCollectGlobalStrings.cs`, `VmAcsScriptPresentation.cs`, `VmAcsClearACSVars.cs`, `VmAcsInventoryNative.cs`
Managed LOC (tally): 213.
Conversion commit: `1607de8310f80dc98853ded9a61d3c8e18020b2c`.

## Padding review (2026-10-21)

| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `VmAcsMarkGlobalVarStrings` | `P_MarkGlobalVarStrings` 1444–1451 |
| `VmAcsCollectGlobalStrings` | `P_CollectACSGlobalStrings` 1461–1487 |
| `VmAcsScriptPresentation` | `ScriptPresentation` 1508–1523 |
| `VmAcsClearACSVars` | `P_ClearACSVars` 1531–1558 |
| `VmAcsInventoryNative` | `DoUseInv` 1723–1748; `UseInventory` 1758–1785; `HCDE_*ACSPlayer*` 1787–1810; `CheckInventory` 1821–1866 |

**Post-review ratio:** 213/189 managed/native ≈ 113% (src file LOC at HEAD; see tally).

## Tests

- `VmAcsGlobalStringsInventoryTests.CollectGlobalStrings_SkipsZeroStackPointer`
- `VmAcsGlobalStringsInventoryTests.PresentNamedScript_WhenNegativeIdResolves`
- `VmAcsGlobalStringsInventoryTests.ClearACSVars_PurgesGlobalsWhenAlsoGlobal`
- `VmAcsGlobalStringsInventoryTests.DoUseInvNative_BypassesFrozenForPlayers`
- `VmAcsGlobalStringsInventoryTests.CheckInventoryNative_HealthMaxUsesActivator`
- `VmAcsGlobalStringsInventoryTests.ResolveAcsPlayerNative_RejectsReservedSlots`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 567 --to 567` (0 hard / 0 soft).

## Seams

`FACSStack` traversal, `PClass::FindActor`, `FindInventory`, and serializer `WriteVars` are not hosted.

## Credit rationale

Whole-function ports for uncredited ACS global-string GC and inventory helpers in `p_acs.cpp`.
