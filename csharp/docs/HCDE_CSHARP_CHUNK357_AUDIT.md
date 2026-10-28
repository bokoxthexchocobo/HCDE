# Chunk 357 — Sector SetTexture missing flat warnings and SummarizeMissingTextures Retired (2026-10-23): out-of-body credit removed; honest **41** < **150**.

## Scope

Sector SetTexture missing flat warnings and SummarizeMissingTextures Retired (2026-10-23): out-of-body credit removed; honest **41** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `48e44225fd6304358aba382a7cd6552bd3c9124c`.

## Padding review (2026-10-21)



### Out-of-body uncredit (357)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **41** |
| **Out-of-body removed** | **0** |
| **Honest** | **41** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 357 --to 357` (0 hard / 0 soft).

## Seams

`TexMan` and `Printf` are not hosted; messages and warn caps mirror native policy.

## Credit rationale

Credits sector `SetTexture` truncate/warn/default path and the missing-texture summary tail loop.
