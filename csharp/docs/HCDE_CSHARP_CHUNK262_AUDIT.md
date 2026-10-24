# Chunk 262 — ReadChars, ReplaceSpecialChars, skip/strip white, GetLine key split. Retired (2026-10-24): thin managed at HEAD — surviving **0** < **80** (50% of honest **160**).

## Scope

ReadChars, ReplaceSpecialChars, skip/strip white, GetLine key split. Retired (2026-10-24): thin managed at HEAD — surviving **0** < **80** (50% of honest **160**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `72dc19b000644097724c40c949f4b6fe3a648d90`.

## Padding review (2026-10-21)



### Out-of-body uncredit (262)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **161** |
| **Out-of-body removed** | **1** |
| **Honest** | **160** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 262 --to 262` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
