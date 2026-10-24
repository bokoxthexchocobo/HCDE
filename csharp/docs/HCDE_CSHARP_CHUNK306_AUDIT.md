# Chunk 306 — G_ParseMapInfo COMPLVL compatibility presets Retired (2026-10-23): out-of-body credit removed; honest **39** < **150**.

## Scope

G_ParseMapInfo COMPLVL compatibility presets Retired (2026-10-23): out-of-body credit removed; honest **39** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `6f09ab4a59fc473f8f83bddf253e4b42175b6ea1`.

## Padding review (2026-10-21)



### Out-of-body uncredit (306)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **39** |
| **Out-of-body removed** | **0** |
| **Honest** | **39** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 306 --to 306` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
