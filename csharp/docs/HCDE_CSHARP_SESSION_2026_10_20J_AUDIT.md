# Session audit 2026-10-20 (continuation J)

## Run start tally

| Field | Value |
|-------|------:|
| `convertedBehaviorNative` | 32,624 |
| `recognitionBehaviorNative` | 5,591 |
| `remainingNative` | 469,871 |

## Chunks landed (287–302)

| Chunk | Native focus | Managed |
|------:|--------------|---------|
| 287 | `d_dehacked.cpp` PatchText head | `VmDehackedPatchText` |
| 288 | PatchStrings / sprite table | `VmDehackedPatchStrings`, `VmDehackedPatchSpriteTable` |
| 289–291 | DoInclude / PatchSpriteNames | `VmDehackedIncludeDirective` |
| 292–293 | MBF emitter defaults | `VmDehackedMbfParamDefaults`, `VmDehackedMbfParamEmitters` |
| 294–295 | EMAPINFO property tail + parse scan | `VmEMapInfoPropertyTail`, `VmEMapInfoDocumentScanner` |
| 296 | MAPINFO scalars + intro + compat tail | `VmMapInfoScalarOptionsBlock`, `VmMapInfoFlagHandlersCompat` |
| 297–299 | `ParseEpisodeInfo` | `VmMapInfoEpisodeParser` |
| 300–302 | `a_weapons.cpp` setslot/print/addslot | `VmWeaponSlotsSetSlot`, `VmWeaponSlotsPrintSettings`, `VmWeaponSlotsAddSlot` |

**Native this run:** 32,624 → **34,926** (**+2,302** native LOC credited).

## Run end tally

| Field | Value |
|-------|------:|
| `convertedBehaviorNative` | **34,926** |
| `recognitionBehaviorNative` | 5,591 |
| `remainingNative` | **467,569** |

## HEAD

`4c4c432c1374f7c4bfb53453801345906f1f564f`

## Ratio note

Several chunks carry **thin managedLoc** versus 150-line native blocks (dehacked include/MBF and weapon CCMD slices). Next pass should deepen ports or narrow native ranges where helpers are still skeletal.
