# Chunk 806 — Generic door/floor, door wait, texture floors, waggle/transfer/donut, pillars (`p_lnspec.cpp` 278–289, 305–334, 336–364, 415–419, 472–483, 502–573, 652–669).

## Scope

Generic door/floor, door wait, texture floors, waggle/transfer/donut, pillars (`p_lnspec.cpp` 278–289, 305–334, 336–364, 415–419, 472–483, 502–573, 652–669).

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Door_WaitRaise** 278–283 (6 lines)
- `src/playsim/p_lnspec.cpp` **LS_Door_WaitClose** 284–289 (6 lines)
- `src/playsim/p_lnspec.cpp` **LS_Generic_Door** 305–334 (30 lines)
- `src/playsim/p_lnspec.cpp` **LS_Floor_LowerByValue** 336–340 (5 lines)
- `src/playsim/p_lnspec.cpp` **LS_Floor_LowerToLowest** 342–346 (5 lines)
- `src/playsim/p_lnspec.cpp` **LS_Floor_LowerToHighest** 348–352 (5 lines)
- `src/playsim/p_lnspec.cpp` **LS_Floor_LowerToNearest** 360–364 (5 lines)
- `src/playsim/p_lnspec.cpp` **LS_Floor_CrushStop** 415–419 (5 lines)
- `src/playsim/p_lnspec.cpp` **LS_Floor_RaiseByTexture** 472–477 (6 lines)
- `src/playsim/p_lnspec.cpp` **LS_Floor_LowerByTexture** 478–483 (6 lines)
- `src/playsim/p_lnspec.cpp` **LS_Floor_Waggle** 502–506 (5 lines)
- `src/playsim/p_lnspec.cpp` **LS_Ceiling_Waggle** 508–512 (5 lines)
- `src/playsim/p_lnspec.cpp` **LS_Floor_TransferTrigger** 514–518 (5 lines)
- `src/playsim/p_lnspec.cpp` **LS_Floor_TransferNumeric** 520–524 (5 lines)
- `src/playsim/p_lnspec.cpp` **LS_Floor_Donut** 526–530 (5 lines)
- `src/playsim/p_lnspec.cpp` **LS_Generic_Floor** 532–567 (36 lines)
- `src/playsim/p_lnspec.cpp` **LS_Floor_Stop** 569–573 (5 lines)
- `src/playsim/p_lnspec.cpp` **LS_Pillar_Build** 652–657 (6 lines)
- `src/playsim/p_lnspec.cpp` **LS_Pillar_BuildAndCrush** 658–663 (6 lines)
- `src/playsim/p_lnspec.cpp` **LS_Pillar_Open** 664–669 (6 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmPLnspecGenericDoorFloorPillar.cs`
Managed LOC (tally): 102.
Conversion commit: `4bf2b7334a02e3762d5089a9d2ef8d6eecafc7cf`.

## Padding review (2026-10-21)

| Managed | Native (`p_lnspec.cpp`) |
|---------|----------------------|
| `DoorWaitRaise` | `LS_*` **278–283** |
| `DoorWaitClose` | `LS_*` **284–289** |
| `GenericDoor` | `LS_*` **305–334** |
| `FloorLowerByValue` | `LS_*` **336–340** |
| `FloorLowerToLowest` | `LS_*` **342–346** |
| `FloorLowerToHighest` | `LS_*` **348–352** |
| `FloorLowerToNearest` | `LS_*` **360–364** |
| `FloorCrushStop` | `LS_*` **415–419** |
| `FloorRaiseByTexture` | `LS_*` **472–477** |
| `FloorLowerByTexture` | `LS_*` **478–483** |
| `FloorWaggle` | `LS_*` **502–506** |
| `CeilingWaggle` | `LS_*` **508–512** |
| `FloorTransferTrigger` | `LS_*` **514–518** |
| `FloorTransferNumeric` | `LS_*` **520–524** |
| `FloorDonut` | `LS_*` **526–530** |
| `GenericFloor` | `LS_*` **532–567** |
| `FloorStop` | `LS_*` **569–573** |
| `PillarBuild` | `LS_*` **652–657** |
| `PillarBuildAndCrush` | `LS_*` **658–663** |
| `PillarOpen` | `LS_*` **664–669** |

**Post-review ratio:** 102/163 managed/native ≈ 63% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecGenericDoorFloorPillarTests.GenericDoor_RaiseInvokesEvDoDoor`
- `VmPLnspecGenericDoorFloorPillarTests.PillarOpen_InvokesEvDoPillar`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 806 --to 806` (0 hard / 0 soft).

## Seams

`EV_DoDoor`, `EV_DoFloor`, `EV_DoPillar`, `EV_StartWaggle`, `EV_DoChange`, `EV_StopFloor`, and `EV_DoDonut` are injected.

## Credit rationale

Faithful whole-function line specials; retires slices on `line-specials-plat-generic-elevator`.

## Portfolio delta (805+806 band; headline **93,153** → **93,034** at `057c9a38`)

| Tally row | Gross / trim (native LOC) | Commit |
| --- | ---: | --- |
| **806** `p-lnspec-generic-door-floor-pillar-chunk806` | **+163** | `f06162fa` |
| **line-specials-plat-generic-elevator** | **−110** (128→18; elevators only) | `f06162fa` |
| **413** `p-lnspec-floor-movers-chunk413` | **−136** (154→18; `LS_Floor_RaiseToCeiling` only) | `057c9a38` |
| **431** `p-lnspec-doors-pillars-chunk431` | **−24** (150→126) | `057c9a38` |
| **416** `p-lnspec-stairs-thing-chunk416` | **−6** (171→165; stair band trim) | `057c9a38` |
| **line-special-feasible-gaps** | **−6** (51→45; waggle overlap) | `057c9a38` |

**806 gross:** **+163** native (`nativeLoc` 163). **806 overlap trims (this chunk):** **−110 −136 −24 −6 −6 = −282**. Combined with **805** gross **+173** and **805** trims **−173**, **net headline Δ = +336 −455 = −119** (**93,153** → **93,034**).

### Re-port / shared managed code

**806** whole-function ports door/floor/pillar `LS_*` bands that **`line-specials-plat-generic-elevator`**, **`p-lnspec-floor-movers-chunk413`**, **`p-lnspec-doors-pillars-chunk431`**, and **`line-special-feasible-gaps`** had partially credited. Native ranges were **trimmed or zeroed on those rows in `f06162fa` / `057c9a38`**, not left double-counted. `VmPLnspecGenericDoorFloorPillar.cs` owns the **806** bands. **`VmPLnspecFloorLineSpecials.cs` removed**; surviving **413** slice lives in `VmPLnspecFloorRaiseToCeiling.cs` only.

### Process / gate (806 landed before full tally hygiene)

| Step | Commit | `LocTallyIntegrity` | Hardened verify | Notes |
| --- | --- | --- | --- | --- |
| Port | `4bf2b733` | — | port-only | Whole-function TU committed **before** tally sync. |
| Tally (partial overlap) | `f06162fa` | **FAIL** (overlap + **−55** header drift) | **FAIL** | **RED** on `origin`. |
| Overlap + headline fix | `057c9a38` | **PASS** | **PASS** | Manual follow-up commit (not same sync as `f06162fa`). |

**Confirmed:** **`f06162fa` was pushed** while integrity and hardened gate **failed**. **`057c9a38`** repaired tally before chunk **807**. From this point forward, **only** `land_chunk_fast.sh` (single flow: port → tally+overlap → reaudit → verify → push).
