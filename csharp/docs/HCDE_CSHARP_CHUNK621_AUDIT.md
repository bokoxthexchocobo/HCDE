# Chunk 621 — `TryReadServerQuerySnapshot` decode (`i_net.cpp` 1263–1466)

## Scope

`TryReadServerQuerySnapshot` decode (`i_net.cpp` 1263–1466)

## Native ranges

- `src/common/engine/i_net.cpp` **TryReadServerQuerySnapshot** 1263–1466 (204 lines)

**Credited native LOC:** 204 (tally `nativeLoc` 204).

## Managed

`VmINetQueryDecode.cs`
Managed LOC (tally): 204.
Conversion commit: `b34696a9548d4da81d9ed0b83f5b5bc57f2498e0`.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| `TryReadServerQuerySnapshotNative` | `TryReadServerQuerySnapshot` 1263–1466 |

### Out-of-body uncredit (621)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **204** |
| **Out-of-body removed** | **0** |
| **Honest** | **204** |


**Post-review ratio:** 204/204 managed/native ≈ 100% (src file LOC at HEAD; see tally).

## Tests

- `VmINetQueryDecodeTests.TryReadServerQuerySnapshotNative_RoundTripsViaCodec`
- `VmINetQueryDecodeTests.TryReadServerQuerySnapshotNative_RejectsShortBuffer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 621 --to 621` (0 hard / 0 soft).

## Seams

`FString` formatting errors are surfaced via `setError` callbacks only.

## Credit rationale

Whole `TryReadServerQuerySnapshot` including optional invasion tail fields.
