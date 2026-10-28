# Chunks 246–260 re-audit (2026-10-20)

Bulk landing credited whole-file bands (~5,153 native LOC) while the PR added ~1,640 lines of managed/tests. Managed code is thin slices and several tally rows reused pre-PR tests (`HcdeEdfThingParser`) or empty commits with no new behavior. This pass trims conversion credit to symbol ranges with named tests that exercise the ported logic.

## Per-chunk verdict (pre-trim → post-trim)

| Chunk | nativeLoc (was) | managedLoc (was) | conversionManagedCommit | Verdict |
|------:|----------------:|-----------------:|-------------------------|---------|
| 246 | 348 | 80 | `4b223c80ab9e2de5c7e19c99677b0c774128303b` | **Trimmed → 88** — credit `MatchesCallerType`, `GetSpriteIndex`, `LoadAltHudStuff` default switch only; not `CallAction` / lump scanner. |
| 247 | 348 | 80 | `4c6ef298cf688f0b1404e158d3af31e6a98a95ab` | **Trimmed → 36** — sprite seed + `SetReplacement` guard; added `VmActorClassRegistryTests`. |
| 248 | 341 | 80 | `22adc45abeafe89d468ab96e2445f932e42f67fd` | **Trimmed → 63** — `DmgFactors::Apply`, summon net bytes, `ParseDamageDefinition` subset; gap 915–920 stays recognition chunk 261. |
| 249 | 350 | 80 | `92e45538c946ba9a13641ebcd8d048474852f933` | **Trimmed → 48** — `NormalizeResourceName` only. |
| 250 | 312 | 80 | `311d9290ee3938d083b2fffc685eaf9c710f9fd7` | **Trimmed → 15** — `RecordIssue` cap (lines 216–230), not 351–662. |
| 251 | 220 | 80 | `8ca7aa91b72b0e6732a51568b621d01182b9c363` | **Trimmed → 66** — `ParseReverbPropertyLine` through `±equalized`. |
| 252 | 250 | 80 | `d5e744b5ab71bf97ae44df4c58d96e5b18cd0f77` | **Removed** — tests pointed at `HcdeEdfThingParser` (chunk 18); no new block parser coverage. |
| 253 | 374 | 80 | `e672be2e0d9808e22722630d5383b0509cc68c55` | **Removed** — tally-only empty commit; same pre-PR thing parser. |
| 254 | 350 | 80 | `a4c1afb0538abf2043c712ea7419f43463b169af` | **Removed** — `VmDehackedGlobals` constant stub; no DEHACKED parse behavior. |
| 255 | 350 | 80 | `c5f657f664a1b7202f9f08eeff37d3434aacadd6` | **Removed** — empty commit, no managed delta. |
| 256 | 350 | 80 | `c5f657f664a1b7202f9f08eeff37d3434aacadd6` | **Removed** — empty commit, no managed delta. |
| 257 | 390 | 80 | `0e50ec83…` (mapinfo header commit) | **Removed** — invented `MapBlockKeys` list; `TryParseInclude` not tied to tested MAPINFO scanner path. |
| 258 | 390 | 80 | (empty tally) | **Removed** — no code. |
| 259 | 390 | 80 | (empty tally) | **Removed** — no code. |
| 260 | 390 | 80 | (empty tally) | **Removed** — no code. |

Post-trim conversion native for **246–251**: **316** native LOC (sum of trimmed `nativeRanges` in `loc_tally.json`).

## Cumulative impact

| Metric | Before re-audit | After re-audit |
|--------|----------------:|---------------:|
| Converted behavior native | 33,709 | 28,872 |
| Recognition behavior native | 5,591 | 5,591 |
| Remaining native | 468,786 | 473,623 |

Chunk **261** (recognition) unchanged.

## Follow-up

Re-credit `d_dehacked.cpp` (from ~1051) and `g_mapinfo.cpp` (past ~1560) only with per-symbol managed ports and dedicated tests—no whole-file tally bands.
