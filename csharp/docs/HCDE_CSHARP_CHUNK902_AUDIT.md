# Chunk 902 — Generic crushers through `LS_Teleport_Line` (`p_lnspec.cpp` **915–1203**). Follows **901** (**≤913**); stops before `LS_ThrustThing` (**1215+**). Wider than **914–1200** to finish teleport-line function.

## Scope

Generic crushers through `LS_Teleport_Line` (`p_lnspec.cpp` **915–1203**). Follows **901** (**≤913**); stops before `LS_ThrustThing` (**1215+**). Wider than **914–1200** to finish teleport-line function.

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Generic_Crusher** 915–1203 (289 lines)

**Credited native LOC:** 289 (tally `nativeLoc` 289).

## Managed

`VmPLnspecCrusherPlatExitTeleSlice.cs`
Managed LOC (tally): 240.
Conversion commit: `6b784a85b34d689f6a2d5121c5e33770db97e58f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_lnspec.cpp`) |
|---------|-------------------------|
| `GenericCrusher*` | **915–935** |
| `Plat_*` / `Generic_Lift` | **937–1065** |
| `Exit_*` | **1067–1087** |
| `Teleport_*` | **1089–1203** |

### vs **901**

| | Lines |
|--|------:|
| **901** | **701–913** |
| **902** (this row) | **915–1203** (**289**) |
| `LS_ThrustThing` | **1215+** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **289** |
| Net behavior-ported | **289** |


**Post-review ratio:** 240/289 managed/native ≈ 83% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecCrusherPlatExitTeleSliceTests.GenericCrusher_SlowSpeedUsesSlowdownMode`
- `VmPLnspecCrusherPlatExitTeleSliceTests.PlatStop_HexenDefaultRemoves`
- `VmPLnspecCrusherPlatExitTeleSliceTests.GenericLift_TargetTwoUsesDownToNearest`
- `VmPLnspecCrusherPlatExitTeleSliceTests.TeleportNoFog_UseAngTwoAddsRotateBoom`
- `VmPLnspecCrusherPlatExitTeleSliceTests.ExitNormal_RequiresGoodExit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 902 --to 902` (0 hard / 0 soft).

## Seams

`EV_DoCeiling` (crushers), `EV_DoPlat`, `EV_StopPlat`, `CheckIfExitIsGood`, `ExitLevel`, `ChangeLevel`, and `EV_Teleport*` are not hosted.

## Credit rationale

Crusher slowdown/hexen modes, plat stop gametype gate, Heretic raise-and-stay default, generic lift target switch, exit gating, teleport flag packing, and silent line teleport args via `VmPLnspecCrusherPlatExitTeleSlice`.
