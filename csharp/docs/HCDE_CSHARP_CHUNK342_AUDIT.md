# Chunk 342 — EMAPINFO unsupported sample stats header

## Scope

EMAPINFO unsupported sample stats header

## Native ranges

- `src/gamedata/hcde_emapinfo.cpp` **EMapInfoStats unsupported samples** 33–50 (18 lines)

**Credited native LOC:** 18 (tally `nativeLoc` 18).

## Managed

`VmEMapInfoStatsModel.cs`, `VmEMapInfoStatsModelTests.cs`
Managed LOC (tally): 18.
Conversion commit: `0a658901c69b0f8c483093f30724e9f31505b283`.

## Tests

- `VmEMapInfoStatsModelTests.ShouldRecordSample_CapsAtEight`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 342 --to 342` (0 hard / 0 soft).

## Seams

Anonymous-namespace `TMap` registries are not re-hosted; only the unsupported-sample cap used by `RecordUnsupported` is modeled.

## Credit rationale

Credits the uncredited `hcde_emapinfo.cpp` header band 33–50 defining `EMapInfoUnsupportedSample` and the eight-sample retention policy consumed by stats recording helpers.
