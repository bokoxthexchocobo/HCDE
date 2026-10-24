# Chunk 289 — PatchSpriteNames Retired (2026-10-23): out-of-body credit removed; honest **148** < **150**.

## Scope

PatchSpriteNames Retired (2026-10-23): out-of-body credit removed; honest **148** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `39d657a29988af4c8289fbafee5405af05595737`.

## Padding review (2026-10-21)



### Out-of-body uncredit (289)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **2** |
| **Honest** | **148** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 289 --to 289` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
