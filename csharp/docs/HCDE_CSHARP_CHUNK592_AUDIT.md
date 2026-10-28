# Chunk 592 — FBehavior::Init format switch, module naming, old-lump upgrade, map-var bootstrap (2329–2497)

## Scope

FBehavior::Init format switch, module naming, old-lump upgrade, map-var bootstrap (2329–2497)

## Native ranges

- `src/playsim/p_acs.cpp` **FBehavior::Init** 2329–2356 (28 lines)
- `src/playsim/p_acs.cpp` **FBehavior::Init** 2358–2414 (57 lines)
- `src/playsim/p_acs.cpp` **FBehavior::Init** 2416–2497 (82 lines)

**Credited native LOC:** 167 (tally `nativeLoc` 167).

## Managed

`VmAcsBehaviorInitLoad.cs`
Managed LOC (tally): 102.
Conversion commit: `edd3e5ef6839fbe4f5b1d0e8ea7d397c95b699fb`.

## Padding review (2026-10-21)

Whole-function ports only; measured after faithful port (no LOC target).

| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `ParseAcsFormatByte` | `FBehavior::Init` format switch 2329–2343 |
| `ResolveModuleName` | module name selection 2346–2353 |
| `ShouldUpgradeOldEnhanced` | old ACSE/ACSe upgrade 2358–2373 |
| `ShouldLocalizeHexenHack` | Hexen localization gate 2390–2397 |
| `BindOldMapVarPointers` / `LoadMiniChunkVars` | map var pointer/MINI init 2416–2497 |

**Post-review ratio:** 102/167 managed/native ≈ 61% (src file LOC at HEAD; see tally).

## Tests

- `VmAcsBehaviorInitLoadTests.ParseAcsFormatByte_MapsEnhancedMarkers`
- `VmAcsBehaviorInitLoadTests.ResolveModuleName_FromFileUsesShortName`
- `VmAcsBehaviorInitLoadTests.ShouldUpgradeOldEnhanced_DetectsAcSe`
- `VmAcsBehaviorInitLoadTests.ShouldLocalizeHexenHack_MatchesHexenWad`
- `VmAcsBehaviorInitLoadTests.LoadMiniChunkVars_WritesSpan`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 592 --to 592` (0 hard / 0 soft).

## Seams

`LoadScriptsDirectory`, `UnescapeStringTable`, `FindChunk`, and `fileSystem` IO are not hosted.

## Credit rationale

Whole-function slices of `FBehavior::Init` with faithful managed mirrors.
