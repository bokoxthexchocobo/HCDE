# Legacy thin-band chunks after 513/514/519 dedupe

Chunks **60**, **270**, **275**, **277**, and **278** are **not** whole-function 150–400 native ports. They come from the re-audit 265–286 “tested ranges only” band and remain **managed inventory** with minimal or zero native credit after overlap removal.

## Gate check (150 native / 50% managed)

| Chunk | Native LOC (after dedupe) | Managed LOC | Meets 150 native? | Meets 50% managed? | Action |
|-------|---------------------------|-------------|-------------------|--------------------|--------|
| **60** `zcc-compare-eq-chunk60` | 27 (emit slices only) | 18 | No | Yes (67%) | **Exempt** thin band; `FxCompareEq::Resolve` body credited on **519** |
| **270** `dehacked-patchsprite-ammo-weapon-chunk270` | 4 | 149 | No | Yes | **Exempt**; weapon slice removed for **514**; sprite/ammo slices retained |
| **275** `dehacked-patchweapon-ammo-chunk275` | 3 (`FinishDehPatch` only) | 79 | No | Yes | **Exempt**; weapon body removed for **514** |
| **277** `dehacked-patchmisc-keys-chunk277` | 0 | 48 | No | N/A | Native cleared; misc keys on **513** |
| **278** `dehacked-patchmisc-tail-chunk278` | 0 | 72 | No | N/A | Native cleared; misc tail on **513** |

**Conclusion:** None of these entries satisfy the post-287 whole-function native floor. They are **grandfathered thin-band** chunks: managed helpers stay in tally; native lines count only non-superseded slices (or zero). No consolidation into synthetic 150-line chunks — that would mis-credit untested native.

**PatchThing thin slices (265–268):** native credit **removed** when whole-function **520/521** land; managed VMs retained as implementation inventory.

Verified: `LocTallyNativeRangesDoNotOverlapAcrossChunks` after tally sync.
