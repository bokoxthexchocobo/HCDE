# Chunk 176 — TextLump + ParseEndGame

`TextLump` resolver hook (behavior under chunk 172 textscreen range) and `VmEndGameParser` legacy endgame blocks.

**Not credited:** MAPINFO `FScanner` driver, `format_type` lenient unknown properties.

Tests: `VmIntermissionParseCompleteTests` (TextLump, EndGame).

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
