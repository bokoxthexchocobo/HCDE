# Chunk 349 — DeHackEd lookup bounds (trimmed; Modes on chunk 263) Retired (2026-10-23): out-of-body credit removed; honest **9** < **150**.

## Scope

DeHackEd lookup bounds (trimmed; Modes on chunk 263) Retired (2026-10-23): out-of-body credit removed; honest **9** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `ae5c36493598023bcf229ab1bc0f75d33cd2f7d9`.

## Padding review (2026-10-21)



### Out-of-body uncredit (349)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **9** |
| **Out-of-body removed** | **0** |
| **Honest** | **9** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 349 --to 349` (0 hard / 0 soft).

## Seams

VM `PClassActor` creation for DSDHacked indices and live `soundEngine` mutation are not hosted; managed layer mirrors naming and default `DehInfo` scalars.

## Credit rationale

Credits `FindInfoName`/`DehFindSound` index guards, `ReplaceSoundName` ds-prefix, and `Modes[]` section names only.
