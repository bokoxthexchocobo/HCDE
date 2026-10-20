# Chunk 516 — HCDEApplyInvasionSnapshot

## Scope

HCDEApplyInvasionSnapshot

## Native ranges

- `src/d_net.cpp` **HCDEApplyInvasionSnapshot** 3351–3510 (160 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Managed

(see conversion commit file list)
Managed LOC (tally): 119.
Conversion commit: `dbdfff03bd097109aab106c2f30253291bd22701`.

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
