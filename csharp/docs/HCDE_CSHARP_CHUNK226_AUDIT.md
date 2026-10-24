# Chunk 226 — MAPINFO ParseSkill extended options through skill block finalize. Retired (2026-10-23): `check_port_coverage` `managed_src_head=0` (no manifest-managed linkage); port stays off-tally.

## Scope

MAPINFO ParseSkill extended options through skill block finalize. Retired (2026-10-23): `check_port_coverage` `managed_src_head=0` (no manifest-managed linkage); port stays off-tally.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `3eee12b751e609d0f99bbe167662c4fe2068cd87`.

## Padding review (2026-10-21)



### Out-of-body uncredit (226)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **226** |
| **Out-of-body removed** | **0** |
| **Honest** | **226** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 226 --to 226` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
