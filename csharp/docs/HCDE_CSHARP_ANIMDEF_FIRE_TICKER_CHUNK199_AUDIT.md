# Chunk 199 — Fire texture parse and animation tickers

`VmAnimDefFireParser`, `VmTextureAnimationTicker` (`SetSwitchTime` / `AdvanceFrame` / update loop).

Tests: `VmAnimDefExtendedClauseTests.FireTexture_ParsesTicsAndColors`, `VmTextureAnimationTickerTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
