# Chunk 1350 — Debugger state pointer from StatePointerNode.cpp (1-281), through end of file.

## Scope

Debugger state pointer from StatePointerNode.cpp (1-281), through end of file.

## Native ranges

- `src/common/scripting/dap/Nodes/StatePointerNode.cpp` **StatePointerNode** 1–281 (281 lines)

**Credited native LOC:** 281 (tally `nativeLoc` 281).

## Managed

`VmStatePointer1350Slice.cs`
Managed LOC (tally): 391.
Conversion commit: `7765a56da00afa0a83224905e8d0de71d395ce75`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| State pointer constructor and label dump (**35–60**) | yes |
| Protocol value, owner label, and child names (**62–120**) | yes |
| Next, sprite, timing, light, and state-flag text (**122–197**) | yes |
| Frame, use flags, define flags, and the remaining fields (**198–281**) | yes |
| Band hull (**1–281**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| StaticFindStateOwner | the actor class that owns a state is supplied by the caller |
| StaticGetStateName | the fallback state text is supplied by the caller |
| ValueStateNode serializer | this file only chooses the name, the bits, and the type token |
| Header child map | the map is never read or written by this file |

A null address or a pointer whose low 32 bits are 0 serializes as <NULL> with variables reference 0. A valid pointer uses GetId, type StatePointer, and twelve appended child names. Indexed variables and a matching evaluate name stay as they were. The first label whose state pointer matches wins. A missing owner, a miss, or the name None uses the injected state text. Otherwise the value is type name, a dot, and the label. An unknown child name returns true and leaves the output node unchanged. Each lookup builds a new node and stores the canonical spelling. NextState carries the same owning type. UseFlags stores DefineFlags as uint8; the native use-flag join is discarded. State-flag text is the unsigned count plus the known STF bits joined by a bar, and an empty join leaves empty parentheses. DefineFlags is the number, a space, and one SDF name, or <INVALID>. Dump prints DehExtra when the owner is missing and the dehacked index is non-negative, invalid when that index is negative, and the injected name when an owner exists. A null state still walks children. The child prefix is the parent prefix, a dot, and the label text.

| Managed (VmStatePointer1350Slice.cs) | Native (StatePointerNode.cpp) |
|--------------------------------------|--------------------------------|
| State pointer, label dump, and children | **1–281** |
| Band hull (exclusive slice) | **1–281** |

### Chunk **1350** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **281** |
| **Net headline** | **+281** vs the prior chunk |


**Post-review ratio:** 391/281 managed/native ≈ 139% (src file LOC at HEAD; see tally).

## Tests

- `VmStatePointer1350SliceTests.ProtocolAndLookup`
- `VmStatePointer1350SliceTests.ChildrenDumpAndFlags`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1350 --to 1350` (0 hard / 0 soft).

## Seams

StaticFindStateOwner and StaticGetStateName stay injected. Value and dummy children record the constructor arguments; their protocol serializers live outside this file. The header child map is never read here.

## Credit rationale

UNCredited StatePointerNode.cpp 1-281 (+281). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines, ahead of d_net_diag.cpp 813-1075.
