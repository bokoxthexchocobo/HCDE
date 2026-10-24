# Chunk 646 — Master CCMDs and public `SV_*` API (`sv_master.cpp` 837–1092)

## Scope

Master CCMDs and public `SV_*` API (`sv_master.cpp` 837–1092)

## Native ranges

- `src/common/engine/sv_master.cpp` **SV_InitMasters** 837–1092 (256 lines)

**Credited native LOC:** 256 (tally `nativeLoc` 256).

## Managed

`VmSvMasterPublicApi.cs`
Managed LOC (tally): 128.
Conversion commit: `31c4766166ac7776910b29286701789a77c5f16a`.

## Padding review (2026-10-21)

| Managed | Native (`sv_master.cpp`) |
|---------|---------------------------|
| `ShouldInitMastersNative` | `SV_InitMasters` 883–891 |
| `ShouldRejectEmptyMasterNative` | `SV_AddMaster` 942–946 |
| `ShouldReresolveAddressesNative` | `SV_UpdateMaster` 1053–1073 |
| `ShouldRunKeepAliveNative` | 1075–1081 |
| `IsAdvertisingEnabledNative` | `SV_IsMasterAdvertisingEnabled` 1084–1087 |

**Post-review ratio:** 128/256 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

- `VmSvMasterPublicApiTests.ShouldShutdownWhenDisabledNative_WhenMastersOffButSocketOpen`
- `VmSvMasterPublicApiTests.ShouldReresolveAddressesNative_RespectsInterval`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 646 --to 646` (0 hard / 0 soft).

## Seams

`closesocket`, `inet_ntoa`, `fprintf`, and master list mutation are hosted.

## Credit rationale

Whole `CCMD(addmaster)` through `SV_GetMasterCount`.
