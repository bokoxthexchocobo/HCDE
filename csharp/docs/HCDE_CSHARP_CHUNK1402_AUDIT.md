# Chunk 1402 — Fake frame, protocol stack frame, and scope children from StackFrameStateNode.cpp (1-161).

## Scope

Fake frame, protocol stack frame, and scope children from StackFrameStateNode.cpp (1-161).

## Native ranges

- `src/common/scripting/dap/Nodes/StackFrameStateNode.cpp` **StackFrameStateNode** 1–161 (161 lines)

**Credited native LOC:** 161 (tally `nativeLoc` 161).

## Managed

`VmStackFrame1402Slice.cs`
Managed LOC (tally): 229.
Conversion commit: `e584c1224c920398c852e6f1a18a2e726cf53b55`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Native constructor (**36–50**) | yes |
| Frame constructor (**52–65**) | yes |
| SerializeToProtocol (**67–109**) | yes |
| GetChildNames (**111–125**) | yes |
| GetChildNode (**127–160**) | yes |
| Band hull (**1–161**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| PCToLine | callee on the script function |
| GetSourceData | callee on the pex cache |
| Local, register, global, and cvar constructors | the nodes are created; their bodies are other files |

The native constructor fills a fake frame, points the node at that frame, and creates only the global and cvar scopes. The frame constructor keeps the caller's frame. A non-native script gets a local scope, and every non-native function gets a register scope. Serialize returns immediately for a native function after the name. A script with a character pointer asks the cache. A positive line is copied with column 1. Line -1 uses one past the greatest line-info value. The instruction pointer is formatted only when the PC is set, including when no line is published. Child names append Local, Global, CVars, and Registers for a script. A native function still returns Global and CVars, then leaves the caller's node unchanged. A non-script function still returns its register node.

| Managed (`VmStackFrame1402Slice.cs`) | Native (`StackFrameStateNode.cpp`) |
|--------------------------------------|-------------------------------------|
| Fake frame, protocol frame, and children | **1–161** |
| Band hull (exclusive slice) | **1–161** |

### Chunk **1402** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **161** |
| **Net headline** | **+161** vs the prior chunk |


**Post-review ratio:** 229/161 managed/native ≈ 142% (src file LOC at HEAD; see tally).

## Tests

- `VmStackFrame1402SliceTests.NativeFrameUsesTheFakeStackAndSkipsLocalScopes`
- `VmStackFrame1402SliceTests.ScriptFramePublishesTheLineAndKeepsRegistersForAPlainFunction`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1402 --to 1402` (0 hard / 0 soft).

## Seams

The file is the whole translation unit. PCToLine and the source cache stay callees. The file ends at line 161.

## Credit rationale

UNCredited StackFrameStateNode.cpp 1-161 (+161). After chunk 1401 this is the unique largest hole. RuntimeEvents.cpp 1-160 is 160. The band is shorter than 400 lines and is not sliced. It runs through end of file.
