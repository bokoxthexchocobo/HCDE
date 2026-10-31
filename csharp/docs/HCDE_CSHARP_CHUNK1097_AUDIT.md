# Chunk 1097 — `NextCommand` switch tail, unknown-command error, and flow-block close from `sbarinfo_commands.cpp` (**3601–3614**); EOF closeout after **1096** dispatch head (**3600**).

## Scope

`NextCommand` switch tail, unknown-command error, and flow-block close from `sbarinfo_commands.cpp` (**3601–3614**); EOF closeout after **1096** dispatch head (**3600**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **SBarInfoCommandFlowControl::NextCommand EOF tail** 3601–3614 (14 lines)

**Credited native LOC:** 14 (tally `nativeLoc` 14).

## Managed

`VmSbarinfoCommandFlowControlNextCommand1097Slice.cs`
Managed LOC (tally): 52.
Conversion commit: `825de530cd6e3c8a867eb64226da1596b1659c4e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Switch cases alpha→ifcvarint (**3601–3605**) | yes |
| Unknown command error + null return (**3608–3609**) | yes (policy) |
| Absent identifier `MustGetToken('}')` (**3612–3613**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **3545–3600** | **1096** command table + dispatch head |
| **3574–3576** | **1096** identifier gate |
| **3606–3607** | switch close brace only |
| **3610–3611** | host `return NULL` after error |
| **3614** | function closing brace |

| Managed (`VmSbarinfoCommandFlowControlNextCommand1097Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|----------------------------------------------------------------|----------------------------------|
| Tail dispatch band + factory mapping | **3601–3605** |
| Unknown identifier outcome | **3608–3609** |
| Flow block terminator | **3612–3613** |
| Band hull (exclusive slice) | **3601–3614** |

### Chunk **1097** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **14** |
| **Net headline** | **+14** vs post-**1096** |


**Post-review ratio:** 52/14 managed/native ≈ 371% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandFlowControlNextCommand1097SliceTests.IsNextCommandDispatchCaseWithinEofBand_IncludesTailCases`
- `VmSbarinfoCommandFlowControlNextCommand1097SliceTests.IsNextCommandDispatchCaseWithinEofBand_ExcludesInInventoryHead`
- `VmSbarinfoCommandFlowControlNextCommand1097SliceTests.TryResolveNextCommandTailFactoryKind_MapsIfCVarInt`
- `VmSbarinfoCommandFlowControlNextCommand1097SliceTests.ResolveNextCommandUnknownIdentifierOutcome_FailsWhenSwitchMisses`
- `VmSbarinfoCommandFlowControlNextCommand1097SliceTests.ResolveNextCommandWhenIdentifierAbsent_RequiresClosingBrace`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1097 --to 1097` (0 hard / 0 soft).

## Seams

**1096** owns `NextCommand` through **SBARINFO_ININVENTORY** (**3579–3600**); **1097** credits alpha/if* factory cases (**3601–3605**), switch fallthrough error (**3608–3609**), and closing-brace terminator (**3612–3613**). Host `new Command*` factories and `sc.ScriptError` formatting excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **3601–3614** (14); intentional short EOF band closing `NextCommand` after **1096** +150 hull.
