# Chunk 221 — UMAPINFO extended properties, map lump ingest, CommitUMapinfo. Retired (2026-10-24): thin managed at HEAD — surviving **0** < **170** (50% of honest **339**).

## Scope

UMAPINFO extended properties, map lump ingest, CommitUMapinfo. Retired (2026-10-24): thin managed at HEAD — surviving **0** < **170** (50% of honest **339**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppLumpGate.cs`, `VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `c881d1dce5ff069f2ed2509fd8006efd4610cab5`.

## Padding review (2026-10-21)



### Out-of-body uncredit (221)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **339** |
| **Out-of-body removed** | **0** |
| **Honest** | **339** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 221 --to 221` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
