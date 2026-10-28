# Chunk 325 — EMAPINFO normalize and SplitValueList/ParseBool (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

EMAPINFO normalize and SplitValueList/ParseBool (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `7da0b496ffbbfb02d4ce2f8076b7e37159275e63`.

## Padding review (2026-10-21)



### Out-of-body uncredit (325)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **42** |
| **Out-of-body removed** | **42** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 325 --to 325` (0 hard / 0 soft).

## Seams

Anonymous-namespace stats tables and `wadlevelinfos` mutation are not hosted; `RememberEMapInfoMap` is a managed set and strict parsers reject trailing junk like `strtol`/`strtod` guards.

## Credit rationale

Credits `NormalizeResourceName`, `SplitValueList`, and empty-true `ParseBool` — not the full primitive block through `SetFlag`/`ParseDouble`.
