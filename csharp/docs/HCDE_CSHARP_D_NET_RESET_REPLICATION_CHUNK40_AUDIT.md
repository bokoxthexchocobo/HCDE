# Net_ResetClientState replication cursors — chunk 40 audit

**Native:** `d_net.cpp` `Net_ResetClientState` actor-delta cursors and priority queues 5397–5400.

**Managed:** `NetClientActorReplicationQueues.cs`, `NetClientDisconnectSession.cs`, `NetPresentationEchoResetTests`.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
