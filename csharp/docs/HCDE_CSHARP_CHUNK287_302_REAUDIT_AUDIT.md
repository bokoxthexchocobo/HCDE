# Chunks 287–302 re-audit (2026-10-20)

Bulk landing credited **2,302** native LOC across 16 chunks while the branch added ~**1,585** lines (code + tests + docs). Managed helpers were thin 150-line band slices with one-liner tests; chunks **290–291** duplicated the same include test; **297–299** split one episode parser file into three nominal 150-line bands.

This pass trims each row to symbol ranges whose behavior is implemented in managed code and named in `loc_tally.json` tests. Sub-150 chunks are intentional. **Removed** tally rows **291**, **298**, and **299** (merged into **290** / **297**).

## Per-chunk verdict (was → trimmed)

| Chunk | nativeLoc (was) | managedLoc (was) | conversionManagedCommit | Verdict |
|------:|----------------:|-----------------:|-------------------------|---------|
| 287 | 150 | 24 | `c4d9001f4848d6dafeaf3916fcd6ba84551d924b` | **→ 7** — Doom→`MUSIC_DOOM` hack (3000–3006); sprite replace stays on chunk **285**. |
| 288 | 150 | 38 | `cdd4fbae2646ddfd181e263e12fdf9657a5d688e` | **→ 16** — continuation fold (3043–3056) + `GOTREDSKUL` alias (3063–3064). |
| 289 | 150 | 36 | `ce1401529f60012c504ae7972c4f910280c61062` | **→ 16** — sprite name length + table grow (3101–3116). |
| 290 | 150 | 40 | `24224a5ff436e2a5d7ae2df67a8264842a99455a` | **→ 21** — `notext`, quoted path, directory combine (3140–3181). |
| 291 | 121 | 40 | `36a666762a99992f5540845052b690d11b59b82f` | **Removed** — duplicate include credit; merged into 290. |
| 292 | 150 | 18 | `f5131a9454e792e38fb8d9f6209f0d98e7728ca7` | **→ 5** — `A_Mushroom` vrange/hrange defaults (791–795). |
| 293 | 150 | 16 | `d71fc5180bffd0a87b42358d14b2218680c3e033` | **→ 3** — turn angle (817) + scratch sound index (831–832). |
| 294 | 150 | 20 | `31f66b4267504076eea265623a9dd897c872823e` | **→ 15** — skydelta scale, unevenlight clear, `-NOFLAT-` SkyPic2 guard. |
| 295 | 98 | 28 | `b095675caf94ecc94dd28aefcd81252826d2996c` | **→ 14** — `[map]` section header parse (833–846). |
| 296 | 153 | 44 | `37b0385b15ffc2670453755078d40a2f8f5e0173` | **→ 9** — `defaultenvironment` pack (1617–1620) + CD `EatNext` rows (2096–2100); compat rows stay on chunk **274**. |
| 297 | 150 | 30 | `84ea0afb2166c6af147e43f1c08090551870bd7a` | **→ 48** — merged episode helpers (2476–2591); absorbs old 298–299. |
| 298 | 150 | 30 | `070890dbcdf190137079dbfba68d2aaaf0023ca8` | **Removed** — merged into 297. |
| 299 | 150 | 35 | `613a4d4e20a43b41fc9f8b06330eed0f12fd76b8` | **Removed** — merged into 297. |
| 300 | 150 | 12 | `a10c1fa0cfb18e9845d6cb30929973f18145406e` | **→ 4** — `setslot` slot bounds (545–548). |
| 301 | 150 | 18 | `578114b1c6593a5e0641f39409b0823dfbaf23e9` | **→ 15** — `PrintSettings` loop (526–540). |
| 302 | 130 | 14 | `a04301d723b484caea74324eebbff9210994dc7a` | **→ 5** — `addslot` argc validation (617–621). |

**Post-trim native sum (chunks 287–302, 13 rows):** **178** LOC (was **2,302**).

## Cumulative impact

| Metric | Before re-audit | After re-audit |
|--------|----------------:|---------------:|
| Converted behavior native | 34,926 | 32,802 |
| Recognition behavior native | 5,591 | 5,591 |
| Remaining native | 467,569 | 469,693 |

## Follow-up

Re-credit `d_dehacked.cpp` (3725+), `g_mapinfo.cpp` (2871+ and 1–1560) only with **whole-function** managed ports and tests that exercise each credited range.
