# Chunk 273 — EMAPINFO trim/quote/comment/property helpers. Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

EMAPINFO trim/quote/comment/property helpers. Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `0f347fe1d86eaab604bbfd4c8f1c1fa3547f7cc7`.

## Padding review (2026-10-21)



### Out-of-body uncredit (273)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **157** |
| **Out-of-body removed** | **157** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 273 --to 273` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
