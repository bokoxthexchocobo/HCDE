# Chunk 567 — P_MarkGlobalVarStrings, P_CollectACSGlobalStrings, ScriptPresentation, P_ClearACSVars, DoUseInv/UseInventory/HCDE player resolve, CheckInventory (1444–1558, 1723–1866) Merged: [568]. Retired (2026-10-24): `--band` hull overlaps existing credit (p-saveg-acs-archive-chunk10 src/playsim/p_acs.cpp 1566-1680).

## Scope

P_MarkGlobalVarStrings, P_CollectACSGlobalStrings, ScriptPresentation, P_ClearACSVars, DoUseInv/UseInventory/HCDE player resolve, CheckInventory (1444–1558, 1723–1866) Merged: [568]. Retired (2026-10-24): `--band` hull overlaps existing credit (p-saveg-acs-archive-chunk10 src/playsim/p_acs.cpp 1566-1680).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `00eb26ee2930901b1e9aa81e204e6822d2fd1c07`.

## Padding review (2026-10-21)

| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `VmAcsMarkGlobalVarStrings` | `P_MarkGlobalVarStrings` 1444–1451 |
| `VmAcsCollectGlobalStrings` | `P_CollectACSGlobalStrings` 1461–1487 |
| `VmAcsScriptPresentation` | `ScriptPresentation` 1508–1523 |
| `VmAcsClearACSVars` | `P_ClearACSVars` 1531–1558 |
| `VmAcsInventoryNative` | `DoUseInv` 1723–1748; `UseInventory` 1758–1785; `HCDE_*ACSPlayer*` 1787–1810; `CheckInventory` 1821–1866 |

<!-- merged from chunk568 -->
| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `VmAcsBehaviorContainer.LoadDefaultModulesNative` | `LoadDefaultModules` 1977–1998 |
| `LoadModuleNative` | `LoadModule` 2000–2023 |
| `CheckAllGoodNative` | `CheckAllGood` 2025–2035 |
| `GetModuleIndexNative` | `GetModule` 2046–2053 |
| `MarkLevelVarStringsNative` / `LockLevelVarStringsNative` / `UnlockLevelVarStringsNative` | `MarkLevelVarStrings` 2055–2077; `LockLevelVarStrings` 2079–2101; `UnlockLevelVarStrings` 2103–2106 |
| `MarkMapVarStringsNative` / `LockMapVarStringsNative` | `FBehavior::MarkMapVarStrings` 2108–2115; `LockMapVarStrings` 2117–2124 |

### Out-of-body uncredit (567)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **189** |
| **Out-of-body removed** | **0** |
| **Honest** | **189** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 567 --to 567` (0 hard / 0 soft).

## Seams

`FACSStack` traversal, `PClass::FindActor`, `FindInventory`, and serializer `WriteVars` are not hosted.

## Credit rationale

Whole-function ports for uncredited ACS global-string GC and inventory helpers in `p_acs.cpp`.
