# Chunk 263 — FindStyle, MBF21 flags, SetDehParams guards, PatchThing group clamps, DEHSUPP RenderStyles.

## Scope

FindStyle, MBF21 flags, SetDehParams guards, PatchThing group clamps, DEHSUPP RenderStyles.

## Native ranges

- `src/gamedata/d_dehacked.cpp` **Modes table** 463–487 (25 lines)
- `src/gamedata/d_dehacked.cpp` **DEHToDouble** 495–498 (4 lines)
- `src/gamedata/d_dehacked.cpp` **HandleKey** 525–536 (12 lines)
- `src/gamedata/d_dehacked.cpp` **FindStyle** 590–598 (9 lines)
- `src/gamedata/d_dehacked.cpp` **SetDehParams index guard** 1099–1109 (11 lines)
- `src/gamedata/d_dehacked.cpp` **SetDehParams printable name** 1162–1172 (11 lines)
- `src/gamedata/d_dehacked.cpp` **deh_mobjflags_mbf21** 1198–1218 (21 lines)
- `src/gamedata/d_dehacked.cpp` **ClearBits2Stuff** 1220–1231 (12 lines)
- `src/gamedata/d_dehacked.cpp` **PatchThing Render Style** 1366–1375 (10 lines)
- `src/gamedata/d_dehacked.cpp` **PatchThing group clamps** 1387–1409 (23 lines)
- `src/gamedata/d_dehacked.cpp` **RenderStyles DEHSUPP** 3708–3724 (17 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmDehackedMbf21FlagNames.cs`, `VmDehackedMbf21FlagNamesTests.cs`, `VmDehackedModeNames.cs`, `VmDehackedModeNamesTests.cs`, `VmDehackedNumericConversion.cs`, `VmDehackedNumericConversionTests.cs`, `VmDehackedPatchNaming.cs`, `VmDehackedPatchNamingTests.cs`, `VmDehackedRenderStyleLookup.cs`, `VmDehackedRenderStyleLookupTests.cs`, `VmDehackedStructKeyApply.cs`, `VmDehackedStructKeyApplyTests.cs`, `VmDehackedThingGroupLimits.cs`, `VmDehackedThingGroupLimitsTests.cs`
Managed LOC (tally): 95.
Conversion commit: `729a3daed6733f61b5ece219ed475bd0635ca84e`.

## Tests

- `VmDehackedModeNamesTests.PatchModes_IncludesThing`
- `VmDehackedNumericConversionTests.DehToDouble_ConvertsFixedPoint`
- `VmDehackedStructKeyApplyTests.TryApply_MatchesKey`
- `VmDehackedRenderStyleLookupTests.FindStyle_ReturnsRegisteredStyle`
- `VmDehackedPatchNamingTests.IsCodePointerIndexInRange_RejectsOutOfBounds`
- `VmDehackedPatchNamingTests.SanitizePatchName_ReplacesUnsafeCharacters`
- `VmDehackedMbf21FlagNamesTests.KnownFlags_IncludesBoss`
- `VmDehackedMbf21FlagNamesTests.ClearMbf21Defaults_ResetsGravity`
- `VmDehackedThingGroupLimitsTests.ClampInfightingGroup_RejectsNegative`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 263 --to 263` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
