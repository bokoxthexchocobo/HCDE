# Chunk 266 — PatchThing blood, drop, crispy, MBF21 through obituaries. Retired (2026-10-23): out-of-body credit removed; honest **143** < **150**.

## Scope

PatchThing blood, drop, crispy, MBF21 through obituaries. Retired (2026-10-23): out-of-body credit removed; honest **143** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `3e2965cb3a2b2cb81b5e6cff53ea7eeec74012e3`.

## Padding review (2026-10-21)



### Out-of-body uncredit (266)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **143** |
| **Out-of-body removed** | **0** |
| **Honest** | **143** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 266 --to 266` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
