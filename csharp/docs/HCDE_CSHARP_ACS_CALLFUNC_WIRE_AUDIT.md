# Principal audit — ACS CallFunc wire-id table

**Date:** 2026-10-19  
**Scope:** Native `p_acs.cpp` `EACSFunctions` enumeration vs `AcsCallFunctionWireIds.cs`.  
**Result:** Accepted as inventory/wiring aid. `AcsCallFunctions` public constants remain the managed dispatch indices (not always equal to native wire order).

## Native behavior

- ACS `CallFunc` opcodes pass a 1-based index into `EACSFunctions` for library calls.

## Managed implementation

- `AcsCallFunctionWireIds.NameToWire` mirrors the native enum order (138 entries).
- `AcsCallFunctions` constants continue to identify `TryInvoke` switch arms.

## Divergences

- Several `AcsCallFunctions` ids (e.g. `CheckClass = 200`) are managed table slots, not native wire indices (`CheckClass` wire `100`).
- Hot functions still invoked via existing `TryInvoke` paths; this chunk does not remap dispatch.

## Test evidence

```text
dotnet test csharp/tests/HCDE.Playsim.Tests/HCDE.Playsim.Tests.csproj -c Release --filter FullyQualifiedName~AcsCallFunctionWireIdTests
```

## LOC tally

`LOC tally: converted this run = 142 native lines (149 C# lines added); native remaining = 506,180`
