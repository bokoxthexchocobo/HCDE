# Chunk 246 — Re-audit: FState::MatchesCallerType, GetSpriteIndex, LoadAltHud default switch only. Retired (2026-10-23): out-of-body credit removed; honest **88** < **150**.

## Scope

Re-audit: FState::MatchesCallerType, GetSpriteIndex, LoadAltHud default switch only. Retired (2026-10-23): out-of-body credit removed; honest **88** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `95747dc953402c71b3a41e8cd4107c04f0c4f237`.

## Padding review (2026-10-21)



### Out-of-body uncredit (246)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **88** |
| **Out-of-body removed** | **0** |
| **Honest** | **88** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 246 --to 246` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
