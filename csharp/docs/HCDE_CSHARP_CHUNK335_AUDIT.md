# Chunk 335 — EMAPINFO ReportStats and HCDE_ParseEMapInfo scan head Retired (2026-10-23): out-of-body credit removed; honest **28** < **150**.

## Scope

EMAPINFO ReportStats and HCDE_ParseEMapInfo scan head Retired (2026-10-23): out-of-body credit removed; honest **28** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `7121a126f5f33c77306dc68dd2a282bf7efad7c7`.

## Padding review (2026-10-21)



### Out-of-body uncredit (335)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **54** |
| **Out-of-body removed** | **26** |
| **Honest** | **28** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 335 --to 335` (0 hard / 0 soft).

## Seams

`Printf` and `fileSystem.ReadFile` are not re-hosted; the scan loop models line iteration and section headers only.

## Credit rationale

Covers `hcde_emapinfo.cpp` 779–832: `ReportStats` summary formatting/sample notes and the opening of `HCDE_ParseEMapInfo` through the first section-header branch, complementing chunk 295's later document rows.
