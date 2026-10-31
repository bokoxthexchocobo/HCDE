# Chunk 1349 — Debugger register scope from RegistersScopeStateNode.cpp (1-283), through end of file.

## Scope

Debugger register scope from RegistersScopeStateNode.cpp (1-283), through end of file.

## Native ranges

- `src/common/scripting/dap/Nodes/RegistersScopeStateNode.cpp` **RegistersScopeStateNode** 1–283 (283 lines)

**Credited native LOC:** 283 (tally `nativeLoc` 283).

## Managed

`VmRegisters1349Slice.cs`
Managed LOC (tally): 364.
Conversion commit: `f9979165454274d9e64314145a260690d8c957db`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Register scope names and protocol scope (**37–100**) | yes |
| Register variable text and child indexes (**102–138**) | yes |
| Pointer, string, float, int, and parameter registers (**140–240**) | yes |
| Special setup registers (**242–283**) | yes |
| Band hull (**1–283**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| CreateNodeForVariable | the node built from a value and a type is supplied by the caller |
| GetVMValue | the special-setup read is the address plus the type, and the memory decode stays outside this file |

The scope name is Registers, the hint is registers, and indexed variables are forced to 0. Child names are appended, not replaced. SpecialInits is included only when the function is a script function. Lookup is case-insensitive and same-length. A miss leaves the output node unchanged. Each lookup builds a new node and stores the requested spelling. Parameter and special serialization write the fixed titles and the indexed count, and they leave a previous named count in place. The other groups write the requested name, the bracket count, and the named count, and they leave a previous indexed count in place. Child indexes are prefix plus the decimal index. The pointer group repeats that parse. std::stoi accepts a sign, leading C whitespace, and trailing junk, treats a leading zero as decimal, and throws on an empty suffix or a value outside int. A suffix that starts past the end of the name throws before the parse. A negative index or an index at the register count returns false and does not create a node. String values are the address of the string slot. Pointer, float, int, and parameter values are read from the live frame. Special setup uses the extra base plus the stored offset and the stored type. A native or missing function makes the special count throw.

| Managed (VmRegisters1349Slice.cs) | Native (RegistersScopeStateNode.cpp) |
|------------------------------------|---------------------------------------|
| Scope, register groups, and special setup | **1–283** |
| Band hull (exclusive slice) | **1–283** |

### Chunk **1349** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **283** |
| **Net headline** | **+283** vs the prior chunk |


**Post-review ratio:** 364/283 managed/native ≈ 129% (src file LOC at HEAD; see tally).

## Tests

- `VmRegisters1349SliceTests.ScopeNamesAndSerialize`
- `VmRegisters1349SliceTests.ChildIndexAndSpecialSetup`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1349 --to 1349` (0 hard / 0 soft).

## Seams

CreateNodeForVariable and GetVMValue stay injected. GetVMScriptFunction returns null for a missing or native function. The register arrays and the extra-stack base are the frame the scope reads.

## Credit rationale

UNCredited RegistersScopeStateNode.cpp 1-283 (+283). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines, ahead of StatePointerNode.cpp 1-281.
