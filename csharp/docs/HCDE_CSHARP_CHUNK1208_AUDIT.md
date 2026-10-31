# Chunk 1208 — Native function and field binary search, VM function lookup, and the sorted import tables from imports.cpp (1–226).

## Scope

Native function and field binary search, VM function lookup, and the sorted import tables from imports.cpp (1–226).

## Native ranges

- `src/common/scripting/core/imports.cpp` **FindFunction** 1–226 (226 lines)

**Credited native LOC:** 226 (tally `nativeLoc` 226).

## Managed

`VmImports1208Slice.cs`
Managed LOC (tally): 309.
Conversion commit: `3b22eb96fa739b861029986221e1689c93833e13`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| CompareClassNames (**43–59**) | yes |
| FindFunction (**67–90**) | yes |
| FindField (**98–122**) | yes |
| FindVMFunction class (**131–135**) | yes |
| FindVMFunction qualified (**145–153**) | yes |
| funccmp and fieldcmp (**162–174**) | yes |
| InitImports (**182–226**) | yes |
| Band hull (**1–226**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

Compare skips one leading character of the stored class name, then compares without case. FindFunction requires a class. FindField treats a null class as an empty name. Both binary-search the sorted table, class first and member second. A qualified VM name splits on the first dot. A name with no dot, an unknown class, or an empty class returns null. FindSymbol walks the parent chain only when the class has a VM type, ignores symbols that are not functions, and reads variant 0. Init builds the native Font struct and its pointer handlers, then clears and sorts the action functions and fields. Each action installs a native function whose qualified name drops the class prefix and whose printable name adds ` [Native]`. A null function slot throws. The size check after Clear always runs the fill.

| Managed (`VmImports1208Slice.cs`) | Native (`imports.cpp`) |
|-----------------------------------|------------------------|
| Imports | **1–226** |
| Band hull (exclusive slice) | **1–226** |

### Chunk **1208** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **226** |
| **Net headline** | **+226** vs post-**1207** |


**Post-review ratio:** 309/226 managed/native ≈ 137% (src file LOC at HEAD; see tally).

## Tests

- `VmImports1208SliceTests.Init_SortsPrefixStrippedNamesAndInstallsNativeFunctions`
- `VmImports1208SliceTests.FindVm_WalksParentsAndSplitsTheQualifiedName`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1208 --to 1208` (0 hard / 0 soft).

## Seams

Stored class names keep a one-character prefix that FName lookup omits. Symbol and class names fold ASCII case. Font size and alignment are the host sizeof and alignof of FFont. The action and field segments are the pending lists passed to Init. A null VMPointer is the native assert. qsort ties are unspecified.

## Credit rationale

UNCredited imports.cpp 1–226 (+226). The file ends at 226. A full tally line scan shows no overlap.
