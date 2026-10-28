# Chunk 409 — RenameSprites IWAD tail, RenameNerve, FixMacHexen Retired (2026-10-24): thin managed at HEAD — surviving **0** < **83** (50% of honest **165**).

## Scope

RenameSprites IWAD tail, RenameNerve, FixMacHexen Retired (2026-10-24): thin managed at HEAD — surviving **0** < **83** (50% of honest **165**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `e3b8f5dd0685af9f36fe8978d12885d10d638f73`.

## Padding review (2026-10-21)



### Out-of-body uncredit (409)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **165** |
| **Out-of-body removed** | **0** |
| **Honest** | **165** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 409 --to 409` (0 hard / 0 soft).

## Seams

Filesystem mutation, MD5 IO, and `deletelumps` tables are not hosted.

## Credit rationale

Credits the `RenameSprites` BLOD/global-lump tail, `RenameNerve`, and `FixMacHexen` from `d_main.cpp`.
