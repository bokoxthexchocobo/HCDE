# Chunk 304 — LoadDehSupp AmmoNames brace list Retired (2026-10-23): out-of-body credit removed; honest **22** < **150**.

## Scope

LoadDehSupp AmmoNames brace list Retired (2026-10-23): out-of-body credit removed; honest **22** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `16c9b248ec1bbff2958fed002dc3e8de5ba0c295`.

## Padding review (2026-10-21)



### Out-of-body uncredit (304)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **22** |
| **Out-of-body removed** | **0** |
| **Honest** | **22** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 304 --to 304` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
