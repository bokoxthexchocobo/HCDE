# Chunk 321 — MAPINFO skymist through gravity air options

## Scope

MAPINFO skymist through gravity air options

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **MAPINFO skymist sucktime** 1180–1271 (92 lines)
- `src/gamedata/g_mapinfo.cpp` **MAPINFO fadetable gravity** 1297–1373 (77 lines)

**Credited native LOC:** 169 (tally `nativeLoc` 169).

## Managed

`VmMapInfoMapOptionsEnvironment.cs`, `VmMapInfoMapOptionsEnvironmentTests.cs`, `VmMapInfoMapOptionsScalars.cs`, `VmMapInfoMapOptionsScalarsTests.cs`
Managed LOC (tally): 120.
Conversion commit: `f49971c89ddde51423abdc961c7e9ef0f39bf203`.

## Tests

- `VmMapInfoMapOptionsScalarsTests.ClampSkyMistYScale_Bounds`
- `VmMapInfoMapOptionsScalarsTests.ClampWaveLimit_CapsAt3000`
- `VmMapInfoMapOptionsScalarsTests.ParseMusicPair_PreservesOrder`
- `VmMapInfoMapOptionsEnvironmentTests.ParseCdIdHex_ReadsHex`
- `VmMapInfoMapOptionsEnvironmentTests.ClampWallShade_DividesByTwo`
- `VmMapInfoMapOptionsEnvironmentTests.ApplyEvenLighting_ClearsSkySpeeds`
- `VmMapInfoMapOptionsEnvironmentTests.ApplyNoGravity_ReturnsMax`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 321 --to 321` (0 hard / 0 soft).

## Seams

MAPINFO handlers omit live `FMapInfoParser` scanner state; managed code models option semantics only.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
