# Chunk 320 — MAPINFO map options levelnum through sky2 Retired (2026-10-23): out-of-body credit removed; honest **82** < **150**.

## Scope

MAPINFO map options levelnum through sky2 Retired (2026-10-23): out-of-body credit removed; honest **82** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `c540ccecb6143380a48619430de6775c0be60f33`.

## Padding review (2026-10-21)



### Out-of-body uncredit (320)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **94** |
| **Out-of-body removed** | **12** |
| **Honest** | **82** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 320 --to 320` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
