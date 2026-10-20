# Chunk 170 — intermission base `ParseKey`

`VmIntermissionModels`, `VmIntermissionTokenize`, `VmIntermissionActionParser` for music/time/background/draw keys.

**Not credited:** `TextLump` filesystem path, cutscene/cast actions, full `FScanner` MAPINFO driver.

Tests: `VmIntermissionActionParserTests.BaseKeys_ParseMusicTimeAndDraw`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
