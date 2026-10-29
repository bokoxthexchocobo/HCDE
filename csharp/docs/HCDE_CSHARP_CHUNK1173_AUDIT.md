# Chunk 1173 — P_TryMove crossing checks from p_map.cpp (2359–2726). The successful link and push-line label stay in 2727–2898.

## Scope

P_TryMove crossing checks from p_map.cpp (2359–2726). The successful link and push-line label stay in 2727–2898.

## Native ranges

- `src/playsim/p_map.cpp` **P_TryMove** 2359–2726 (368 lines)

**Credited native LOC:** 368 (tally `nativeLoc` 368).

## Managed

`VmPMapTryMove1173Slice.cs`
Managed LOC (tally): 559.
Conversion commit: `0c8d2bcd7dc15f58da3679f679c733469e2b3cfd`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `P_TryMove` banner (**2359–2367**) | yes |
| Position reject and pass-mobj step (**2368–2421**) | yes |
| Fit, dropoff, floor picture, and bot (**2423–2584**) | yes |
| Fake-floor eyes and MBF step (**2586–2610**) | yes |
| Portal crossing loop (**2612–2726**) | yes |
| Band hull (**2359–2726**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2299–2358** | `CheckForPushSpecial` is chunk **715** |
| **2727–2898** | successful link, push-line label, and the short overload |
| **2899–2991** | `P_CheckMove` is chunk **715** |
| **6101–6522** | radius damage stays off the queue |
| **4566–4690**, **5238–5244**, **5822–5828**, **7272–7391** | short tails stay uncredited |

The move clears float-ok and portal-step, samples Z before an optional floor plane, and sets the in-try-move flag. A failed position collides when the blocker exists and the actor is not a player whose cheats include the predicting bit (bitwise and). No blocker, a player blocker, or a non-player mover pushes. A player stepping onto a non-player pushes when the step, the blocker ceiling portal, or the trace ceiling is too tight. Otherwise a missing pass-mobj flag, or the no-pass compatibility flag, restores Z and returns false. The compiled fly path pushes when the top is above the ceiling and does not rewrite vertical velocity.

Floor and ceiling huggers take the trace plane. The same floor sector then wins over a hugger when the caller passed a floor plane. Without noclip, a gap shorter than the actor pushes before float-ok is set. A teleport skips the must-lower and step tests. A missile that is not a step missile cannot climb. A step that fits is tested at the floor and restored; a step missile stays on that floor, and negative velocity either bounces or becomes zero. Cross-dropoff compatibility clears the dropoff option unless the move came from a scroller. Dropoff option 2 is cleared for a large gap, a missing target, or a target above the drop. Actors that cannot drop restore Z. Actors that are avoiding a dropoff return false without restoring Z. The floor-picture lock, a dangerous bot sector, and an MBF bounce step each return false on their own terms. Eyes are sampled from the original Z. Linked portals move in ascending fraction order, keeping the earlier line when fractions tie. A non-linked portal checks actors at the translated trace point and either restores the previous coordinates or commits the translated trace position, velocity, and angle.

| Managed (`VmPMapTryMove1173Slice.cs`) | Native (`p_map.cpp`) |
|----------------------------------------|----------------------|
| Crossing checks | **2359–2726** |
| Band hull (exclusive slice) | **2359–2726** |

### Chunk **1173** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **368** |
| **Net headline** | **+368** vs post-**1172** |


**Post-review ratio:** 559/368 managed/native ≈ 152% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapTryMove1173SliceTests.Block_StepsAPassableBodyAndRejectsATightFit`
- `VmPMapTryMove1173SliceTests.Dropoff_KeepsTheFloorPictureAndStopsTheBot`
- `VmPMapTryMove1173SliceTests.Portals_LinkInOrderAndRestoreABlockedExit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1173 --to 1173` (0 hard / 0 soft).

## Seams

P_CheckPosition, P_TestMobjZ, portal translation, and the push-line label stay outside this band. This slice decides whether the move fails, pushes, or is ready to link.

## Credit rationale

UNCredited p_map.cpp 2359–2726 (+368). Chunk 715 ends at 2358. 2727–2898 stays for the link. A full tally line scan shows no overlap. Radius damage 6101–6522 is not this band.
