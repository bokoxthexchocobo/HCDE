# Chunk 1154 — Weapon raise, bob smoothing, 2D/3D bob, and extra weapon buttons from `p_pspr.cpp` (**640–834**).

## Scope

Weapon raise, bob smoothing, 2D/3D bob, and extra weapon buttons from `p_pspr.cpp` (**640–834**).

## Native ranges

- `src/playsim/p_pspr.cpp` **Weapon bob and button checks** 640–834 (195 lines)

**Credited native LOC:** 195 (tally `nativeLoc` 195).

## Managed

`VmPPsprBob1154Slice.cs`
Managed LOC (tally): 165.
Conversion commit: `d6072c739ef07c91c121e605db3a7ba498c74942`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `P_BringUpWeapon` (**659–673**) | yes |
| `R_WeaponBobSmoothFrac` (**710–724**) | yes |
| `P_BobWeapon` (**726–751**) | yes |
| `P_BobWeapon3D` (**753–779**) | yes |
| `P_CheckWeaponButtons` (**789–814**) | yes |
| `WOFFlags` (**825–833**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **485–639** | sprite `SetState` already credited |
| **641–648** and **816–821** | VM thunks that return **0**, forwarded to the host |
| **663–665** and **672** | debug traces |
| **704–708** | CVAR callback with no immediate action |
| **94–102** | button table already credited by chunk **967**; this slice walks the host list |
| **835+** | overlay writers already credited |

`PSPB_None` is **0**, `PSPB_2D` is **1**, and `PSPB_3D` is **2**. `OF_EuthanizeMe` is **1<<5**. `WF_WEAPONRELOADOK` is **1<<5**, `WF_WEAPONZOOMOK` is **1<<6**, and `WF_USER1OK` through `WF_USER4OK` are **1<<8** through **1<<11**. `BT_RELOAD` is **1<<6**, `BT_ZOOM` is **1<<7**, and `BT_USER1` through `BT_USER4` are **1<<21** through **1<<24**. `WOF_KEEPX` is **1**, `WOF_KEEPY` is **2**, `WOF_ADD` is **4**, `WOF_INTERPOLATE` is **8**, `WOF_RELATIVE` is **16**, and `WOF_ZEROY` is **32**. `P_BringUpWeapon` calls the player-pawn virtual only when it exists. With smoothing off, the tic fraction is returned unchanged, including values outside **0..1**. With smoothing on, values at or below **0** become **0**, values at or above **1** become **1**, and the interior uses `t*t*(3-2*t)`. A missing bob virtual resets interpolation and does not store a sample. A present virtual passes frac **1**, marks 2D interpolation only for `P_BobWeapon`, and walks inventory. A euthanized item ends the walk before its override and before every later item. An item without `ModifyBob` keeps the current sample and still advances. `P_BobWeapon3D` is the same walk with separate translation and rotation, and its interpolation update is not the 2D one. No ready weapon leaves the sprite alone. Otherwise the first check whose weapon-state bit and button bit are both set, and whose state exists, wins. A matching check with no state falls through.

| Managed (`VmPPsprBob1154Slice.cs`) | Native (`p_pspr.cpp`) |
|------------------------------------|------------------------|
| Raise, bob, buttons, overlay flags | **640–834** |
| Band hull (exclusive slice) | **640–834** |

### Chunk **1154** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **195** |
| **Net headline** | **+195** vs post-**1153** |


**Post-review ratio:** 165/195 managed/native ≈ 85% (src file LOC at HEAD; see tally).

## Tests

- `VmPPsprBob1154SliceTests.Smooth_PassesRawFractionsUntilTheCurveIsEnabled`
- `VmPPsprBob1154SliceTests.Bob_StopsAtADeadItemAndResetsWithoutAVirtual`
- `VmPPsprBob1154SliceTests.Buttons_PreferZoomAndSkipAMissingState`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1154 --to 1154` (0 hard / 0 soft).

## Seams

The bring-up and bob virtuals, inventory links, and button-state lookup stay host-supplied. The button table itself is chunk **967**.

## Credit rationale

UNCredited **`p_pspr.cpp`** **640–834** (+195), inside the 1600-line file, with no conversion or recognition overlap.
