# Chunk 211 — DSDA OPTIONS defaults, option_list keys, parseOptions apply. Retired (2026-10-23): out-of-body credit removed; honest **52** < **150**.

## Scope

DSDA OPTIONS defaults, option_list keys, parseOptions apply. Retired (2026-10-23): out-of-body credit removed; honest **52** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `9a8178dcf7c77873ff891c75ea36e5366d03133d`.

## Padding review (2026-10-21)



### Out-of-body uncredit (211)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **99** |
| **Out-of-body removed** | **47** |
| **Honest** | **52** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 211 --to 211` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
