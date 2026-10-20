# Chunk 246 — Re-audit: FState::MatchesCallerType, GetSpriteIndex, LoadAltHud default switch only.

## Scope

Re-audit: FState::MatchesCallerType, GetSpriteIndex, LoadAltHud default switch only.

## Native ranges

- `src/gamedata/info.cpp` **FState::MatchesCallerType** 113–147 (35 lines)
- `src/gamedata/info.cpp` **GetSpriteIndex** 271–306 (36 lines)
- `src/gamedata/info.cpp` **LoadAltHudStuff defaults** 319–335 (17 lines)

**Credited native LOC:** 88 (tally `nativeLoc` 88).

## Managed

`VmActorInfoRuntimeTests.cs`, `VmActorStateRuntime.cs`, `VmAltHudLoader.cs`, `VmSpriteIndexLookup.cs`
Managed LOC (tally): 94.
Conversion commit: `4b223c80ab9e2de5c7e19c99677b0c774128303b`.

## Tests

- `VmActorInfoRuntimeTests.MatchesCallerType_RejectsWrongClass`
- `VmActorInfoRuntimeTests.GetSpriteIndex_CachesLastLookup`
- `VmActorInfoRuntimeTests.AltHud_DefaultsForHeretic`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 246 --to 246` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
