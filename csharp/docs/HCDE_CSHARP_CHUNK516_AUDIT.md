# Chunk 516 — HCDEApplyInvasionSnapshot

## Scope

HCDEApplyInvasionSnapshot

## Native ranges

- `src/d_net.cpp` **HCDEApplyInvasionSnapshot** 3351–3510 (160 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Managed

`VmDNetInvasionSnapshotApply.cs`
Managed LOC (tally): 343.
Conversion commit: `1d132c033f60e30f292da6a59e400da186bb4fa7`.

## Padding review (2026-10-21)

| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `ApplyInvasionSnapshot` / `RunApplyInvasionSnapshotPipeline` | `HCDEApplyInvasionSnapshot` **3351–3510** |
| `RunClientCapabilityGate` | client + capability guards **3353–359** |
| `RunInitialHeaderValidation` / `RunHeaderValidationPipeline` / `TryReadSnapshotHeader` | magic + header size **3361–383** |
| `RunInvasionDisabledGametypeGate` / `TraceGametypeOverride` | disabled-mode gametype **3371–378** (local `!invasionEnabled`) |
| `ValidateSnapshotHeaderBytes` / `RunSnapshotHeaderValidation` | version/flags/state **3384–390** |
| `RunSpawnMetadataPipeline` / `ValidateSpawnMetadataBlock` | v2 spawn metadata **3417–435** |
| `BuildPayloadFromFields` / `PendingWaveFromState` | mirror field merge **3449–471** |
| `RunEmbeddedReplayPipeline` | authority + actor deltas **3475–489** |
| prior decision helpers (`VersionAllowed`, `ClampNonNegative`, …) | shared predicates **3357–end** |

### Out-of-body uncredit (516)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **160** |
| **Out-of-body removed** | **0** |
| **Honest** | **160** |


**Post-review ratio:** 343/160 managed/native ≈ 214% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetInvasionSnapshotApplyTests.VersionAllowed_AcceptsV1AndV2`
- `VmDNetInvasionSnapshotApplyTests.SpawnFlagsAllowed_RejectsUnknownBits`
- `VmDNetInvasionSnapshotApplyTests.ClampNonNegative_Floors`
- `VmDNetInvasionSnapshotApplyTests.TraceGametypeOverride_WhenDisabledAndActiveState`
- `VmDNetInvasionSnapshotApplyTests.PendingWaveFromState_OnlyCountdownCarriesWave`
- `VmDNetInvasionSnapshotApplyTests.RunHeaderValidationPipeline_RejectsBadReserved`
- `VmDNetInvasionSnapshotApplyTests.RunApplyInvasionSnapshotPipeline_AppliesV1Snapshot`
- `VmDNetInvasionSnapshotApplyTests.ValidateSpawnMetadataBlock_RejectsReservedBytes`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 516 --to 516` (0 hard / 0 soft).

## Seams

`HCDEReadBE32Field`, `HCDEApplyAuthorityEvents`, `Net_PrepareInvasionMirrorFromSnapshot`, and live globals are not hosted.

## Credit rationale

Credits invasion snapshot header validation, spawn metadata v2, wave count merge, and embedded authority/actor replay gates from `d_net.cpp` (3351–3510).
