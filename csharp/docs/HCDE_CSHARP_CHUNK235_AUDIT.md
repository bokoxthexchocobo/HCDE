# Chunk 235 — GameNames and border preset tables (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

GameNames and border preset tables (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `6008fb66fad591703b028d3758ac970603ca0ba3`.

## Padding review (2026-10-21)



### Out-of-body uncredit (235)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **39** |
| **Out-of-body removed** | **39** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 235 --to 235` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Credits `GameNames[]` and border preset lookup — not the `GAMEINFOKEY` macro catalog or file banner.
