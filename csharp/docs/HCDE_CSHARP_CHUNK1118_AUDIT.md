# Chunk 1118 — `GetDefaultSpeed`, `P_SpawnMissileXYZ` aim policy, `P_SpawnMissile`, and `P_SpawnMissileZ` from `p_mobj.cpp` (**7901–8050**). Skips recognition **7880–7900** (`PlaySpawnSound`, chunk **19**).

## Scope

`GetDefaultSpeed`, `P_SpawnMissileXYZ` aim policy, `P_SpawnMissile`, and `P_SpawnMissileZ` from `p_mobj.cpp` (**7901–8050**). Skips recognition **7880–7900** (`PlaySpawnSound`, chunk **19**).

## Native ranges

- `src/playsim/p_mobj.cpp` **PlaySpawnSound and SpawnMissileXYZ** 7901–8050 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPMobjSpawnMissile1118Slice.cs`
Managed LOC (tally): 132.
Conversion commit: `c20f9e3c91f381e6b8cd291e1e6b446e89834ff1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `GetDefaultSpeed` fast override (**7912–7921**) | yes |
| Missing source/dest abort (**7935–7945**) | yes |
| Floorclip unless sentinel Z (**7947–7950**) | yes |
| Owner default + hugger/overhead Z (**7955–7978**) | yes |
| Spectral friend + spawn check (**7985–7991**) | yes |
| `P_SpawnMissile` launch Z (**8015**) | yes |
| `P_SpawnMissileZ` source gate (**8029–8033**) | yes |
| `P_OldSpawnMissile` null gate (**8047–8049**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **7880–7900** | recognition chunk **19** |
| **7902–7907**, **7996–8006**, **8018–8024**, **8036–8042** | VM thunks |
| **7952**, **7968**, **7981**, **7992** | host spawn/resize/net |
| **8051+** | old missile body outside hull |

| Managed (`VmPMobjSpawnMissile1118Slice.cs`) | Native (`p_mobj.cpp`) |
|--------------------------------------------|---------------------|
| Speed + missile aim/Z | **7901–8050** |
| Band hull (exclusive slice) | **7901–8050** |

### Chunk **1118** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1117** |


**Post-review ratio:** 132/150 managed/native ≈ 88% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjSpawnMissile1118SliceTests.ResolveSpawnSound_UsesPositionWhenSpawnerMissingAndSectorAudible`
- `VmPMobjSpawnMissile1118SliceTests.GetDefaultSpeed_PrefersFastSpeedWhenSkillIsFast`
- `VmPMobjSpawnMissile1118SliceTests.RunSpawnMissileXyz_ZerosHuggerVelocityAndAbortsFailedCheck`
- `VmPMobjSpawnMissile1118SliceTests.TrySpawnMissileZ_RejectsMissingSource`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1118 --to 1118` (0 hard / 0 soft).

## Seams

`Spawn`, `Vec3To`, shadow handling, and `P_OldSpawnMissile` body past the null check remain host-injected.

## Credit rationale

UNCredited conversion band **7901–8050** (+150) after **1117**. **7877–7900** overlaps recognition **19**.
