# `a_floor.cpp` — chunks 800+ (after **799** `CreateFloor` 281–514)

Standing rules: one TU per chunk, **whole functions only**, **150–400** native LOC, no padding or “just over 150%” bundles.

## Rejected layouts

| Idea | Native | Why reject |
|------|--------|------------|
| **800** = `EV_DoFloor` + crush/stop + `EV_DoDonut` + `EV_DoChange` only | **157** | Five small EVs stitched to barely clear 150 — engineered floor, not a natural band. |
| **802** = waggle whole functions only | **146** | Below 150 — do **not** pad; merge or leave uncredited. |

## Approved layout

### **799** (landed) — `CreateFloor` **281–514** (234)

Single whole function; spawn switch + instant/sound/change tail.

### **800** — floor line specials + stairs + donut + pure change (**339** native)

One contiguous **FLevelLocals** floor-EV band (tag iterators → `CreateFloor` / thinkers / sector edits), comfortably inside 150–400:

| Symbol | Lines | LOC |
|--------|------:|----:|
| `FLevelLocals::EV_DoFloor` | 539–552 | 14 |
| `FLevelLocals::EV_FloorCrushStop` | 562–579 | 18 |
| `FLevelLocals::EV_StopFloor` | 582–596 | 15 |
| `FLevelLocals::EV_BuildStairs` | 607–788 | 182 |
| `FLevelLocals::EV_DoDonut` | 796–863 | 68 |
| `FLevelLocals::EV_DoChange` | 1113–1154 | 42 |
| **Sum** | | **339** |

**Audit rationale:** Same subsystem (linedef-driven floor specials and stair/donut/retexture EVs) in one credit; avoids a 157-line “minimum viable” chunk and keeps `BuildStairs` (182) with its natural EV neighbors instead of isolating it from the smaller stop/do-floor helpers.

**Managed:** one VM (e.g. `VmAFloorEv.cs`); seams: tag iterators, `CreateFloor`, height/neighbor finders, `getNextSector`, thinker spawn/destroy, sector texture/special.

**Excluded:** `DEFINE_ACTION_FUNCTION(CreateFloor)` **516–529** (macro wrapper, not the credited function body).

### **801** — `DElevator` setup + waggle thinkers (**206** native)

Waggle whole functions alone are **146** (&lt;150). Merge with the **adjacent** elevator thinker lifecycle functions immediately above waggle in the TU (not `DElevator::Tick` — already credited elsewhere):

| Symbol | Lines | LOC |
|--------|------:|----:|
| `DElevator::Construct` | 878–885 | 8 |
| `DElevator::Serialize` | 887–909 | 23 |
| `DElevator::OnDestroy` | 911–939 | 29 |
| `DWaggleBase::Serialize` | 1167–1178 | 12 |
| `DWaggleBase::Construct` | 1190–1193 | 4 |
| `DWaggleBase::DoWaggle` | 1201–1270 | 70 |
| `DFloorWaggle::Construct` | 1278–1283 | 6 |
| `DFloorWaggle::Tick` | 1285–1288 | 4 |
| `DCeilingWaggle::Construct` | 1296–1301 | 6 |
| `DCeilingWaggle::Tick` | 1303–1306 | 4 |
| `FLevelLocals::EV_StartWaggle` | 1314–1353 | 40 |
| **Sum** | | **206** |

**Audit rationale:** Waggle shares thinker/sector plane mechanics with elevator construction in the same file tail; bundling elevator construct/serialize/destroy with waggle types reaches a honest **206** without padding. `IMPLEMENT_CLASS` lines between symbols are not credited.

**Already credited elsewhere in this TU:** `DFloor::Tick` / sounds / `EV_DoElevator` (**798**), `DElevator::Tick` (prior chunk-6 band), `CreateFloor` (**799**).

### After **801**

No remaining whole-function bands ≥150 native in `a_floor.cpp` without partial credits. Continue in **`p_enemy` / `p_spec`** per session open items.
