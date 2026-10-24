# Chunk 264 — MAPINFO level scalar options after line 1560 (teamdamage, fog, lightmode). Retired (2026-10-23): out-of-body credit removed; honest **65** < **150**.

## Scope

MAPINFO level scalar options after line 1560 (teamdamage, fog, lightmode). Retired (2026-10-23): out-of-body credit removed; honest **65** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `435d6b44a0f89bb617a35d1d3adb11f1b4e27697`.

## Padding review (2026-10-21)



### Out-of-body uncredit (264)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **65** |
| **Out-of-body removed** | **0** |
| **Honest** | **65** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 264 --to 264` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
