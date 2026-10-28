# Chunk 765 — `DPSprite` construction/list insert, `FindPSprite`, `P_CreatePsprite`, weapon bob, and button routing (`p_pspr.cpp` 163–270, 659–821; excludes `CUSTOM_CVAR` 704–708 and credited `SetState` 485+). Retired (2026-10-24): `--band` hull overlaps existing credit (p-psprite-setstate-chunk700 src/playsim/p_pspr.cpp 485-639).

## Scope

`DPSprite` construction/list insert, `FindPSprite`, `P_CreatePsprite`, weapon bob, and button routing (`p_pspr.cpp` 163–270, 659–821; excludes `CUSTOM_CVAR` 704–708 and credited `SetState` 485+). Retired (2026-10-24): `--band` hull overlaps existing credit (p-psprite-setstate-chunk700 src/playsim/p_pspr.cpp 485-639).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `cc50ba6d7b57eeff499b862295c8b755211d8f60`.

## Padding review (2026-10-21)

| Managed | Native (`p_pspr.cpp`) |
|---------|----------------------|
| `Construct` | `DPSprite::DPSprite` **163–219** |
| `FindPsprite` / `FindPspriteAction` | `player_t::FindPSprite` **227–242**, DEFINE **244–249** |
| `CreatePsprite` | `P_CreatePsprite` **258–270** |
| `BringUpWeapon` | **659–673** |
| `WeaponBobSmoothFrac` | **710–724** |
| `BobWeapon` / `BobWeapon3D` | **726–779** |
| `CheckWeaponButtons` / `CheckWeaponButtonsAction` | **789–821** |

### Out-of-body uncredit (765)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **207** |
| **Out-of-body removed** | **6** |
| **Honest** | **201** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 765 --to 765` (0 hard / 0 soft).

## Seams

VM `BringUpWeapon`/`BobWeapon` virtuals, inventory override virtuals, `P_SetPsprite`, and GC/write barriers on list insert are injected.

## Credit rationale

Single-TU band linking psprite construction/lookup with the weapon bob and `ButtonChecks` loop (not a floor-padded scatter credit).
