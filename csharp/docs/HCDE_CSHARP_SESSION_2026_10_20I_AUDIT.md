# Session audit 2026-10-20 (continuation I)

## Header drift (−160 native)

Reconciled earlier on branch: three conversion chunks without explicit `"bucket": "conversion"` (`p-map-radius-thrust-chunk2` 58, `p-enemy-move-blocked-chunk2` 21, `p-mobj-missile-spawn-live` 81) made spreadsheet sums miss header `convertedBehaviorNative`. Fixed via explicit buckets + `sync_loc_tally.py`. Documented in `HCDE_CSHARP_CHUNK267_268_REBALANCE_ADDENDUM.md`. New test `LocTallyConversionChunksDeclareExplicitBucket` guards recurrence.

## Chunks landed (275–286)

| Chunk | Native focus | Managed types |
|------:|--------------|---------------|
| 275 | PatchWeapon tail | `VmDehackedPatchWeaponAmmo` |
| 276 | PatchSound, PatchPointer | `VmDehackedPatchSound`, `VmDehackedPatchPointer` |
| 277 | PatchMisc keys | `VmDehackedPatchMisc` |
| 278 | PatchMisc powerups/armor | `VmDehackedPatchMiscPowerups`, `VmDehackedPatchMiscArmor` |
| 279 | ParseMapDefinition flags | `VmMapInfoFlagApply` |
| 280 | MAPINFO options | `VmMapInfoOptionDispatch` |
| 281–282 | FWeaponSlots | `VmWeaponSlotsRuntime` |
| 283 | EMAPINFO primitives | `VmEMapInfoParsePrimitives` |
| 284 | EMAPINFO apply helpers | `VmEMapInfoApplyHelpers` |
| 285 | PatchMusic / sprite replace | `VmDehackedPatchMusic`, `VmDehackedReplaceSprite` |
| 286 | EMAPINFO lump resolve | `VmEMapInfoLumpResolve` |

## Behavior LOC (after `sync_loc_tally`)

| Metric | Value |
|--------|------:|
| `convertedBehaviorNative` | 32,624 |
| `recognitionBehaviorNative` | 5,591 |
| `remainingNative` | 469,871 |

## HEAD

`e04ef3179f65c9a2456a536cdc87483a3a5c399b`
