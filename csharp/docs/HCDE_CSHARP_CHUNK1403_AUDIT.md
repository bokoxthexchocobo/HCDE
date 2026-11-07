# Chunk 1403 — One handler per runtime event from RuntimeEvents.cpp (1-160), copied out of the lock before it runs.

## Scope

One handler per runtime event from RuntimeEvents.cpp (1-160), copied out of the lock before it runs.

## Native ranges

- `src/common/scripting/dap/RuntimeEvents.cpp` **RuntimeEvents** 1–160 (160 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Managed

`VmRuntimeEvents1403Slice.cs`
Managed LOC (tally): 135.
Conversion commit: `4a1b2f6e89157ad229ffcc995652ac61381efd4d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Subscribe and unsubscribe macro (**39–64**, used at **66–72**) | yes |
| EmitBreakpointChangedEvent (**77–92**) | yes |
| EmitInstructionExecutionEvent (**93–108**) | yes |
| EmitLogEvent (**109–124**) | yes |
| EmitExceptionEvent (**125–140**) | yes |
| IsDebugServerRunning (**142–154**) | yes |
| Band hull (**1–160**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| DebugTrace::Debugf | callee; the formatted message is the result |
| CreateStack and CleanupStack emit | the comment at **156–157** says this file does not emit them |

Subscribe replaces the one handler and stores the active flag from whether that handler is set. An empty subscribe still records the subscribe message and leaves the slot inactive. Unsubscribe of an empty handle returns false and does not take the lock. Any other handle, including a different function, clears the slot when it is active and records the unsubscribe message. An inactive slot returns false and keeps the previous message. Each emit reads the flag, then copies the handler under the lock, and calls that copy after the lock is released. A clear between the two reads does not call. IsDebugServerRunning returns the copied predicate, or false when the slot is inactive or empty.

| Managed (`VmRuntimeEvents1403Slice.cs`) | Native (`RuntimeEvents.cpp`) |
|-----------------------------------------|------------------------------|
| One handler per event and the emit copy | **1–160** |
| Band hull (exclusive slice) | **1–160** |

### Chunk **1403** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **160** |
| **Net headline** | **+160** vs the prior chunk |


**Post-review ratio:** 135/160 managed/native ≈ 84% (src file LOC at HEAD; see tally).

## Tests

- `VmRuntimeEvents1403SliceTests.SubscribeReplacesTheOnlyHandlerAndAnyLiveHandleClearsIt`
- `VmRuntimeEvents1403SliceTests.EmitCopiesTheHandlerThenSkipsAClearedSlot`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1403 --to 1403` (0 hard / 0 soft).

## Seams

The file is the whole translation unit. Debug trace formatting stays a callee. CreateStack and CleanupStack have subscribe and unsubscribe only. The file ends at line 160.

## Credit rationale

UNCredited RuntimeEvents.cpp 1-160 (+160). After chunk 1402 this is the unique largest hole. codegen.cpp 3778-3933 is 156. The band is shorter than 400 lines and is not sliced. It runs through end of file.
