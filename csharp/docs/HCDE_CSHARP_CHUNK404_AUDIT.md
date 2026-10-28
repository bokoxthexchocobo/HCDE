# Chunk 404 — CheckCmdLine warp/skill/map and CheckEpisodeCmd Retired (2026-10-24): `--band` hull overlaps existing credit (d-main-startup-rename-chunk407 src/d_main.cpp 2802-2842).

## Scope

CheckCmdLine warp/skill/map and CheckEpisodeCmd Retired (2026-10-24): `--band` hull overlaps existing credit (d-main-startup-rename-chunk407 src/d_main.cpp 2802-2842).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `2ecb04d068381205b182e9375efab98f8443679c`.

## Padding review (2026-10-21)



### Out-of-body uncredit (404)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **155** |
| **Out-of-body removed** | **0** |
| **Honest** | **155** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 404 --to 404` (0 hard / 0 soft).

## Seams

`Args`, `P_CheckMapData`, and `StartScreen` are not hosted.

## Credit rationale

Credits the `CheckCmdLine` map/skill/`+map` tail and `CheckEpisodeCmd` from `d_main.cpp`.
