# Chunk 225 — doomfont `FFont` seam (recognition addendum, no new tally ranges)

## FFont blame

| Native region | Classification | Managed surface |
|---------------|----------------|-----------------|
| `doomfont.cpp` `new FFont(...)` inside chunk 216 range 36–92 | **Recognition** (engine font object construction; not ported) | `VmDoomFontPlanner` selects lump/recipe names only |
| `doomfont.cpp` 94–118 BigFont / OriginalBigFont branches | **Conversion** (chunk 224) | `VmDoomFontPlanner.PlanBigFont` / `ResolveBigFont` |
| `V_GetFont`, `FFont::FindFont`, `I_FatalError` fallbacks 120–170 | **Conversion** (chunk 216) | Planner resolve helpers |

`append_tally_chunk.py` rejects overlapping recognition lines already credited as conversion (e.g. 101, 113–117 sit inside chunk 224’s 94–118 conversion band). This audit records the seam without double-crediting `loc_tally.json`.

Verified: `LocTallyNativeRangesDoNotOverlapAcrossChunks`; no chunk 225 entry in `loc_tally.json`.
