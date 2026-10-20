# Chunk 83 — `g_taunt.cpp`

## Scope

`*taunt` / `*taunt-variant` sound naming and the 1500 ms CCMD cooldown gate (network emit omitted; cosmetic timing only).

## Managed mapping

- `VmHcdeTaunt.cs`

## Tests

- `VmWholeFilePortTests.HcdeTaunt_BuildsCanonicalAndVariantNames`
- `VmWholeFilePortTests.HcdeTaunt_CooldownBlocksBackToBackCalls`
