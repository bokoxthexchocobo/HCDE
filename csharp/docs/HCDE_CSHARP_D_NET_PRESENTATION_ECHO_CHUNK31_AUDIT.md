# d_net presentation echo and remote interp reset — chunk 31 audit

**Native:** `d_net_diag.cpp` `Net_ResetPresentationEchoStateForClient` 797–806, `Net_ResetPresentationEchoState` 808–812; `d_net.cpp` `HCDERemotePlayerInterp` reset 5395–5396.

**Managed:** `NetClientPresentationEchoRegistry.cs`, `RemotePlayerInterpResetState.cs`, `NetClientDisconnectSession` wiring, `NetPresentationEchoResetTests`.

Verified: `dotnet build -warnaserror`; full `dotnet test`.
