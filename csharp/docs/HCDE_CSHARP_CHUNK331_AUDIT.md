# Chunk 331 — ParseMapDefinition MITYPE flag apply arms (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **22** < **150**.

## Scope

ParseMapDefinition MITYPE flag apply arms (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **22** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `30e6c263139e1e6d7f61e4b71ed6473786bd4700`.

## Padding review (2026-10-21)



### Out-of-body uncredit (331)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **22** |
| **Out-of-body removed** | **0** |
| **Honest** | **22** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 331 --to 331` (0 hard / 0 soft).

## Seams

`FMapInfoParser` scanner, `AutoSegs::MapInfoOptions`, and unknown-property recovery are not hosted; switch arms are pure flag-word mutations on `VmMapInfoLevelFlags`.

## Credit rationale

Credits `MITYPE_SETFLAG2`, `MITYPE_SCFLAGS2`, and `MITYPE_CLRCOMPATFLAG` switch arms only.
