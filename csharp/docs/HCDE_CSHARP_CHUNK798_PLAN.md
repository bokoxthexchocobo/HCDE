# Chunk 798 plan — `a_floor.cpp` (pre-land)

## Clarification: 985–1313 vs ~225 native

The session audit **open-item** line “`a_floor.cpp` tail **985–1313** (~329 lines)” was an **uncredited file span** from gap analysis (everything after the already-credited `DElevator::Tick` band through before `EV_StartWaggle`), **not** one function and **not** a proposed single-chunk credit.

That span mixes multiple whole functions and headers, including:

| Symbol | Lines | Lines (count) | In chunk 798? |
|--------|------:|--------------:|:-------------:|
| `DElevator::Tick` | 941–984 | 44 | **No** — already credited (`loc_tally` / chunk 6 band) |
| `DElevator::StartFloorSound` | 992–995 | 4 | **Yes** |
| `FLevelLocals::EV_DoElevator` | 1010–1095 | 86 | **Yes** |
| `FLevelLocals::EV_DoChange` | 1113–1154 | 42 | **No** — future chunk (≥150 with neighbors if needed) |
| Waggle / `EV_StartWaggle` bands | 1167+ | (varies) | **No** — separate chunks |

Chunk **798** credits **five whole functions only** (DFloor mover tick/change/sound + elevator EV setup), **225** native LOC total — within **150–400**, no partial functions.

`static StartFloorSound` (**45–61**) is **not** credited separately; native credits `DFloor::StartFloorSound` / `DElevator::StartFloorSound`, which call it. Managed port uses an injected floor-sequence seam matching `::StartFloorSound` behavior (not a second credited native symbol).

## Chunk 798 — exact `nativeRanges` (whole functions only)

| Native symbol | Start | End | Native LOC |
|---------------|------:|----:|-----------:|
| `DFloor::Tick` | 114 | 225 | 112 |
| `DFloor::SetFloorChangeType` | 233 | 251 | 19 |
| `DFloor::StartFloorSound` | 259 | 262 | 4 |
| `DElevator::StartFloorSound` | 992 | 995 | 4 |
| `FLevelLocals::EV_DoElevator` | 1010 | 1095 | 86 |
| **Sum** | | | **225** |

**Excluded from 798 (not partial trims):** `DFloor::Construct` (270–273), `CreateFloor` / `EV_DoFloor` (earlier file), `DElevator::Tick` (941–984, prior credit), `EV_DoChange`, waggle types.

## Split rule

If a future bundle exceeds **400** native LOC, split **only** at whole-function boundaries.

**800+ regrouping (2026-10-22):** see `HCDE_CSHARP_A_FLOOR_CHUNK_PLAN_800plus.md` — **800** = floor EV + stairs + donut + change (**339**); **801** = `DElevator` construct/serialize/destroy + waggle + `EV_StartWaggle` (**206**). Do not credit waggle alone (**146**) or a **157**-line five-function EV patch.
