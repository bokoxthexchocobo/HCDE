# Chunk 646 — Master CCMDs and public `SV_*` API (`sv_master.cpp` 837–1092)

## Scope

Master CCMDs and public `SV_*` API (`sv_master.cpp` 837–1092)

## Native ranges

- `src/common/engine/sv_master.cpp` **SV_InitMasters** 837–846 (10 lines)
- `src/common/engine/sv_master.cpp` **SV_InitMasters** 848–858 (11 lines)
- `src/common/engine/sv_master.cpp` **SV_InitMasters** 860–865 (6 lines)
- `src/common/engine/sv_master.cpp` **SV_InitMasters** 867–876 (10 lines)
- `src/common/engine/sv_master.cpp` **SV_InitMasters** 878–881 (4 lines)
- `src/common/engine/sv_master.cpp` **SV_InitMasters** 883–909 (27 lines)
- `src/common/engine/sv_master.cpp` **SV_InitMasters** 911–920 (10 lines)
- `src/common/engine/sv_master.cpp` **SV_InitMasters** 922–938 (17 lines)
- `src/common/engine/sv_master.cpp` **SV_InitMasters** 940–967 (28 lines)
- `src/common/engine/sv_master.cpp` **SV_InitMasters** 969–986 (18 lines)
- `src/common/engine/sv_master.cpp` **SV_InitMasters** 988–1033 (46 lines)
- `src/common/engine/sv_master.cpp` **SV_InitMasters** 1035–1042 (8 lines)
- `src/common/engine/sv_master.cpp` **SV_InitMasters** 1044–1082 (39 lines)
- `src/common/engine/sv_master.cpp` **SV_InitMasters** 1084–1087 (4 lines)
- `src/common/engine/sv_master.cpp` **SV_InitMasters** 1089–1092 (4 lines)

**Credited native LOC:** 242 (tally `nativeLoc` 242).

## Managed

`VmSvMasterPublicApi.cs`
Managed LOC (tally): 330.
Conversion commit: `67af04240228463929a02c962efbcc3f3d9a3682`.

## Padding review (2026-10-21)

| Managed | Native (`sv_master.cpp`) |
|---------|---------------------------|
| `RunAddMasterCcmdNative` | 837–846 |
| `RunAdvertiseCcmdNative` | 848–858 |
| `RunUnadvertiseCcmdNative` | 860–865 |
| `RunDelMasterCcmdNative` | 867–876 |
| `RunMastersCcmdNative` | 878–881 |
| `InitMastersNative` | 883–909 |
| `ShutdownMastersNative` | 911–920 |
| `UpdateMasterServersNative` | 922–938 |
| `AddMasterNative` | 940–967 |
| `RemoveMasterNative` | 969–986 |
| `ListMastersNative` | 988–1033 |
| `ArchiveMastersNative` | 1035–1042 |
| `UpdateMasterNative` | 1044–1082 |
| `IsAdvertisingEnabledNative` | 1084–1087 |
| `GetMasterCountNative` | 1089–1092 |

### Out-of-body uncredit (646)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **242** |
| **Out-of-body removed** | **0** |
| **Honest** | **242** |


**Post-review ratio:** 330/242 managed/native ≈ 136% (src file LOC at HEAD; see tally).

## Tests

- `VmSvMasterPublicApiTests.InitMastersNative_SkipsWhenAdvertisingDisabled`
- `VmSvMasterPublicApiTests.UpdateMasterNative_RunsKeepAliveWhenIntervalElapsed`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 646 --to 646` (0 hard / 0 soft).

## Seams

`closesocket`, `inet_ntoa`, `fprintf`, and master list mutation are hosted.

## Credit rationale

Whole `CCMD(addmaster)` through `SV_GetMasterCount`.
