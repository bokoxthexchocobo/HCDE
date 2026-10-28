# Chunk 189 — SNDINFO include path resolution

`VmSndInfoIncludePath.Resolve` mirrors `S_ResolveIncludePath`.

Tests: `VmSndInfoIncludePathTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
