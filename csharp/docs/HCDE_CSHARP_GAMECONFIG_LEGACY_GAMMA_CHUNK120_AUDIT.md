# Chunk 120 — `gameconfigfile.cpp` `LegacyGammaToFixGamma`

Maps legacy `vid_gamma` (0.1–3.0) into `vid_fixgamma` storage range.

Tests: `VmGameConfigLegacyGammaTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
