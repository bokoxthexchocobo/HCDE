# Chunk 179 — intermission screen init backgrounds

`VmIntermissionScreenInit.ResolveBackground` (`@` finale page index, `$` lookup).

**Not credited:** texture load, overlay init, music start.

Tests: `VmIntermissionParseCompleteTests.ScreenInit_ResolvesFinalePageIndex`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
