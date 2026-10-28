# Chunk 121 — `s_doomsound.cpp` music path rules

Mirrors `IsKnownMusicExtension`, `HasMusicPathSegment`, and `MusicPathMatchesName` used by nested music lookup.

Tests: `VmDoomMusicPathRulesTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
