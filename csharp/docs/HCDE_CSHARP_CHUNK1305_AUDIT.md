# Chunk 1305 — Debug groups, the message log, and the breakpoint callback from gl_debug.cpp (1-365).

## Scope

Debug groups, the message log, and the breakpoint callback from gl_debug.cpp (1-365).

## Native ranges

- `src/common/rendering/gl/gl_debug.cpp` **FGLDebug::Update** 1–365 (365 lines)

**Credited native LOC:** 365 (tally `nativeLoc` 365).

## Managed

`VmGlDebug1305Slice.cs`
Managed LOC (tally): 388.
Conversion commit: `701fd743b7fc141b6139ce4cf41af0b5248b627d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Debug-level cvar callback (**31–37**) | yes |
| Update query drain (**59–83**) | yes |
| Object labels (**92–106**) | yes |
| PushGroup and PopGroup (**115–142**) | yes |
| Breakpoint mode and logging level (**153–188**) | yes |
| Severity filter and message log (**197–252**) | yes |
| PrintMessage and DebugCallback (**260–308**) | yes |
| Source, type, and severity names (**316–363**) | yes |
| Band hull (**1–365**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| OpenGL entry points | host records the call; this band decides when and with which arguments |

The cvar callback prints only when the debug-api flag is clear. Update always clears the stat string, reads each elapsed query, deletes it, and appends name=%04.2f ms. It then copies the keep flag onto the active flag and clears keep. Without the debug api it returns after that drain. Labels and debug groups run only when the api flag is set and the level is not zero. A GPU stat that is active still begins a time query and still ends it on pop, even when the debug group is skipped. Breakpoint mode installs the callback with the debug object and enables synchronous output only when the cvar differs from the stored mode; clearing it passes a null callback and disables synchronous output. The logging level is stored as an unsigned enum, so a negative cvar enables every severity. The log is read only when that stored level is greater than zero, fifty messages at a time, and a message that does not fit the buffer stays queued. Push and pop group types are dropped. A repeated message does not advance the counter. The fiftieth new message prints only the suppression line. The fifty-first is remembered and not printed. Later messages return before they are remembered. The callback skips filtered severities before the notification assert, and a severity that is not a notification fails that assert after the print. Names for an unknown source, type, or severity are the decimal value.

| Managed (`VmGlDebug1305Slice.cs`) | Native (`gl_debug.cpp`) |
|-----------------------------------|-------------------------|
| Queries, groups, log, and callback | **1–365** |
| Band hull (exclusive slice) | **1–365** |

### Chunk **1305** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **365** |
| **Net headline** | **+365** vs post-**1304** |


**Post-review ratio:** 388/365 managed/native ≈ 106% (src file LOC at HEAD; see tally).

## Tests

- `VmGlDebug1305SliceTests.Update_DrainsQueriesAndArmsTheBreakpointOnce`
- `VmGlDebug1305SliceTests.Log_StopsAtFiftyAndTheCallbackAsserts`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1305 --to 1305` (0 hard / 0 soft).

## Seams

HasDebugApi is the RFL_DEBUG bit (128) on the renderer flags. The debug level and breakpoint are the gl_debug_level and gl_debug_breakpoint cvars. GPU stat strings are the renderer globals. The message cap and the elapsed-query list are process lifetime, shared by every debug object.

## Credit rationale

UNCredited gl_debug.cpp 1-365 (+365). A full tally line scan shows no overlap. The file ends at line 365.
