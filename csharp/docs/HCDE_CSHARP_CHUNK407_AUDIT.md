# Chunk 407 — Startup progress UI, CheckDefaultSkill, RenameSprites policy

## Scope

Startup progress UI, CheckDefaultSkill, RenameSprites policy

## Native ranges

- `src/d_main.cpp` **StartupStatusLine** 2802–2842 (41 lines)
- `src/d_main.cpp` **CheckDefaultSkill** 2893–2913 (21 lines)
- `src/d_main.cpp` **RenameSprites** 2926–3057 (132 lines)

**Credited native LOC:** 194 (tally `nativeLoc` 194).

## Managed

`VmDMainCheckDefaultSkill.cs`, `VmDMainRenameSpritesPolicy.cs`, `VmDMainStartupProgress.cs`, `VmDMainStartupProgressTests.cs`
Managed LOC (tally): 98.
Conversion commit: `5f61cf6d744f21c752241b5187081a750bcdd70a`.

## Tests

- `VmDMainStartupProgressTests.ShouldSkipStartupUi_WhenBatchRun`
- `VmDMainStartupProgressTests.ShouldOverrideWithCmdlineSkill_WhenSet`
- `VmDMainStartupProgressTests.ResolveRenameTable_Heretic_HasPairs`
- `VmDMainStartupProgressTests.ShouldRenameSprites_FalseForDoom`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 407 --to 407` (0 hard / 0 soft).

## Seams

`I_ShowStartupStatus`, `gameskill` cvar, and filesystem rename IO are not hosted.

## Credit rationale

Credits startup progress helpers, `CheckDefaultSkill`, and the `RenameSprites` gametype rename tables from `d_main.cpp`.
