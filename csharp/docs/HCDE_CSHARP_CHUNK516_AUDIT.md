# Chunk 516 — HCDEApplyInvasionSnapshot

## Scope

HCDEApplyInvasionSnapshot

## Native ranges

- `src/d_net.cpp` **HCDEApplyInvasionSnapshot** 3351–3510 (160 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Managed

`VmDNetInvasionSnapshotApply.cs`
Managed LOC (tally): 97.
Conversion commit: `330afd54a5f690af6c4a785a0b586f0d4aba3254`.

## Padding review (2026-10-21)

| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `ExpectActorDeltas` / `HeaderBytesAvailable` / `MagicMatches` / `InvasionModeDisabledLocally` / `AcceptSnapshotWhenGametypeZero` / `VersionHasSpawnMetadata` / `SnapshotHeaderSize` / `VersionAllowed` / `ReservedZero` / `FlagsAllowed` / `StateRawAllowed` / `SpawnFlagsAllowed` / `SpawnFallbackSourceAllowed` / `ClampNonNegative` / `BossWaveFlags` / `UsingFallback` / `ShouldApplyAuthorityEvents` / `ShouldApplyActorDeltas` / `TraceGametypeOverride` / `InvasionSnapshotHeaderV1Size` / `InvasionSnapshotHeaderV2Size` / `InvasionSnapshotVersionOffset` / `InvasionSnapshotFlagsOffset` / `InvasionSnapshotStateOffset` / `InvasionSnapshotReservedOffset` / `InvasionSnapshotStateTicsOffset` / `ReadFieldsSucceeded` / `SpawnMetadataReadSucceeded` / `ReservedSpawnBytesZero` / `ReplicatedActiveMonsters` / `WaveDirectorSpawned` / `WaveDirectorCleared` / `SpawnDirectoryBudget` / `SpawnDirectoryTag` / `PreviousStateDiffers` / `PreviousWaveDiffers` | `HCDEApplyInvasionSnapshot` 3351–3510 (3353: `if (!HCDEIsValidLiveClient(clientNum))`; 3354: `return false;`; 3355: `if (!HCDELivePeerHasCapability(clientNum, HCDELiveCapInvasionSnapshot…`) |

### Out-of-body uncredit (516)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **160** |
| **Out-of-body removed** | **0** |
| **Honest** | **160** |


**Post-review ratio:** 97/160 managed/native ≈ 61% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetInvasionSnapshotApplyTests.VersionAllowed_AcceptsV1AndV2`
- `VmDNetInvasionSnapshotApplyTests.SpawnFlagsAllowed_RejectsUnknownBits`
- `VmDNetInvasionSnapshotApplyTests.ClampNonNegative_Floors`
- `VmDNetInvasionSnapshotApplyTests.TraceGametypeOverride_WhenDisabledAndActiveState`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 516 --to 516` (0 hard / 0 soft).

## Seams

`HCDEReadBE32Field`, `HCDEApplyAuthorityEvents`, `Net_PrepareInvasionMirrorFromSnapshot`, and live globals are not hosted.

## Credit rationale

Credits invasion snapshot header validation, spawn metadata v2, wave count merge, and embedded authority/actor replay gates from `d_net.cpp` (3351–3510).
