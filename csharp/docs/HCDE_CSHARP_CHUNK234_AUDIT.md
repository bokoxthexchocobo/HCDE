# Chunk 234 — Uncredited doomfont gap lines outside HU macro band. Retired (2026-10-23): out-of-body credit removed; honest **3** < **150**.

## Scope

Uncredited doomfont gap lines outside HU macro band. Retired (2026-10-23): out-of-body credit removed; honest **3** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `0c79dfb32f46b52b7d82d1f00c7d504b27bb7582`.

## Padding review (2026-10-21)



### Out-of-body uncredit (234)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **33** |
| **Out-of-body removed** | **30** |
| **Honest** | **3** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 234 --to 234` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
