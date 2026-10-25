# Chunk 430 — FileSystemPrintf, D_InitGame RNG/exec/WAD mount header Retired (2026-10-24): thin managed at HEAD — surviving **0** < **82** (50% of honest **163**).

## Scope

FileSystemPrintf, D_InitGame RNG/exec/WAD mount header Retired (2026-10-24): thin managed at HEAD — surviving **0** < **82** (50% of honest **163**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `0cff1e5b49fba90c0d49a0ee99ec8a6269adb738`.

## Padding review (2026-10-21)



### Out-of-body uncredit (430)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **163** |
| **Out-of-body removed** | **0** |
| **Honest** | **163** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 430 --to 430` (0 hard / 0 soft).

## Seams

`fileSystem.InitMultipleFiles`, `GameConfig`, and `I_FatalError` are not hosted.

## Credit rationale

Credits FS printf routing, dedicated-server framebuffer skip, RNG seed selection, exec list chaining, and lump filter blanket setup from `d_main.cpp`.
