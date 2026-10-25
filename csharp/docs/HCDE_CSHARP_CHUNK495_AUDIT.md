# Chunk 495 — P_AdjustFloorCeil, above/below intersectors, P_DoCrunch compat head Retired (2026-10-24): `--band` hull overlaps existing credit (p-map-crush-tail-decal-activate-chunk499 src/playsim/p_map.cpp 6679-6685).

## Scope

P_AdjustFloorCeil, above/below intersectors, P_DoCrunch compat head Retired (2026-10-24): `--band` hull overlaps existing credit (p-map-crush-tail-decal-activate-chunk499 src/playsim/p_map.cpp 6679-6685).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `288ff231a733aef7eda9b0a9caa31a066bb51fa1`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `SkipNonSolidThing` / `ApplyCrushDamageOnCadence` / `ShouldSkipIteratorCandidate` / `InvulnerableOrDormant` | `P_ChangeSector` 6482–6511 |
| `PassMobjMonsterCheck` / `BridgeSkipsFloorSync` / `RestorePassmobjFlag` / `FromPMoveMonster` / `OrPassmobjFlags` | `P_AdjustFloorCeil` 6523–6566 (6528: `if ((thing->flags2 & MF2_PASSMOBJ) && (thing->flags3 & MF3_ISMONSTER))`; 6533: `if (cpos->movemidtex)`; 6538: `thing->flags2 |= MF2_PASSMOBJ;`) |
| `SkipNoclip` / `RequiresSolid` / `BlockDistOutOfRange` / `SkipSpecialPickup` / `SkipCorpseUnlessIce` / `SkipAlwaysBlockingPair` / `AboveIntersection` | `P_FindAboveIntersectors` 6574–6622 (6576: `if (actor->flags & MF_NOCLIP)`; 6577: `return;`; 6579: `if (!(actor->flags & MF_SOLID))`) |
| `BelowIntersection` / `HideBloodWhenTypeLow` | `P_FindBelowIntersectors` 6630–6678 (6632: `if (actor->flags & MF_NOCLIP)`; 6633: `return;`; 6635: `if (!(actor->flags & MF_SOLID))`) |
| `CrusherCompatSkipsCorpse` / `CrushRequiresGrind` / `PlayCrushSoundWhenIdle` | `P_DoCrunch` 6686–6700 (6688: `if (!(thing && thing->CallGrind(true) && cpos)) return;`; 6690: `if ((thing->Level->i_compatflags2 & COMPATF2_DR_CRUSHER) &&`; 6699: `return;`) |

### Out-of-body uncredit (495)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **187** |
| **Out-of-body removed** | **30** |
| **Honest** | **157** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 495 --to 495` (0 hard / 0 soft).

## Seams

`P_CheckPosition`, `P_FindFloorCeiling`, `FMultiBlockThingsIterator`, `P_DamageMobj`, and blood spawn are not hosted.

## Credit rationale

Credits 3DMidTex passmobj, bridge spawn-pos floor sync, intersector filtering, crush compat corpse skip, and crush damage cadence from `p_map.cpp` (6482–6700).
