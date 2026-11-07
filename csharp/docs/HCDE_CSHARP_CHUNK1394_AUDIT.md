# Chunk 1394 — I_NetError through I_NetDone from i_net.cpp (1639-1811).

## Scope

I_NetError through I_NetDone from i_net.cpp (1639-1811).

## Native ranges

- `src/common/engine/i_net.cpp` **I_NetError** 1639–1811 (173 lines)

**Credited native LOC:** 173 (tally `nativeLoc` 173).

## Managed

`VmNetWindow1394Slice.cs`
Managed LOC (tally): 209.
Conversion commit: `2f19aeac69cd51f694d657a42cae0f1d212ffaf3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| I_NetError (**1641–1649**) | yes |
| I_NetInit (**1651–1665**) | yes |
| I_NetMessage (**1672–1686**) | yes |
| I_NetLoop (**1697–1728**) | yes |
| I_NetClientConnected (**1732–1750**) | yes |
| I_NetClientUpdated (**1753–1756**) | yes |
| I_NetClientDisconnected (**1758–1770**) | yes |
| I_NetUpdatePlayers (**1772–1780**) | yes |
| I_ShouldStartNetGame (**1782–1787**) | yes |
| I_GetKickClients and I_GetBanClients (**1789–1805**) | yes |
| I_NetDone (**1807–1810**) | yes |
| Band hull (**1639–1811**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| SetConnectFlow | ends at line 1638, outside this hole |
| I_ClearClient | starts at line 1812, outside this hole |
| I_NetLog | callee; this tree prints a carriage return and a 40-column field |
| NetStartWindow | callee; the interactive loop returns the window result and does not call the callback |
| HCDERconPollListener | callee; polled only while dedicated |

I_NetError logs the fatal line, warns, closes the window only when the session is interactive, and does not return. I_NetInit and I_NetMessage print the NetServer or NetSession tag. The Windows dedicated status line is recorded only when that build and dedicated mode are both set. The window guard runs only for an interactive session. The headless loop polls rcon when dedicated, sleeps between failed callbacks, and a Windows dedicated abort returns before the trailing pump. A reserved slot prints the authority line and skips connect. Kick and ban clear the list, then push until the callee returns -1.

| Managed (`VmNetWindow1394Slice.cs`) | Native (`i_net.cpp`) |
|-------------------------------------|----------------------|
| Pregame net window through I_NetDone | **1639–1811** |
| Band hull (exclusive slice) | **1639–1811** |

### Chunk **1394** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **173** |
| **Net headline** | **+173** vs the prior chunk |


**Post-review ratio:** 209/173 managed/native ≈ 121% (src file LOC at HEAD; see tally).

## Tests

- `VmNetWindow1394SliceTests.ErrorInitMessageAndLoopFollowTheDedicatedBranch`
- `VmNetWindow1394SliceTests.ClientFlowKickBanAndDoneUseTheWindowCallees`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1394 --to 1394` (0 hard / 0 soft).

## Seams

SetConnectFlow ends at line 1638 and stays outside this band. I_ClearClient starts at line 1812 and stays outside. I_NetLog, NetStartWindow, HCDERconPollListener, and the dedicated-server console helpers are callees.

## Credit rationale

UNCredited i_net.cpp 1639-1811 (+173). After chunk 1393 this is the unique largest hole. sc_man.cpp 1-170 is 170. The band is shorter than 400 lines and is not sliced.
