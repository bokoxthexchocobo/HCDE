# Chunk 1178 — DMovePoly and DMovePolyTo ticks plus EV_MovePoly and EV_MovePolyTo from po_man.cpp (369–526).

## Scope

DMovePoly and DMovePolyTo ticks plus EV_MovePoly and EV_MovePolyTo from po_man.cpp (369–526).

## Native ranges

- `src/playsim/po_man.cpp` **EV_MovePoly** 369–526 (158 lines)

**Credited native LOC:** 158 (tally `nativeLoc` 158).

## Managed

`VmPoManMove1178Slice.cs`
Managed LOC (tally): 203.
Conversion commit: `9cfe93a3c5152df2738cbecfb216fc7bb20374ea`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| DMovePoly::Tick (**376–397**) | yes |
| EV_MovePoly (**405–445**) | yes |
| DMovePolyTo::Tick (**453–474**) | yes |
| EV_MovePolyTo (**482–519**) | yes |
| Band hull (**369–526**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–368** | registration and EV_RotatePoly are chunk **1177** |
| **527–625** | DPolyDoor::Tick is chunk **671** |

A missing poly on either tick does nothing. A failed move leaves distance, speed, and the velocity alone and does not update links. A successful move subtracts the absolute speed. Distance at or below zero stops the sequence, destroys the thinker, and still updates links. A shorter remaining distance keeps the sign of the speed and, for a plain move, rebuilds the velocity from the angle. A move-to replaces the velocity with the target minus the poly start spot, which is the full remaining delta.

EV_MovePoly prints the invalid-poly message and returns false when the id is missing. A busy or blocked mirror stops the walk unless override is set. Each started poly stores the distance, speed, current angle, and that angle's vector, then starts the door sequence at mode 0. The original distance divided by the original speed stops interpolation when the ratio is less than or equal to 2, including a negative speed. The angle then advances 180 degrees. The unused local angle copy is not reintroduced.

EV_MovePolyTo measures the unit from the original poly's start spot to the target and keeps that length for every mirror. Each target is that poly's start plus the unit times the length. Velocity is the unit times the speed. After a start, the unit reverses and the length stays. The same ratio stops interpolation. A zero-length target stays on the start spot and stops interpolation.

| Managed (`VmPoManMove1178Slice.cs`) | Native (`po_man.cpp`) |
|--------------------------------------|------------------------|
| Move and move-to ticks and events | **369–526** |
| Band hull (exclusive slice) | **369–526** |

### Chunk **1178** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **158** |
| **Net headline** | **+158** vs post-**1177** |


**Post-review ratio:** 203/158 managed/native ≈ 128% (src file LOC at HEAD; see tally).

## Tests

- `VmPoManMove1178SliceTests.MoveTick_StopsAtTheEndAndRescalesTheLastStep`
- `VmPoManMove1178SliceTests.MovePoly_FlipsAngleAndStopsFastInterpolation`
- `VmPoManMove1178SliceTests.MoveTo_AimsAtTheStartSpotAndReversesMirrors`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1178 --to 1178` (0 hard / 0 soft).

## Seams

MovePolyobj and the mirror walk are host callbacks. Thinker types and interpolation live on chunk 1177. DPolyDoor::Tick at 527 is chunk 671.

## Credit rationale

UNCredited po_man.cpp 369–526 (+158). Chunk 1177 ends at 368. A full tally line scan shows no overlap. The door tick at 527 stays chunk 671.
