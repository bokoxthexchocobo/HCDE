# Chunk 814 — deferred (batch 811–814 review)

## Planned `p_map` collision slice — **not landed**

Draft `chunk814.json` targeted `p_map.cpp` **98–315** (`VmPMapActorCollision`). Pre-port check:

```
uncredited_range_check.py --file src/playsim/p_map.cpp --start 98 --end 315
  [OVERLAP] … credited by p-map-find-floor-ceiling-chunk786 (214–240, 252–315)
```

**Action:** Removed draft VM/tests/json from the tree; do **not** credit without a fresh **UNCredited** band.

## `p_mobj.cpp` **1–209** — not eligible as whole-function chunk

```
uncredited_range_check.py --file src/playsim/p_mobj.cpp --start 1 --end 209
  [UNCredited] cli 1-209 (209 lines)
```

Whole functions in that band are only `sv_gravity` (**122–132**), `AActor::EnableNetworking` (**175–184**), and `AActor::PropagateMark` (**192–200**) (~30 native LOC). The rest is `IMPLEMENT_*` / `DEFINE_*` registration — not whole-function ports. **150 native LOC floor** cannot be met without violating whole-function-only rule.

Prior mobj/pspr credits to respect: chunks **748–751**, **758–762**, **765** (`p_pspr` ctor/**FindPSprite**), **813** (`p_pspr` header **1–162**).

## Next uncredited `p_map` bands (for a future 814+)

- `p_map.cpp` **1–213** (213 lines) — early movement/collision not yet tallied as one chunk
- `p_map.cpp` **5051–5244** (194 lines)
