# Session wrap-up audit — 2026-10-20B (PR #78)

Branch: `cursor/native-managed-matrix-3df6`. Continued from `HCDE_CSHARP_SESSION_2026_10_20_AUDIT.md` open items: KEYCONF, `sprites.cpp`, `voxeldef.cpp`, then `g_doomedmap.cpp` MAPINFO DoomEd slice.

## Conversion chunks (this session)

| Chunk | Tally name | Native LOC | Audit |
| --- | --- | ---: | --- |
| 207 | `keyconf-chunk207` | 239 | `HCDE_CSHARP_KEYCONF_CHUNK207_AUDIT.md` |
| 208 | `sprite-frame-install-chunk208` | 196 | `HCDE_CSHARP_SPRITE_FRAME_INSTALL_CHUNK208_AUDIT.md` |
| 209 | `voxeldef-chunk209` | 184 | `HCDE_CSHARP_VOXELDEF_CHUNK209_AUDIT.md` |
| 210 | `doomedmap-mapinfo-chunk210` | 178 | `HCDE_CSHARP_DOOMEDMAP_CHUNK210_AUDIT.md` |

**Native lines credited this session:** **797** (sum of chunk `nativeLoc`).

## Behavior LOC (after chunk 210, `sync_loc_tally`)

| Metric | Value |
| --- | ---: |
| **Cumulative conversion** (native) | **24,169** |
| **Cumulative recognition** (native) | **5,397** |
| **Remaining native** | **478,520** |
| **Cumulative conversion** (managed) | **30,330** (approx.; see matrix) |

Session start cumulative conversion (post chunk 205 reclass): **23,372** → **+797** this session.

## Build / test

- `dotnet build csharp/HCDE.sln -warnaserror`: clean each chunk.
- `dotnet test csharp/HCDE.sln`: **22,678** passed at session end.

## Open priorities (next session)

1. **`options.cpp` `parseOptions`** (149–200) — apply DSDA OPTIONS to level compat flags (extends chunk 185 parser).
2. **`sprites.cpp` `R_InitSpriteDefs`** — texture hash + voxel name spin parsing (312+); seam at `TexMan` / `fileSystem`.
3. **`g_doomedmap.cpp`** — `InitActorNumsFromMapinfo`, spawnable types, remainder of file.
4. **`doomfont.cpp`**, **`umapinfo.cpp`**, **`p_xlat.cpp`** — whole-file candidates with 0% credit in tally.
