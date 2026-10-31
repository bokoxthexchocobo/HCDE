# Chunk 1166 — Inventory use, plane watcher, and ACS module loading from `p_acs.cpp` (**1711–2101**).

## Scope

Inventory use, plane watcher, and ACS module loading from `p_acs.cpp` (**1711–2101**).

## Native ranges

- `src/playsim/p_acs.cpp` **Inventory and modules** 1711–2101 (391 lines)

**Credited native LOC:** 391 (tally `nativeLoc` 391).

## Managed

`VmPAcsInventory1166Slice.cs`
Managed LOC (tally): 490.
Conversion commit: `86fe0d46f47964add48cb5bc5249b9d7078d117e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `DoUseInv` (**1723–1748**) | yes |
| `UseInventory` (**1758–1785**) | yes |
| Player slot helpers (**1787–1810**) | yes |
| `CheckInventory` (**1821–1866**) | yes |
| `DPlaneWatcher` construct, serialize, and tick (**1899–1971**) | yes |
| Module load, unload, and lookup (**1977–2053**) | yes |
| Level var string mark and lock (**2055–2101**) | yes |
| Band hull (**1711–2101**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1688–1710** | `P_ReadACSVars` / `P_WriteACSVars` are already conversion credit |
| **2108–2124** | `MarkMapVarStrings` and `LockMapVarStrings` start after this band |
| **2281–2750** | `FBehavior::Init` is already conversion credit |
| `FindInventory` / `PClass::FindActor` | class and item lookup stay host-supplied |
| `I_ToInternalClientSlot` / `I_IsServerReservedSlot` | slot mapping stays host-supplied |

`DoUseInv` returns false when the actor has no matching item. With no player it returns `UseInventory`. With a player it clears `CF_TOTALLYFROZEN` (**1 << 12**) for the call, then ORs that saved bit back. Other cheat bits changed during the call stay as the call left them.

`UseInventory` returns **0** for a null type or an unknown class. A null activator adds one for each in-game player, across `MAXPLAYERS` (**64**), whose item use succeeds. An activator returns **1** or **0**.

A negative player number is returned unchanged and does not call the slot host. `Resolve` stores the internal slot even when it then rejects the number. Rejection is a negative slot, a slot at or above **64**, a reserved slot, or a player who is not in the game.

`CheckInventory` returns **0** for a null activator or type. `Health` returns max health or current health and does not look up a class. `Armor` is looked up as `BasicArmor`. An unknown class and a non-inventory class log and return **0**. Max amount prefers the held item, otherwise the class default. Otherwise the held amount is returned, or **0**.

A plane watcher stores the special, activator, line, nonzero side, ceiling flag, and five args. Last distance is the chosen plane `D`. Watch distance is `D - height * normal.Z`. Serialize stores special, sector, ceiling, both distances, activator, line, side, and args. A missing sector destroys the watcher. A tick fires when the stored last distance is below the watch and the current distance has reached it, or the stored last distance is above the watch and the current distance has fallen to it. The stored last distance is not replaced. The watcher is destroyed after the special runs.

`LoadModule` returns null when the lump is **-1** and there is no reader. An existing lump is reused. A failed `Init` drops the module and records an invalid-module message. `LoadDefaultModules` scans each `LOADACS` string, loads a library lump at or above **0**, and records a missing library otherwise. `CheckAllGood` is true only when every module is good. Unload destroys from the end and clears the list. `GetModule` treats a negative index as out of range.

Mark and lock walk every module, then the server script list, then the client script list. A null thinker is skipped.

| Managed (`VmPAcsInventory1166Slice.cs`) | Native (`p_acs.cpp`) |
|------------------------------------------|----------------------|
| Inventory and modules | **1711–2101** |
| Band hull (exclusive slice) | **1711–2101** |

### Chunk **1166** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **391** |
| **Net headline** | **+391** vs post-**1165** |


**Post-review ratio:** 490/391 managed/native ≈ 125% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsInventory1166SliceTests.UseInv_ClearsTotallyFrozenOnlyWhileTheItemIsUsed`
- `VmPAcsInventory1166SliceTests.CheckInventory_RenamesArmorAndHealthBeforeClassLookup`
- `VmPAcsInventory1166SliceTests.Players_KeepNegativeNumbersAndRejectReservedSlots`
- `VmPAcsInventory1166SliceTests.WatcherAndModules_CrossThePlaneAndReuseALoadedLump`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1166 --to 1166` (0 hard / 0 soft).

## Seams

Class lookup, client-slot mapping, reserved slots, and module `Init` stay host-supplied. `MarkMapVarStrings` and `LockMapVarStrings` begin at **2108**, so this slice only walks the modules and scripts.

## Credit rationale

UNCredited **`p_acs.cpp`** **1711–2101** (+391). Conversion credit ends at **1710** and resumes at **2281**. A full tally line scan shows no overlap. Outside the **9091–9240** denylist.
