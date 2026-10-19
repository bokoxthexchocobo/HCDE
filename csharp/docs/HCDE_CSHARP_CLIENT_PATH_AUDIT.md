# Principal audit — client path and IWAD scan fixes

**Date:** 2026-10-19  
**Scope:** `VmMusicPatchPaths.IsAbsolute` and `VmIwadCandidateScanner` hidden/dotfile rules.  
**Result:** Accepted. Linux CI no longer treats `C:\foo` as a relative patch path; recursive IWAD scan skips dot-prefixed leaves.

## Test evidence

```text
dotnet test csharp/tests/HCDE.Client.Tests/HCDE.Client.Tests.csproj -c Release
```

2338/2338 passed in this run.

## LOC tally

`LOC tally: converted this run = 0 native lines (6 C# lines added); native remaining = 508,048`
