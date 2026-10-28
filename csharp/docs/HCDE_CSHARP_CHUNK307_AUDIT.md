# Chunk 307 — LoadDehSupp Aliases brace list Retired (2026-10-23): out-of-body credit removed; honest **15** < **150**.

## Scope

LoadDehSupp Aliases brace list Retired (2026-10-23): out-of-body credit removed; honest **15** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `eca040db9de2fd73aef5915e7664659cefe6af76`.

## Padding review (2026-10-21)



### Out-of-body uncredit (307)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **16** |
| **Out-of-body removed** | **1** |
| **Honest** | **15** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 307 --to 307` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
