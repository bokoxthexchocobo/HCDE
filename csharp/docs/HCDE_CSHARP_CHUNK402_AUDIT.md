# Chunk 402 — D_DoomInit, AddAutoloadFiles, CheckCmdLine dmflags Retired (2026-10-24): thin managed at HEAD — surviving **0** < **90** (50% of honest **179**).

## Scope

D_DoomInit, AddAutoloadFiles, CheckCmdLine dmflags Retired (2026-10-24): thin managed at HEAD — surviving **0** < **90** (50% of honest **179**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `62ef4fc1d061d3b8a6b1b1c518456bb10eb098f6`.

## Padding review (2026-10-21)



### Out-of-body uncredit (402)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **179** |
| **Out-of-body removed** | **0** |
| **Honest** | **179** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 402 --to 402` (0 hard / 0 soft).

## Seams

`Args`, `M_LoadDefaults`, and `Net_TraceSet*` are not hosted.

## Credit rationale

Credits `D_DoomInit`, `AddAutoloadFiles`, and the opening `CheckCmdLine` dmflags/gametype logic from `d_main.cpp`.
