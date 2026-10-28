# Chunk 428 — PatchTextures, CheckForHacks, FixWideStatusBar, GC roots, system UI toggles Retired (2026-10-23): out-of-body credit removed; honest **145** < **150**.

## Scope

PatchTextures, CheckForHacks, FixWideStatusBar, GC roots, system UI toggles Retired (2026-10-23): out-of-body credit removed; honest **145** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `6a5e136191c88a69851c55efc91ccbe205b8f296`.

## Padding review (2026-10-21)



### Out-of-body uncredit (428)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **153** |
| **Out-of-body removed** | **8** |
| **Honest** | **145** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 428 --to 428` (0 hard / 0 soft).

## Seams

`TexMan`, `GC::Mark`, and `StatusBar` mutation are not hosted.

## Credit rationale

Credits Hexen BLANK patch, Heretic/Doom texture build hacks, wide status bar offsets, and game-root GC marking from `d_main.cpp`.
