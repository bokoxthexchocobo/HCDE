# Chunk 294 — EMAPINFO skydelta, unevenlight, SkyPic2 guard

## Scope

EMAPINFO skydelta, unevenlight, SkyPic2 guard

## Native ranges

- `src/gamedata/hcde_emapinfo.cpp` **ApplyEMapInfoProperty tail** 675–824 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 15).

## Managed

`VmEMapInfoPropertyTail.cs`, `VmEMapInfoPropertyTailTests.cs`
Managed LOC (tally): 21.
Conversion commit: `31f66b4267504076eea265623a9dd897c872823e`.

## Tests

- `VmEMapInfoPropertyTailTests.IsNoFlatSky2_DetectsMarker`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 294 --to 294` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
