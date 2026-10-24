# Chunk 344 — TranslateTeleportThings and SetTexture warn policy

## Scope

TranslateTeleportThings and SetTexture warn policy

## Native ranges

- `src/maploader/maploader.cpp` **TranslateTeleportThings SetTexture** 78–250 (173 lines)

**Credited native LOC:** 173 (tally `nativeLoc` 173).

## Managed

`VmMapLoaderMissingTexturePolicy.cs`, `VmMapLoaderMissingTexturePolicyTests.cs`, `VmMapLoaderTeleportTranslate.cs`, `VmMapLoaderTeleportTranslateTests.cs`
Managed LOC (tally): 36.
Conversion commit: `049dbe1ec1591792621c1dd447ff7818816e8cf2`.

## Tests

- `VmMapLoaderTeleportTranslateTests.ApplyTeleportLineArgs_SetsTidWhenTagZero`
- `VmMapLoaderMissingTexturePolicyTests.ShouldPrintWarning_UntilLimit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 344 --to 344` (0 hard / 0 soft).

## Seams

Thinker iteration and `TexMan` IO remain outside managed code; TID assignment and missing-texture warn caps mirror loader policy.

## Credit rationale

First whole-function slice of untouched `maploader.cpp`: `TranslateTeleportThings` TID rewrite for untagged teleport sectors and `SetTexture` missing-texture warn limits plus eight-character flat truncation.
