# Chunks 207–245 re-audit (2026-10-20)

Automated pass via `csharp/tools/reaudit_chunks.py audit --from 207 --to 245`. Unlike 265–286 / 287–302, these rows use **multiple named tests per range** and whole-function style ports; diff ratios were already healthy.

## Result

| Check | Outcome |
|-------|---------|
| Tally rows in range | **38** (207–244 + **227b**; no row **225** / **245** in tally) |
| Native LOC sum | **5,372** (unchanged) |
| Hard audit failures | **0** after fix |
| Applied trims | **None** (22 soft `thin_test_band` warnings — kept; integration tests cover full managed ports) |

## Fix applied

| Chunk | Change |
|------:|--------|
| **238** | `Lock globals` test `VmLockDefKeywords.LockKeywords_IncludesAny` → **`VmActorKeyCheckTests.LockKeywords_IncludesAny`** (test lives on `VmActorKeyCheckTests`). |

## Out of tally (documented, not trimmed)

| Artifact | Note |
|----------|------|
| `doomfont-ffont-seam-chunk225` | Recognition stub in `tools/chunk225.json` only (no tests); not merged into tally. |
| `umapinfo-remainder-chunk245` | Conversion JSON prepared; never appended (partial overlap with 215–222 credits). |

## Cumulative impact

**No change** to `convertedBehaviorNative` / `recognitionBehaviorNative` / `remainingNative` from this re-audit (test-name fix only).

## Tooling

- `reaudit_chunks.py audit --from N --to M` — hard vs soft findings
- `reaudit_chunks.py apply-fixes` — applies `FIX_TEST_RENAMES` / future trim dict (empty trim set for 207–245)
