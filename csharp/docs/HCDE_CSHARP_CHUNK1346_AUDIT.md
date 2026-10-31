# Chunk 1346 — Debugger value formatting from ValueStateNode.cpp (1-289), through end of file.

## Scope

Debugger value formatting from ValueStateNode.cpp (1-289), through end of file.

## Native ranges

- `src/common/scripting/dap/Nodes/ValueStateNode.cpp` **ValueStateNode** 1–289 (289 lines)

**Credited native LOC:** 289 (tally `nativeLoc` 289).

## Managed

`VmValueState1346Slice.cs`
Managed LOC (tally): 465.
Conversion commit: `0c4208f2c991288ddf0a21cd3178fb6ad1a94c33`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Type name table (**36–63**) | yes |
| Constructor (**65–68**) | yes |
| String and pointer (**77–140**) | yes |
| Integer and float (**142–154**) | yes |
| Bool through color (**162–216**) | yes |
| State label (**217–256**) | yes |
| Enum and error (**257–279**) | yes |
| Serialize (**283–288**) | yes |
| Band hull (**1–289**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| GetBasicType | callee; the table in this file is what the index selects |
| TexMan and soundEngine | callees; injected |
| GetStateFromIdx | callee; injected |
| Lines after 289 | the header-only declaration stays with the class |

A zero integer still prints. Unsigned 32-bit minus one prints the full unsigned width. A 16-bit and an 8-bit value keep only the low bits, and a signed 8-bit value sign-extends. Float prints six digits after the float cast. Negative zero stays negative. A missing string is empty and does not read characters. A null character pointer prints two quotes. A class pointer ignores the address. An invalid function pointer prints NULL. A function with an owning class prints that class even when a variant exists. A function with only an implementation prints that name. A function with neither leaves the value empty. A pointer whose low 32 bits are zero is null and does not dereference. A scalar pointer prints the address and the inner value. A non-scalar pointer prints only the address. Any other type prints its descriptive name and the error token. Bool uses the word true for every non-zero integer. Name, sprite, texture, translation, and sound use their labels. A missing texture or a sound index outside the open range is invalid. Color prints eight uppercase hex digits and the four channels. A positive state index uses the name and the owner, or unknownOwner when the lookup has none. A negative index uses the owned label. A negative index the owner does not own stays unknown. An enum-shaped type and any other int-not-int type both finish as Enum plus the decimal. Serialize copies the formatted type and value, always returns true, and writes the eval name only when it differs from the name.

| Managed (VmValueState1346Slice.cs) | Native (ValueStateNode.cpp) |
|-------------------------------------|------------------------------|
| Value formatting and serialize | **1–289** |
| Band hull (exclusive slice) | **1–289** |

### Chunk **1346** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **289** |
| **Net headline** | **+289** vs the prior chunk |


**Post-review ratio:** 465/289 managed/native ≈ 161% (src file LOC at HEAD; see tally).

## Tests

- `VmValueState1346SliceTests.ScalarsPointersAndFloats`
- `VmValueState1346SliceTests.NamesColorsAndStates`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1346 --to 1346` (0 hard / 0 soft).

## Seams

The basic-type table names the value before the string, pointer, integer, float, and int-not-int branches replace it. A value is valid only when the pointer bits and the low 32 bits are both non-zero. Strings quote their characters or print empty. Class pointers print the pointed descriptive name. Function pointers append the owner or the implementation and a glibc pointer. Scalar pointers recurse. State labels use a positive name index, or a negative index that the owner still owns. Assignment of the protocol name happens only when the eval name differs. The enum branch always finishes as Enum and a decimal.

## Credit rationale

UNCredited ValueStateNode.cpp 1-289 (+289). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines, tied with i_steam.cpp 1-289. Path order takes the value node.
