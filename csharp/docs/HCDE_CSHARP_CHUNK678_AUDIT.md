# Chunk 678 — Whole `DPolyAction` / move-door thinker serialize+construct, `FPolyObj` ctor, `GetMirror`, `RotatePt`, poly thinker ticks, `EV_MovePolyTo`, and `FPolyMirrorIterator` (`po_man.cpp` 147–280, 290–313, 376–397, 453–474, 482–519, 715–730, 738–741, 918–927, 1781–1793, 1805–1842). Supersedes unbacked `po-man-thinker-moveto-chunk674`. Retired (2026-10-24): `--band` hull overlaps existing credit (po-man-poly-door-chunk671 src/playsim/po_man.cpp 527-625).

## Scope

Whole `DPolyAction` / move-door thinker serialize+construct, `FPolyObj` ctor, `GetMirror`, `RotatePt`, poly thinker ticks, `EV_MovePolyTo`, and `FPolyMirrorIterator` (`po_man.cpp` 147–280, 290–313, 376–397, 453–474, 482–519, 715–730, 738–741, 918–927, 1781–1793, 1805–1842). Supersedes unbacked `po-man-thinker-moveto-chunk674`. Retired (2026-10-24): `--band` hull overlaps existing credit (po-man-poly-door-chunk671 src/playsim/po_man.cpp 527-625).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `3849c4463b63411039b95cfe85dce71d13f54882`.

## Padding review (2026-10-21)

| Managed | Native (`po_man.cpp`) |
|---------|-------------------------|
| `PolyActionSerialize` / `PolyActionConstruct` / `PolyActionOnDestroy` / `PolyActionStop` / `PolyActionSetInterpolation` / `PolyActionStopInterpolation` | `DPolyAction::Serialize` 147–154; `Construct` 156–162; `OnDestroy` 164–173; `Stop` 175–179; `SetInterpolation` 181–184; `StopInterpolation` 186–193 |
| `RotatePolyConstruct` | `DRotatePoly::Construct` 203–206 |
| `MovePolySerialize` / `MovePolyConstruct` | `DMovePoly::Serialize` 216–221; `Construct` 223–228 |
| `MovePolyToSerialize` / `MovePolyToConstruct` | `DMovePolyTo::Serialize` 239–244; `Construct` 246–250 |
| `PolyDoorSerialize` / `PolyDoorConstruct` | `DPolyDoor::Serialize` 260–269; `Construct` 271–280 |
| `FPolyObjConstruct` / `FPolyObjGetMirror` | `FPolyObj::FPolyObj` 715–730; `GetMirror` 738–741 |
| `RotatePt` | `RotatePt` 918–927 |
| `RotatePolyTick` | `DRotatePoly::Tick` 290–313 |
| `MovePolyTick` | `DMovePoly::Tick` 376–397 |
| `MovePolyToTick` | `DMovePolyTo::Tick` 453–474 |
| `EvMovePolyTo` | `EV_MovePolyTo` 482–519 |
| `VmPoManMirrorChain.Enumerate` | `FPolyMirrorIterator::FPolyMirrorIterator` 1781–1793; `FPolyMirrorIterator::NextMirror` 1805–1842 |

### Out-of-body uncredit (678)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **276** |
| **Out-of-body removed** | **0** |
| **Honest** | **276** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 678 --to 678` (0 hard / 0 soft).

## Seams

`RotatePolyobj`, `MovePolyobj`, thinker spawn, sound, and interpolation objects remain native.

## Credit rationale

Thinker lifecycle and tick bands aligned to ported managed helpers after shrinking chunk 674.
