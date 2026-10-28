# Chunk 433 — D_InitGame texture progress, startup screen, exec/cvars, TexMan init header Retired (2026-10-24): thin managed at HEAD — surviving **0** < **81** (50% of honest **161**).

## Scope

D_InitGame texture progress, startup screen, exec/cvars, TexMan init header Retired (2026-10-24): thin managed at HEAD — surviving **0** < **81** (50% of honest **161**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `2b3c2a8cd86d19217a781cc90e0c00250802c8c1`.

## Padding review (2026-10-21)



### Out-of-body uncredit (433)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **161** |
| **Out-of-body removed** | **0** |
| **Honest** | **161** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 433 --to 433` (0 hard / 0 soft).

## Seams

`FStartupScreen`, `TexMan.Init`, and `V_Init2` are not hosted.

## Credit rationale

Credits startup kind selection, nostartscreen gating, dedicated delayed-command policy, compatmode argv parsing, and progress budget math from `d_main.cpp`.
