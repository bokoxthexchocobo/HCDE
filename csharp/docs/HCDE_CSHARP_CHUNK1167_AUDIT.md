# Chunk 1167 — ACS module string locks and variable serialization from `p_acs.cpp` (**2102–2280**).

## Scope

ACS module string locks and variable serialization from `p_acs.cpp` (**2102–2280**).

## Native ranges

- `src/playsim/p_acs.cpp` **Module state** 2102–2280 (179 lines)

**Credited native LOC:** 179 (tally `nativeLoc` 179).

## Managed

`VmPAcsModuleState1167Slice.cs`
Managed LOC (tally): 294.
Conversion commit: `6b23db362fbd40a45dfaef2aeb40bd0744396b99`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `UnlockLevelVarStrings` (**2103–2106**) | yes |
| `MarkMapVarStrings` / `LockMapVarStrings` (**2108–2124**) | yes |
| `SerializeModuleStates` (**2126–2169**) | yes |
| `SerializeVars` / `SerializeVarSet` (**2171–2238**) | yes |
| `ParseLocalArrayChunk` (**2240–2258**) | yes |
| `FBehavior` constructor (**2260–2278**) | yes |
| Band hull (**2102–2280**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1711–2101** | inventory and module loading are chunk **1166** |
| **2281–2750** | `FBehavior::Init` is already conversion credit |
| `GlobalACSStrings` | the string table itself stays host-supplied |

Unlock forwards the level number to the string table. Mark walks the **128** map variables and then each array's element pointer and size. Lock does the same walk with the level number.

Module serialization opens `acsmodules`. A reader whose saved count differs from the live count stops with the have/save counts. Each open object writes the live name and data size, or reads the saved pair. A name that differs ignoring case stops with the live name and the saved name. A matching name with a different size stops with both byte counts. A closed object is skipped. A matching module then serializes its variables.

Variable serialization opens `variables`, writes the map store, then each array. A writer finds the first and last non-zero slots. An all-zero set stores count **0** and no values. A reader clears the destination, and when the saved count is non-zero it clamps `first + count` to the destination length before copying.

A local-array chunk reads the second 32-bit word, subtracts **2**, keeps the low **16** bits, and divides by **4**. Each following 32-bit size is little-endian. The offset advances by that size. Count **0** leaves the info pointer untouched and returns the incoming offset.

The behavior constructor zeros the script, function, and array counts, clears the pointers, sets the format to unknown (**3**), sets the lump to **-1**, zeros the map store, and empties the module name.

| Managed (`VmPAcsModuleState1167Slice.cs`) | Native (`p_acs.cpp`) |
|--------------------------------------------|----------------------|
| Module state | **2102–2280** |
| Band hull (exclusive slice) | **2102–2280** |

### Chunk **1167** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **179** |
| **Net headline** | **+179** vs post-**1166** |


**Post-review ratio:** 294/179 managed/native ≈ 164% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsModuleState1167SliceTests.Strings_MarkAndLockEveryMapVarThenEachArray`
- `VmPAcsModuleState1167SliceTests.Modules_StopWhenTheSaveNamesOrSizesDisagree`
- `VmPAcsModuleState1167SliceTests.VarSets_DropTheZeroEdgesAndClampALongRead`
- `VmPAcsModuleState1167SliceTests.ChunkAndConstruct_ReadLittleSizesAndResetTheModule`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1167 --to 1167` (0 hard / 0 soft).

## Seams

The global string table stays host-supplied. This slice chooses the arrays, the saved names and sizes, and the non-zero variable span.

## Credit rationale

UNCredited **`p_acs.cpp`** **2102–2280** (+179). Chunk **1166** ends at **2101**. `FBehavior::Init` credit starts at **2281**. A full tally line scan shows no overlap.
