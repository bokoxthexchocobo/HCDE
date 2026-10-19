# ACS PrintBind / ThingDamage2 wire collision — chunk 16 audit

## Scope

Native BEHAVIOR wire indices 334–336 (`PCD_PRINTBIND`, `PCD_SETACTORSTATE`, `PCD_THINGDAMAGE2`) diverge from the generated `AcsPcode` enum (e.g. `PrintBind = 335`, `SetActorState = 336`, `ThingDamage2 = 337`). The VM previously treated wire **335** as `ThingDamage2`, which blocked `PrintBind` (334) and mis-routed `SetActorState` (335).

**Narrow fix:** `AcsPcodeWire` constants for those three wires, `TryDispatchCollidingWireOpcodes` before the main `switch`, removal of the erroneous `case 335` `ThingDamage2` handler. No bulk `switch` rewrite.

**Conversion native LOC credited:** 20 (`p_acs.cpp` `PCD_PRINTBIND` 9092–9107, `PCD_THINGDAMAGE2` 10477–10480; `SetActorState` behavior was already executed at the enum case—only wire routing added).

**Managed LOC credited:** 58 (`AcsPcodeWire.cs`, `AcsVm.WireDispatch.cs`, `AcsVm` hook + delete wrong case).

## Tests

- `AcsPrintBindWireTests.PrintBindWire_AppendsUnknownCommandPlaceholder`
- `AcsThingDamageOpcodeTests` (wire `336`)
- `AcsSetActorStateTests` (bytecode updated to `AcsPcodeWire.SetActorState` / 335)

## Open

- Remaining `AcsPcode` enum values still do not match the native wire table; only 334–336 are dispatched by wire index. A full enum↔wire migration is out of scope for this chunk.
- Bytecode that still emits legacy enum `SetActorState` (336) hits the existing `switch` case; native maps should use wire 335.

## Verified

`dotnet build -warnaserror`; full `dotnet test` (22,192 passed).
