# Chunk 229 — DEFINE_GLOBAL/ACTION_FUNCTION exports for FTeam; accessors are chunk 228. Retired (2026-10-23): out-of-body credit removed; honest **74** < **150**.

## Scope

DEFINE_GLOBAL/ACTION_FUNCTION exports for FTeam; accessors are chunk 228. Retired (2026-10-23): out-of-body credit removed; honest **74** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `1d18cf6ca4b9a8ec4f6d0398a3dfb3e7b3f1f17c`.

## Padding review (2026-10-21)



### Out-of-body uncredit (229)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **87** |
| **Out-of-body removed** | **13** |
| **Honest** | **74** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 229 --to 229` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
