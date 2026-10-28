# Principal audit — native action function catalog

**Date:** 2026-10-19  
**Scope:** `p_actionfunctions.cpp` `DEFINE_ACTION_FUNCTION` inventory via `generate_action_function_index.py`.  
**Result:** Accepted as migration index; no new runtime dispatch in this chunk.

## Deliverable

- `csharp/docs/data/action_function_index.json` listing native action function symbols for gap tracking against managed `ActorSpecialActions` / state hooks.

## Tests

`ActionFunctionIndexTests` validates JSON count > 0 and known symbols (`A_SetGravity`, `A_FirePistol`).

## LOC tally

`LOC tally: converted this run = 380 native lines (120 C# lines added); native remaining = 503,385`
