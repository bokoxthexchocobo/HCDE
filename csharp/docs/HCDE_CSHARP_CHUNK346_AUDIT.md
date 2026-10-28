# Chunk 346 — LookupLevelName strip and brace format (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **57** < **150**.

## Scope

LookupLevelName strip and brace format (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **57** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `dc9fb59352ab7b002a17c452f8109c5df661d67f`.

## Padding review (2026-10-21)



### Out-of-body uncredit (346)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **57** |
| **Out-of-body removed** | **0** |
| **Honest** | **57** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 346 --to 346` (0 hard / 0 soft).

## Seams

Inventory-based `CheckLevelRedirect` still depends on engine actors; this chunk ports name stripping and new-vs-old brace parsing only.

## Credit rationale

Credits localized title header stripping and `FMapInfoParser` open/close brace plus unexpected-EOF guard.
