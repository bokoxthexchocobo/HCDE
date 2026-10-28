# Chunk 443 — D_DoomMain_Internal buttons, syscallbacks, basewad, help/version exit Retired (2026-10-23): out-of-body credit removed; honest **145** < **150**.

## Scope

D_DoomMain_Internal buttons, syscallbacks, basewad, help/version exit Retired (2026-10-23): out-of-body credit removed; honest **145** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `0af1ec826fb400d33d698081e7ab5a611cb96c97`.

## Padding review (2026-10-21)



### Out-of-body uncredit (443)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **145** |
| **Out-of-body removed** | **0** |
| **Honest** | **145** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 443 --to 443` (0 hard / 0 soft).

## Seams

`D_DoomInit`, `FIWadManager`, and `PClass::StaticInit` bodies are not hosted.

## Credit rationale

Credits NetIDStart, button release locks, sys callback table wiring, forceswnodes, batch logfile policy, version/help early exit, and debug-port gating from `d_main.cpp`.
