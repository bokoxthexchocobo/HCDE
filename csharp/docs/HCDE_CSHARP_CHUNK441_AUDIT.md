# Chunk 441 — D_InitGame shader compile, net init, demo/title startup Retired (2026-10-24): thin managed at HEAD — surviving **0** < **75** (50% of honest **150**).

## Scope

D_InitGame shader compile, net init, demo/title startup Retired (2026-10-24): thin managed at HEAD — surviving **0** < **75** (50% of honest **150**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `10374933da4a52488c67ceeeb45e21d974f4f167`.

## Padding review (2026-10-21)



### Out-of-body uncredit (441)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **0** |
| **Honest** | **150** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 441 --to 441` (0 hard / 0 soft).

## Seams

`G_InitNew`, `G_LoadGame`, and `StartCutscene` are not hosted.

## Credit rationale

Credits norun/batch early return, delayed commands, invul map toggle, demo autostart, warp wipe policy, and dedicated stored-warp ignore from `d_main.cpp`.
