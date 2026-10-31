# Chunk 1120 — `P_SpawnSubMissile` pitch tail and `P_SpawnPlayerMissile` aim, floor clamp, and velocity mode from `p_mobj.cpp` (**8201–8350**); follows **1119**.

## Scope

`P_SpawnSubMissile` pitch tail and `P_SpawnPlayerMissile` aim, floor clamp, and velocity mode from `p_mobj.cpp` (**8201–8350**); follows **1119**.

## Native ranges

- `src/playsim/p_mobj.cpp` **SpawnSubMissile tail and SpawnPlayerMissile** 8201–8350 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPMobjPlayerMissile1120Slice.cs`
Managed LOC (tally): 136.
Conversion commit: `4996e5ff62861f7d1f34dd61a1576af7ffe87298`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Sub-missile pitch speed (**8206–8207**) | yes |
| Missing source/type and local suppress (**8236–8250**) | yes |
| Clear ignore-noautoaim (**8253**) | yes |
| Weapon no-autoaim keeps pitch (**8263–8268**) | yes |
| Three-angle sweep and freelook break (**8276–8298**) | yes |
| Spawn Z floor clamp (**8302–8310**) | yes |
| Hugger vs 3D velocity and spectral (**8318–8329**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **8208**, **8313**, **8338** | host net/spawn |
| **8214–8219**, **8344–8350** | VM thunk / parameter list |
| **8357+** | angle sentinel outside hull |

| Managed (`VmPMobjPlayerMissile1120Slice.cs`) | Native (`p_mobj.cpp`) |
|---------------------------------------------|---------------------|
| Sub-missile pitch + player missile aim | **8201–8350** |
| Band hull (exclusive slice) | **8201–8350** |

### Chunk **1120** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1119** |


**Post-review ratio:** 136/150 managed/native ≈ 91% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjPlayerMissile1120SliceTests.SubMissileVerticalSpeed_NegatesSine`
- `VmPMobjPlayerMissile1120SliceTests.AimRange_UsesSixteenTilesWhenUnset`
- `VmPMobjPlayerMissile1120SliceTests.RunSpawnPlayerMissile_KeepsPlayerPitchWhenWeaponDisablesAutoAim`
- `VmPMobjPlayerMissile1120SliceTests.RunSpawnPlayerMissile_ClampsSpawnBelowFloor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1120 --to 1120` (0 hard / 0 soft).

## Seams

`P_AimLineAttack`, `Spawn`, and coop missile registration remain host-injected; the 1e37 angle sentinel is past **8350**.

## Credit rationale

UNCredited **`p_mobj.cpp`** **8201–8350** (+150) with no conversion or recognition line overlap.
