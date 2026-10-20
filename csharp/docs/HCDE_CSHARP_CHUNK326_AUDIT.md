# Chunk 326 — EMAPINFO SetLevelNum GetOrCreateLevel NormalizeMusicName stats apply bool

## Scope

EMAPINFO SetLevelNum GetOrCreateLevel NormalizeMusicName stats apply bool

## Native ranges

- `src/gamedata/hcde_emapinfo.cpp` **SetLevelNum GetOrCreateLevel NormalizeMusicName** 302–375 (74 lines)
- `src/gamedata/hcde_emapinfo.cpp` **RecordUnsupported ApplyBoolProperty** 377–394 (18 lines)

**Credited native LOC:** 92 (tally `nativeLoc` 92).

## Managed

`VmEMapInfoLevelBootstrap.cs`, `VmEMapInfoLevelBootstrapTests.cs`, `VmEMapInfoMusicNormalize.cs`, `VmEMapInfoMusicNormalizeTests.cs`
Managed LOC (tally): 85.
Conversion commit: `c5a40d322627b274446730b37cd3d531a22f712a`.

## Tests

- `VmEMapInfoLevelBootstrapTests.SetLevelNum_ClearsDuplicate`
- `VmEMapInfoLevelBootstrapTests.GetOrCreateLevel_AssignsDefaultMapNum`
- `VmEMapInfoMusicNormalizeTests.NormalizeMusicName_PrefixesDoomTrack`
- `VmEMapInfoMusicNormalizeTests.NormalizeMusicName_PreservesExplicitPath`
- `VmEMapInfoApplyHelpersTests.ApplyBoolProperty_RecordsInvalidOnBadToken`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 326 --to 326` (0 hard / 0 soft).

## Seams

`FindLevelInfo` and `wadlevelinfos` storage are modeled as injectable lists; `RecordUnsupported`/`ApplyBoolProperty` reuse existing managed stats helpers without pulling in the full parse loop.

## Credit rationale

Covers the uncredited `hcde_emapinfo.cpp` band 302–394 between chunk 283's `GetDefaultLevelNum` tail and chunk 284's `GetOrCreateLevel` row: duplicate `levelnum` clearing, lazy level creation with default MAP/E#M# numbering, Doom/Heretic music prefix normalization, and the stats/bool-apply helpers that feed the property dispatcher.
