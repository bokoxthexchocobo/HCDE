# Chunk 204 — SNDINFO player registry helpers

`VmSndInfoPlayerClassRegistry`, `VmSndInfoGender` (player sound common parse, class table).

Tests: `VmSndInfoPlayerClassRegistryTests`, existing `SndInfoMapMusicParserTests` / `SndInfoPlayerSoundTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
