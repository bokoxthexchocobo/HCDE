# Chunk 295 — EMAPINFO scan Retired (2026-10-23): out-of-body credit removed; honest **94** < **150**.

## Scope

EMAPINFO scan Retired (2026-10-23): out-of-body credit removed; honest **94** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `d91570c42f1262b30897aff8418afc8d8fae21e4`.

## Padding review (2026-10-21)



### Out-of-body uncredit (295)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **98** |
| **Out-of-body removed** | **4** |
| **Honest** | **94** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 295 --to 295` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
