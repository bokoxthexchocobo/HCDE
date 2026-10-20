# Chunk 327 — EMAPINFO ApplyInt Double BossSpecials DefaultEnvironment lump head

## Scope

EMAPINFO ApplyInt Double BossSpecials DefaultEnvironment lump head

## Native ranges

- `src/gamedata/hcde_emapinfo.cpp` **ApplyInt ApplyDouble ApplyBossSpecials** 405–464 (60 lines)
- `src/gamedata/hcde_emapinfo.cpp` **ApplyDefaultEnvironment ResolveSiblingLump head** 466–533 (68 lines)

**Credited native LOC:** 128 (tally `nativeLoc` 128).

## Managed

`VmEMapInfoApplyHelpers.cs`, `VmEMapInfoApplyHelpersTests.cs`, `VmEMapInfoLumpResolve.cs`
Managed LOC (tally): 55.
Conversion commit: `5e9cfd300693278354535a2051930dc9fed1da76`.

## Tests

- `VmEMapInfoApplyHelpersTests.ApplyIntProperty_RejectsBadNumber`
- `VmEMapInfoApplyHelpersTests.ApplyBossSpecials_ParsesE1M8`
- `VmEMapInfoApplyHelpersTests.ApplyDefaultEnvironment_RecordsUnsupportedWhenMissing`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 327 --to 327` (0 hard / 0 soft).

## Seams

`HCDEReverbContainers` and `fileSystem` lump lookup are injected; boss-special bit masks use placeholder flags consistent with earlier `TryParseBossSpecialFlag` chunk.

## Credit rationale

Credits `hcde_emapinfo.cpp` 404–533 between the chunk 284 `ApplyBoolProperty` row and chunk 286 lump tail: int/double apply wrappers, boss-special list parsing, Eternity reverb ID resolution with unsupported fallback, and the first segment of `ResolveSiblingLump` name normalization exercised by the apply/lump tests.
