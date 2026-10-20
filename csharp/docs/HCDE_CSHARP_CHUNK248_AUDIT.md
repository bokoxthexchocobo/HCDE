# Chunk 248 — Re-audit: DmgFactors::Apply, SummonActor net payload, ParseDamageDefinition subset (VM export gap 915-920 in chunk 261).

## Scope

Re-audit: DmgFactors::Apply, SummonActor net payload, ParseDamageDefinition subset (VM export gap 915-920 in chunk 261).

## Native ranges

- `src/gamedata/info.cpp` **DmgFactors::Apply** 819–837 (19 lines)
- `src/gamedata/info.cpp` **SummonActor net write** 857–864 (8 lines)
- `src/gamedata/info.cpp` **FMapInfoParser::ParseDamageDefinition** 1005–1040 (36 lines)

**Credited native LOC:** 63 (tally `nativeLoc` 63).

## Managed

`VmActorInfoNativeExportRecognition.cs`, `VmActorInfoNativeExportRecognitionTests.cs`, `VmDamageTypeDefinition.cs`, `VmSummonCommandBuilder.cs`
Managed LOC (tally): 86.
Conversion commit: `22adc45abeafe89d468ab96e2445f932e42f67fd`.

## Tests

- `VmActorInfoRuntimeTests.DamageFactor_AppliesSpecificType`
- `VmActorInfoRuntimeTests.Summon_BuildsExtendedMessage`
- `VmActorInfoRuntimeTests.TryParseDamageDefinition_AppliesNoArmor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 248 --to 248` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
