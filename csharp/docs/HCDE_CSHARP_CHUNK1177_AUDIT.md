# Chunk 1177 — Poly thinker registration, lifecycle, rotate tick, and EV_RotatePoly from po_man.cpp (1–368).

## Scope

Poly thinker registration, lifecycle, rotate tick, and EV_RotatePoly from po_man.cpp (1–368).

## Native ranges

- `src/playsim/po_man.cpp` **EV_RotatePoly** 1–368 (368 lines)

**Credited native LOC:** 368 (tally `nativeLoc` 368).

## Managed

`VmPoManRotate1177Slice.cs`
Managed LOC (tally): 410.
Conversion commit: `0b666fed8fba7655979d1e8bd4e9581a1fb0b41f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Class registration and DPolyAction lifecycle (**141–193**) | yes |
| DRotatePoly, DMovePoly, DMovePolyTo, DPolyDoor construct and serialize (**201–280**) | yes |
| DRotatePoly::Tick (**290–313**) | yes |
| EV_RotatePoly (**322–368**) | yes |
| Band hull (**1–368**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **369–526** | move and move-to thinkers are the next slice |
| **527–625** | DPolyDoor::Tick is chunk **671** |
| **1781–1842** | mirror iterator body stays a host list |

Registration records DPolyAction with its interpolation pointer and the four derived classes. Construct stores the poly, zeroes speed and distance, and takes an interpolation reference. Destroy clears specialdata only when it still points at this thinker, drops that reference, and chains the base destroy. Stop stops the sequence and then destroys.

The rotate tick returns when the poly is missing. A non-perpetual distance shorter than the absolute speed clamps the signed speed to that distance before the rotate. A failed rotate keeps the distance. Perpetual distance −1 rotates and returns without subtracting. Any other success subtracts the absolute speed and, when the distance is exactly zero, stops the sequence and destroys.

EV_RotatePoly prints the invalid-poly message and returns false when the id is missing. The host mirror list is walked from the current poly. A busy or blocked poly stops the walk unless override is set. Linked polyportals (portal flag 2) stop the walk before a thinker is created. Otherwise a rotate thinker is stored, the poly is unblocked, and the distance is a perpetual −1 for byte angle 255, byteAngle×(90/64) for any other non-zero byte, or 360 when the byte is zero. Speed is speed×direction×(90/512). The door sequence starts at mode 0, then the direction sign flips for the next mirror. The call returns whether any thinker was created.

| Managed (`VmPoManRotate1177Slice.cs`) | Native (`po_man.cpp`) |
|----------------------------------------|------------------------|
| Registration, lifecycle, rotate tick, EV_RotatePoly | **1–368** |
| Band hull (exclusive slice) | **1–368** |

### Chunk **1177** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **368** |
| **Net headline** | **+368** vs post-**1176** |


**Post-review ratio:** 410/368 managed/native ≈ 111% (src file LOC at HEAD; see tally).

## Tests

- `VmPoManRotate1177SliceTests.Lifecycle_RegistersClassesAndClearsThePoly`
- `VmPoManRotate1177SliceTests.RotateTick_ClampsSpeedAndStopsAtZero`
- `VmPoManRotate1177SliceTests.RotatePoly_RejectsBusyMirrorsAndScalesTheAngle`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1177 --to 1177` (0 hard / 0 soft).

## Seams

FPolyMirrorIterator::NextMirror at 1805 stays a host list. RotatePolyobj, SetInterpolation, and the sound sequencer are host callbacks. DPolyDoor::Tick at 527 is chunk 671.

## Credit rationale

UNCredited po_man.cpp 1–368 (+368). DMovePoly::Tick starts at 376. A full tally line scan shows no overlap. 369–526 is the next slice.
