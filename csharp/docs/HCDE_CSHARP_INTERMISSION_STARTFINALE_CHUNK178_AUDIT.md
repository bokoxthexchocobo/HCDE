# Chunk 178 — F_StartFinale builder

`VmFinaleIntermissionBuilder` textscreen + ending wiper descriptor assembly.

**Not credited:** `F_StartIntermission` controller, chess-ending early out beyond flag.

Tests: `VmIntermissionParseCompleteTests.FinaleBuilder_AddsWiperWhenEnding`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
