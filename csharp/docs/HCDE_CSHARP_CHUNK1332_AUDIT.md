# Chunk 1332 — CVar bool, int, float, and string conversion, the static format buffer, hex-byte decode, cvar_set, cvar_forceset, and the callback gate from c_cvars.cpp (321-676).

## Scope

CVar bool, int, float, and string conversion, the static format buffer, hex-byte decode, cvar_set, cvar_forceset, and the callback gate from c_cvars.cpp (321-676).

## Native ranges

- `src/common/console/c_cvars.cpp` **FBaseCVar::ToBool** 321–676 (356 lines)

**Credited native LOC:** 356 (tally `nativeLoc` 356).

## Managed

`VmCVarConvert1332Slice.cs`
Managed LOC (tally): 642.
Conversion commit: `8d0613ca3da19a418852e85a334619846699c39f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Closing brace of SetGenericRep (**321–321**) | yes |
| ToBool (**322–347**) | yes |
| ToInt (**349–379**) | yes |
| ToFloat (**381–401**) | yes |
| Static buffer and ToString (**403–434**) | yes |
| FromBool (**436–464**) | yes |
| FromInt (**466–495**) | yes |
| FromFloat (**497–528**) | yes |
| HexToByte (**530–550**) | yes |
| FromString (**552–590**) | yes |
| cvar_set and cvar_forceset (**592–618**) | yes |
| EnableNoSet through DisableCallbacks (**620–671**) | yes |
| Boolean-cvar comment (**672–676**) | yes |
| Band hull (**321–676**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| SetGenericRep body | chunk 162 through line 320; cvar_set only records the call |
| ForceSet | callee of cvar_forceset |
| FindCVar | callee; empty and null names miss, other names match ignoring case |
| FBoolCVar | line 677, chunk 162 |

ToBool and ToInt treat color like int. A string compares true and false without case, then uses base-0 strtoll. Leading spaces do not match the words, so "  true" is false. Octal 010 is 8. 0xFFFFFFFF narrows to -1. An overflowing magnitude clamps, then narrows: the positive clamp becomes -1 and the negative clamp becomes 0. ToFloat does not special-case the words, so "true" is 0. strtod accepts decimal, hex floats, inf, and nan. FromBool, FromInt, FromFloat, and FromString do not have a color case, so color becomes int 0. ToString of bool returns the static true or false and leaves the buffer alone. String returns the pointer and leaves the buffer alone. Int, color, and float write the 39-character buffer. %g is six significant digits, with nan, inf, and signed zero spelled the C way. The same buffer is what FromInt and FromFloat return. HexToByte shifts two digits. A letter adds its offset from A or a and does not add 10, so A0 is 0 and ff is 85. cvar_set and cvar_forceset return null when the name is missing and otherwise record one callee write. EnableNoSet latches the static gate on. EnableCallbacks turns the callback gate on and walks registration order, skipping CVAR_NOINITCALL. A var with a user callback counts one fire. InitZSCallbacks instantiates CVAR_ZS_CUSTOM entries and registers MarkZSCallbacks every call. MarkZSCallbacks marks only those custom entries. DisableCallbacks turns the gate off and leaves the no-set latch set.

| Managed (`VmCVarConvert1332Slice.cs`) | Native (`c_cvars.cpp`) |
|---------------------------------------|------------------------|
| Value conversion, string parse, and the callback gate | **321–676** |
| Band hull (exclusive slice) | **321–676** |

### Chunk **1332** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **356** |
| **Net headline** | **+356** vs post-**1331** |


**Post-review ratio:** 642/356 managed/native ≈ 180% (src file LOC at HEAD; see tally).

## Tests

- `VmCVarConvert1332SliceTests.Convert_ParsesBoolIntFloatAndString`
- `VmCVarConvert1332SliceTests.Gate_SetsNamedCvarsAndWalksCallbacks`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1332 --to 1332` (0 hard / 0 soft).

## Seams

FindCVar, SetGenericRep, and ForceSet stay callees. cvar_set records the string passed to SetGenericRep. cvar_forceset records the string passed to ForceSet. EnableCallbacks calls the virtual Callback. InitZSCallbacks calls InstantiateZSCVar and registers MarkZSCallbacks.

## Credit rationale

UNCredited c_cvars.cpp 321-676 (+356). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines, ahead of d_dehacked.cpp 1907-2252 (346) and whole-file s_sound_eternity.cpp 1-331. Line 321 is the closing brace of SetGenericRep, already credited through 320. FBoolCVar at 677 stays chunk 162.
