# Chunk 171 — intermission fader / wiper / scroller keys

Extended `VmIntermissionActionParser` for `FadeType`, `WipeType`, scroll timing keys.

**Not credited:** runtime screenjob classes, wipe render paths.

Tests: `VmIntermissionActionParserTests.FaderAndWiper_ParseSpecializedKeys`, `Scroller_ConvertsSecondsToTics`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
