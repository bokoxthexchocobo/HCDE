# Chunk 229 — DEFINE_GLOBAL/ACTION_FUNCTION exports for FTeam; accessors are chunk 228.

## Scope

DEFINE_GLOBAL/ACTION_FUNCTION exports for FTeam; accessors are chunk 228.

## Native ranges

- `src/gamedata/teaminfo.cpp` **DEFINE_GLOBAL Teams** 347–348 (2 lines)
- `src/gamedata/teaminfo.cpp` **FTeam VM exports** 350–434 (85 lines)

**Credited native LOC:** 87 (tally `nativeLoc` 87).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: ``.

## Tests

- `VmTeamInfoParserTests.FormatTeamList_IncludesIndices`
- `VmTeamInfoParserTests.ParseTeam_ReadsOptions`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 229 --to 229` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
