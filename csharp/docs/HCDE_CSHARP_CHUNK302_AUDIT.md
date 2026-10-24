# Chunk 302 — addslot Retired (2026-10-23): out-of-body credit removed; honest **86** < **150**.

## Scope

addslot Retired (2026-10-23): out-of-body credit removed; honest **86** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `6c3b1ea2cb01e4e6202fde5b271e56324a9189da`.

## Padding review (2026-10-21)



### Out-of-body uncredit (302)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **130** |
| **Out-of-body removed** | **44** |
| **Honest** | **86** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 302 --to 302` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
