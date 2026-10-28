# Chunk 347 — ParseLookupName lookup/$ tokens (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **17** < **150**.

## Scope

ParseLookupName lookup/$ tokens (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **17** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `a0891b3eca6236ce570d3026400fc053b903a2f3`.

## Padding review (2026-10-21)



### Out-of-body uncredit (347)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **17** |
| **Out-of-body removed** | **0** |
| **Honest** | **17** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 347 --to 347` (0 hard / 0 soft).

## Seams

Scanner `sc` token stream is abstracted; HCDE mod-compat trailing-comma tolerance is not re-hosted in full.

## Credit rationale

Credits `FMapInfoParser::ParseLookupName` lookup and `$` string-table prefix handling only.
