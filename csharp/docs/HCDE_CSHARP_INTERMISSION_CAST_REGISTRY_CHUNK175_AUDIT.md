# Chunk 175 — intermission cast / registry / document

Cast `ParseKey`, cutscene body storage, `VmIntermissionRegistry`, `VmIntermissionDocumentParser`.

**Not credited:** full `ParseCutscene` screenjob driver, `GotoTitle` runtime class binding.

Tests: `VmIntermissionParseCompleteTests` (cast, document).

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
