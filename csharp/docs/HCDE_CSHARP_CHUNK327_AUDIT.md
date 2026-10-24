# Chunk 327 — EMAPINFO ApplyInt/Double/BossSpecials and lump normalize (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

EMAPINFO ApplyInt/Double/BossSpecials and lump normalize (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppLumpGate.cs`, `VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `9d1715ebb7257ab57e010713315055e2d259bc52`.

## Padding review (2026-10-21)



### Out-of-body uncredit (327)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **27** |
| **Out-of-body removed** | **27** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 327 --to 327` (0 hard / 0 soft).

## Seams

`HCDEReverbContainers` and `fileSystem` lump lookup are injected; boss-special bit masks use placeholder flags consistent with earlier `TryParseBossSpecialFlag` chunk.

## Credit rationale

Credits typed apply wrappers, `E1M8` boss-special bit, and `NormalizeResourceName` head used by lump resolve tests.
