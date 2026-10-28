# Chunk 435 — D_InitGame teaminfo through late V_Init2 Retired (2026-10-24): thin managed at HEAD — surviving **0** < **76** (50% of honest **151**).

## Scope

D_InitGame teaminfo through late V_Init2 Retired (2026-10-24): thin managed at HEAD — surviving **0** < **76** (50% of honest **151**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `67ae73f9c97dee60d048d6f47844d0a24f0dbd90`.

## Padding review (2026-10-21)



### Out-of-body uncredit (435)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **151** |
| **Out-of-body removed** | **0** |
| **Honest** | **151** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 435 --to 435` (0 hard / 0 soft).

## Seams

`PClassActor::StaticInit`, `R_Init`, and `D_CheckNetGame` are not hosted.

## Credit rationale

Credits player-class fatal gate, DEH/BEX patch flow, bot argv wiring, playloop init gating, and late video init policy from `d_main.cpp`.
