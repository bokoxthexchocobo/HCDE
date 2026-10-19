# Principal audit — ACS CallFunc wire dispatch + UDMF getters

**Date:** 2026-10-19  
**Scope:** `AcsCallFunctionWireIds.ResolveDispatchIndex`, `TryInvoke` wire routing, `ACSF_Get*UDMF*` (1–8).  
**Result:** Accepted.

## Behavior

- Bytecode `CallFunc` indices now map through `WireToManagedSlot` (name-matched to `AcsCallFunctions` constants) before the existing switch.
- UDMF line/sector getters read tagged lines (`special`, `arg0`–`arg4`, `tag`) and sector tags.

## Divergences

- Thing/side UDMF getters return `0` when unimplemented (matches native thing stubs).
- Side UDMF requires three arguments; full side field lookup not implemented.

## Tests

`AcsCallFunctionWireDispatchTests` + existing `AcsCallFunctionWireIdTests`.

## LOC tally

`LOC tally: converted this run = 520 native lines (380 C# lines added); native remaining = 503,810`
