# Chunk 1375 — Debugger cvar variable text and the cvar scope child list from CVarScopeStateNode.cpp (1-217).

## Scope

Debugger cvar variable text and the cvar scope child list from CVarScopeStateNode.cpp (1-217).

## Native ranges

- `src/common/scripting/dap/Nodes/CVarScopeStateNode.cpp` **CVarScopeStateNode** 1–217 (217 lines)

**Credited native LOC:** 217 (tally `nativeLoc` 217).

## Managed

`VmCVarScope1375Slice.cs`
Managed LOC (tally): 186.
Conversion commit: `ea16c75ec4fea1f4af59c829e67960bef1157f40`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| File header and includes (**1–30**) | yes |
| `CVarStateNode` constructor and `ToVariable` (**31–145**) | yes |
| `SerializeToProtocol` for the variable and the scope (**147–169**) | yes |
| `GetChildNames` (**171–190**) | yes |
| `GetChildNode` (**192–216**) | yes |
| Band hull (**1–217**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| `CVarScopeStateNode.h` | comparer and field declarations; not this cpp hole |
| `c_cvars.cpp` / `stringtable.h` | cvar reads and `localize`; callees |

A null cvar becomes name and value `<INVALID>`. `GetRealType` selects `CVar<Bool>` through `CVar<Dummy>`, and any other type is `<UNKNOWN>`. A float human string gains `.0` only when it has neither `.` nor a lowercase `e`. A string cvar is wrapped in quotes only when the favorite type is also string. A non-empty description is localized. The parenthetical is used when that text is non-empty and its tail after the first character differs from the description. The variable name is stored last. The scope is named `CVars`, expensive, hint `cvars`, with `variablesReference` from `GetId`, indexed variables 0, and named variables equal to the cached name count. An empty cache walks the cvar map, stores a child per cvar, and sorts names so a leading lowercase character comes before a leading uppercase character. Otherwise the order is ordinal. A later lookup that misses the caseless child map calls `FindCVar`, inserts `GetName()`, and sorts again.

| Managed (`VmCVarScope1375Slice.cs`) | Native (`CVarScopeStateNode.cpp`) |
|-------------------------------------|------------------------------------|
| Cvar text and scope children | **1–217** |
| Band hull (exclusive slice) | **1–217** |

### Chunk **1375** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **217** |
| **Net headline** | **+217** vs the prior chunk |


**Post-review ratio:** 186/217 managed/native ≈ 86% (src file LOC at HEAD; see tally).

## Tests

- `VmCVarScope1375SliceTests.ToVariableFormatsTypeValueAndDescription`
- `VmCVarScope1375SliceTests.ScopeListsChildrenAndFindsLateCvars`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1375 --to 1375` (0 hard / 0 soft).

## Seams

GetFavoriteRep, GetHumanString, GetRealType, GetDescription, GetName, GStrings.localize, FindCVar, and the cvar map live outside this file and are injected. CVarNameComparer is declared in the header and applied by the sorts in this band.

## Credit rationale

UNCredited CVarScopeStateNode.cpp 1-217 (+217). A full tally line scan, including recognition ranges, ties this hole with d_net.cpp 3685-3901. Both are 217 lines. Path order takes the scripting file. The file ends at 217. Gaps longer than 400 lines stay whole and are not sliced.
