# Chunk 395 — CVARINFO scanner and deh/bex patch discovery

## Scope

CVARINFO scanner and deh/bex patch discovery

## Native ranges

- `src/d_main.cpp` **ParseCVarInfo** 2086–2241 (156 lines)
- `src/d_main.cpp` **ConsiderPatches** 2251–2267 (17 lines)

**Credited native LOC:** 173 (tally `nativeLoc` 173).

## Managed

`VmDMainParseCVarInfo.cs`, `VmDMainParseCVarInfoTests.cs`
Managed LOC (tally): 86.
Conversion commit: `1a55c44f39854456fb794c7b8cdd04f62cde350a`.

## Tests

- `VmDMainParseCVarInfoTests.HasValidScopeFlags_RejectsMissingScope`
- `VmDMainParseCVarInfoTests.NormalizeConfigOnlyFlags_ClearsServerUser`
- `VmDMainParseCVarInfoTests.IsPatchExtension_MatchesDeh`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 395 --to 395` (0 hard / 0 soft).

## Seams

`FScanner`, `C_CreateCVar`, and `D_LoadDehFile` are not hosted.

## Credit rationale

Credits `ParseCVarInfo` and `ConsiderPatches` from `d_main.cpp`.
