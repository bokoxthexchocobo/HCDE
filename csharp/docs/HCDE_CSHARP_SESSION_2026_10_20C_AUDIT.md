# Session wrap-up audit — 2026-10-20C (PR #78)

Branch: `cursor/native-managed-matrix-3df6`. Continuation after chunks 207–210: **options.cpp**, **sprites.cpp** init hash, **g_doomedmap.cpp** resolve/spawn map, **umapinfo.cpp**, **doomfont.cpp**, **p_xlat.cpp**, **g_skill.cpp**.

## Conversion chunks (211–220)

| Chunk | Tally name | Native LOC | Focus |
| --- | --- | ---: | --- |
| 211 | `dsda-options-parse-chunk211` | 99 | DSDA `parseOptions` + full option key table |
| 212 | `sprite-init-hash-chunk212` | 61 | `R_InitSpriteDefs` texture hash + voxel spin name parse |
| 213 | `doomedmap-resolve-chunk213` | 48 | `InitActorNumsFromMapinfo` / `P_GetSpawnableType` |
| 214 | `doomedmap-spawnmap-chunk214` | 77 | `ParseSpawnMap` / `InitClassMap` |
| 215 | `umapinfo-token-chunk215` | 58 | `ParseMultiString`, lump name, player action option |
| 216 | `doomfont-planner-chunk216` | 112 | `InitDoomFonts` selection planner + HU constants |
| 217 | `p-xlat-flags-chunk217` | 58 | `TranslateLineDef` flag passes + zero special |
| 218 | `umapinfo-standard-chunk218` | 62 | `ParseStandardProperty` subset |
| 219 | `g-skill-mapinfo-chunk219` | 57 | `ParseSkill` defaults + factor options |
| 220 | `p-xlat-simple-chunk220` | 60 | Simple line translation branch |

**Native lines credited (chunks 211–220):** **711** (several chunks under the 150 LOC band where native surface is small; no stub credit).

Audits: `csharp/docs/HCDE_CSHARP_CHUNK211_AUDIT.md` … `CHUNK220_AUDIT.md`.

## Behavior LOC (`sync_loc_tally`)

| Metric | Value |
| --- | ---: |
| **Cumulative conversion** (native) | **24,880** |
| **Cumulative recognition** (native) | **5,397** |
| **Remaining native** | **477,809** |
| **Cumulative conversion** (managed) | **31,021** |

Delta conversion native since post–chunk-210 baseline (24,169): **+711**.

## Build / test

- `dotnet build csharp/HCDE.sln -warnaserror`: clean after each chunk.
- `dotnet test csharp/HCDE.sln`: **22,692** passed at session end.

## Open priorities

1. **`R_InitSpriteDefs`** main sprite lump scan (post-hash loops in `sprites.cpp`).
2. **`g_doomedmap.cpp`** spawnable / conversation ID init (`InitSpawnablesFromMapinfo`).
3. **`umapinfo.cpp`** map block parser and boss actions.
4. **`p_xlat.cpp`** boomish/general line translation tails.
5. **`doomfont.cpp`** full `FFont` construction (recognition vs new conversion after blame).
