# Chunk 1385 — DAP debug server from DebugServer.cpp (1-194), through end of file.

## Scope

DAP debug server from DebugServer.cpp (1-194), through end of file.

## Native ranges

- `src/common/scripting/dap/DebugServer.cpp` **DebugServer::Listen** 1–194 (194 lines)

**Credited native LOC:** 194 (tally `nativeLoc` 194).

## Managed

`VmDebugServer1385Slice.cs`
Managed LOC (tally): 291.
Conversion commit: `4e789e9a8254f1b72d3e6927085647eedcaf1371`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Preamble and includes (**1–31**) | yes |
| Namespace open (**32**) | yes |
| Constructor (**34–41**) | yes |
| `RunRestartThread` (**43–73**) | yes |
| `Listen` (**75–98**) | yes |
| `onClientConnected` (**100–139**) | yes |
| `onError` (**141–157**) | yes |
| `StartServer` (**159–171**) | yes |
| `Stop` (**173–191**) | yes |
| Destructor (**193**) | yes |
| Namespace close (**194**) | yes |
| Band hull (**1–194**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Header fields | declarations, not this cpp |
| `EndSession`, `StartSession`, `IsEndingSession` | debugger callees |
| Socket `start` and `stop` | server callees |
| Debugger-enabled subscribe and unsubscribe | runtime-event callees |

Port 0 logs the skip warning and returns before a socket or a restart routine exists. Any other port stores the port, creates the socket when missing, and otherwise stops the live socket first. That stop sets `stopped`, and the following listen does not clear it. The restart routine then ends the session and returns without building a socket. A closed connection with the gate free, `terminate` clear, and no session ending sets `closed` and `terminate`. A held gate or an ending session only logs the close. Disconnect sets `terminate` and leaves `closed` unchanged. The restart routine calls `EndSession` with the current `closed` flag, clears `terminate`, and throws `ExitEvent` reason 0 when the debugger is quitting. That throw happens before the stopped trace. A server error while no restart is pending sets `terminate`, `closed`, and the restart flag. The routine stops the socket, replaces it, and starts it. A start that reports an error while the restart flag is still set logs the restart-failed line and leaves the flag set. A failed start unsubscribes, sets `stopped`, and logs the restart failure. A null error string is `(null)` in the red log and `unknown` in the warning.

| Managed (`VmDebugServer1385Slice.cs`) | Native (`DebugServer.cpp`) |
|------------------------------------------------|--------------------------------|
| Debug server listen, restart, and stop | **1–194** |
| Band hull (exclusive slice) | **1–194** |

### Chunk **1385** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **194** |
| **Net headline** | **+194** vs the prior chunk |


**Post-review ratio:** 291/194 managed/native ≈ 150% (src file LOC at HEAD; see tally).

## Tests

- `VmDebugServer1385SliceTests.ListenSkipsPortZeroAndCloseOrDisconnectWakesTheRestartThread`
- `VmDebugServer1385SliceTests.ErrorRestartsUntilStartFailsAndRelistenLeavesStopped`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1385 --to 1385` (0 hard / 0 soft).

## Seams

The header only declares the server. ZScriptDebugger session calls, socket start and stop, and RuntimeEvents subscribe and unsubscribe stay callees. The restart routine parks while terminate is clear, and Stop joins that routine. Quitting throws ExitEvent reason 0 from the restart routine, before the stopped trace. Re-listen stores the new port and then Stop, which leaves stopped set, so the next terminate ends the session and does not build a server. A null server-error argument is formatted as (null). The warning uses unknown. An error raised while a restart is already pending logs the restart-failed line and does not schedule another restart.

## Credit rationale

UNCredited DebugServer.cpp 1-194 (+194). After chunk 1384 the top hole is a tie at 194 with d_net.cpp 4300-4493. Path order selects DebugServer.cpp. codegen.cpp 6316-6508 is 193. The file is shorter than 400 lines and is not sliced.
