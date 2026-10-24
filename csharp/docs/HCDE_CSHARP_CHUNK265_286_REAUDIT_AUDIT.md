# Chunks 265–286 re-audit (2026-10-20)

Bulk landing credited **3,371** native LOC across 22 chunks while managed helpers were often thin slices with one or two tests per 150-line band. Chunks **281** and **282** duplicated the same `TryLocateWeapon` test; **271** credited `edata`/`loadacs` against a generic bool helper test.

This pass trims each row to symbol ranges whose behavior is implemented in managed code and named in `loc_tally.json` tests. **Removed** tally row **282** (merged into **281** with `ApplyDefaultSlotWeapons` + `LocateWeapon` credits).

## Per-chunk verdict (was → trimmed)

| Chunk | nativeLoc (was) | nativeLoc (trimmed) | Verdict |
|------:|----------------:|--------------------:|---------|
| 265 | 151 | 27 | Actor resolve + numeric bounds/HP/translucency only. |
| 266 | 143 | 38 | Blood color + MBF21 token parse. |
| 267 | 253 | 38 | Initial frame, death sound, MBF bounce remap, translucent alpha. |
| 268 | 75 | 12 | Height pair check + speed rescale. |
| 269 | 154 | 12 | Shotgun flash default tics + Duration. |
| 270 | 160 | 6 | Sprite offset, backpack amounts, select/deselect swap. |
| 271 | 173 | 50 | Unclamped blend, HCDE shadow clamps, optional-bool toggles, skyrotate; dropped untested edata/loadacs. |
| 272 | 150 | 21 | AddWeaponList + single-weapon positions. |
| 273 | 157 | 42 | StripLineComment + SplitPropertyLine. |
| 274 | 150 | 1 | `nojump` table row only. |
| 275 | 155 | 21 | Ammo type give/use + FinishDehPatch AmmoUse clear. |
| 276 | 150 | 25 | PatchSound consume loop + Boom pointer hack. |
| 277 | 150 | 1 | Initial Health key row. |
| 278 | 150 | 5 | Infighting `0xDD` normalize. |
| 279 | 150 | 12 | `MITYPE_SETFLAG` assign branch. |
| 280 | 150 | 3 | Map flag handler name match head. |
| 281 | 150 | 30 | Merged **282**: AddDefaultWeapon + LocateWeapon. |
| 282 | 150 | — | **Removed** — merged into 281. |
| 283 | 150 | 8 | `GetDefaultLevelNum` MAPxx branch. |
| 284 | 150 | 9 | `ApplyBoolProperty` invalid path. |
| 285 | 150 | 11 | PatchMusic line + PLAY sprite guard. |
| 286 | 150 | 9 | Sibling lump path + gravity scale. |

**Post-trim native sum (chunks 265–286, 21 rows):** **385** LOC (was **3,371**).

## Cumulative impact

| Metric | Before re-audit | After re-audit |
|--------|----------------:|---------------:|
| Converted behavior native | 32,899 | 29,913 |
| Recognition behavior native | 5,591 | 5,591 |
| Remaining native | 469,596 | 472,582 |

## Follow-up

Continue whole-function ports: `LoadDehSupp` **WeaponNames** (3748+), `G_ParseMapInfo` **COMPLVL** (2953+), then MAPINFO options 1–1560 by handler.
