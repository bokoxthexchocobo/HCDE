# Chunk 261 — DEFINE_ACTION_FUNCTION _DamageTypeDefinition.IgnoreArmor

## Scope

DEFINE_ACTION_FUNCTION _DamageTypeDefinition.IgnoreArmor

## Native ranges

- `src/gamedata/info.cpp` **DEFINE_ACTION_FUNCTION IgnoreArmor** 915–920 (6 lines)

**Credited native LOC:** 6 (tally `nativeLoc` 6).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: ``.

## Tests

- `VmActorInfoNativeExportRecognitionTests.DamageTypeZScriptExports_IncludesIgnoreArmor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 261 --to 261` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
