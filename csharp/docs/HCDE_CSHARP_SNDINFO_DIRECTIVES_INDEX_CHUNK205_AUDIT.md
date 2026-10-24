# Chunk 205 — SNDINFO sound-setting directive index

`VmSndInfoDirectiveIndex` documents native `SI_Alias`…`SI_Random` coverage in `SndInfoMapMusicParser`.

Tests: `VmSndInfoDirectiveIndexTests`, `SndInfoMapMusicParserTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.

## Addendum (recognition reclass)

Git blame on `SndInfoMapMusicParser.cs` shows `$alias`, `$limit`, `$rolloff`, `$random`, and related directive handling landed in **`356fa958`** (2026-10-08) and **`ba92aa505`** (2026-10-14), before PR conversion chunks 195–200. Chunk **205** native ranges (`s_advsound.cpp` 955–1136) are therefore **`recognition`**, not **`conversion`**. `VmSndInfoDirectiveIndex` (commit `d0719376`) is documentation only and does not re-credit conversion native LOC.
