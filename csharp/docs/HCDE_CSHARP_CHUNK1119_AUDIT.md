# Chunk 1119 — `P_OldSpawnMissile` body, `P_SpawnMissileZAimed`, `P_SpawnMissileAngleZSpeed`, and `P_SpawnSubMissile` friend head from `p_mobj.cpp` (**8051–8200**); follows **1118**.

## Scope

`P_OldSpawnMissile` body, `P_SpawnMissileZAimed`, `P_SpawnMissileAngleZSpeed`, and `P_SpawnSubMissile` friend head from `p_mobj.cpp` (**8051–8200**); follows **1118**.

## Native ranges

- `src/playsim/p_mobj.cpp` **Old and angled missile spawn** 8051–8200 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPMobjMissileAngles1119Slice.cs`
Managed LOC (tally): 122.
Conversion commit: `b1fd993a2e702076a43de6a5659c102934f3330b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Old missile Z and vertical speed (**8051–8061**) | yes |
| Spectral friend on old missile (**8063–8066**) | yes |
| Aimed vz = (destZ-sourceZ)/ (dist/speed) (**8104–8107**) | yes |
| Angle-speed floorclip and owner default (**8139–8147**) | yes |
| Yaw, vz, spectral, spawn check (**8151–8161**) | yes |
| Sub-missile spectral friend copy (**8192–8200**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **8047–8049** | null gate credited on **1118** |
| **8055**, **8068–8069**, **8108–8109** | host sound/net/shadow |
| **8073–8079**, **8112–8118**, **8166–8176** | VM thunks |
| **8204+** | aim pitch outside hull |

| Managed (`VmPMobjMissileAngles1119Slice.cs`) | Native (`p_mobj.cpp`) |
|---------------------------------------------|---------------------|
| Old/aimed/angle missile policy | **8051–8200** |
| Band hull (exclusive slice) | **8051–8200** |

### Chunk **1119** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1118** |


**Post-review ratio:** 122/150 managed/native ≈ 81% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjMissileAngles1119SliceTests.AimedVerticalSpeed_UsesTravelTimeWhenDistancePositive`
- `VmPMobjMissileAngles1119SliceTests.RunOldSpawnMissile_PlacesThirtyTwoAboveSource`
- `VmPMobjMissileAngles1119SliceTests.RunSpawnMissileAngleZSpeed_AppliesFloorclipAndFailsCheck`
- `VmPMobjMissileAngles1119SliceTests.ApplySubMissileFriend_CopiesSpectralMissileOwner`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1119 --to 1119` (0 hard / 0 soft).

## Seams

`Spawn`, `VelFromAngle`, `P_PlaySpawnSound`, and shadow handling remain host-injected; sub-missile aim continues at **8204**.

## Credit rationale

UNCredited **`p_mobj.cpp`** **8051–8200** (+150) with no conversion or recognition line overlap.
