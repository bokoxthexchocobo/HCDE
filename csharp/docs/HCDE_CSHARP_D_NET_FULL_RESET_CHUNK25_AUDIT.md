# d_net Net_ResetClientState and DisconnectClient — chunk 25 audit

## Scope

Port the remainder of native client-slot reset and disconnect teardown not credited in chunk 13 (latency rings, consistency history, malformed-packet counters, player slot flags).

**Native:** `d_net.cpp` `Net_ResetClientState` 5349–5394; `DisconnectClient` 5403–5424, 5427–5437, 5443–5452, 5454–5455.

**Managed:** `NetClientChannelResetState.cs`, `NetClientDisconnectTeardown.cs`, `NetClientFullResetTests`.

## Verified

`dotnet build -warnaserror`; full `dotnet test`.
