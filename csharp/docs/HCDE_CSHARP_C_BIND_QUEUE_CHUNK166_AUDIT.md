# Chunk 166 — double-click queue

`VmKeyBindQueue`: queued press deadline, double-binding selection, key-up `+` hold path subset.

**Not credited:** delayed synthetic release (`DelayedReleases`), chat mode filter, `ExecBinding`/`AddCommandString`.

Tests: `VmKeyBindQueueTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
