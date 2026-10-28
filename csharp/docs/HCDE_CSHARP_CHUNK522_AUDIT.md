# Chunk 522 — HCDEAppendCanonicalEventPayload (merged chunk 523) (merged chunk 523) (merged chunk 523) Retired (2026-10-24): `--band` hull overlaps existing credit (d-net-reset-commands-chunk12 src/d_net.cpp 4623-4660).

## Scope

HCDEAppendCanonicalEventPayload (merged chunk 523) (merged chunk 523) (merged chunk 523) Retired (2026-10-24): `--band` hull overlaps existing credit (d-net-reset-commands-chunk12 src/d_net.cpp 4623-4660).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `20b6eeaa8a2dd6be6974ea23469d2918ae16f9af`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-21):** `TryBuildFromLegacy` mirrors `HCDEAppendCanonicalEventPayload` switch **3747–3899** (`d_net.cpp`). `VmDNetCheckLevelStart` helpers mirror `CheckLevelStart` **5641–5791** (merged chunk 523). Excluded `HSendPacket`, `TryReleaseLevelStart`, `P_ClearPredictionData` bodies except where cited.

| Managed | Native (`d_net.cpp`) |
|---------|----------------------|
| `TryBuildFromLegacy` | `HCDEAppendCanonicalEventPayload` **3747–3899** |
| `ShouldResendLevelReady` / `LevelReadyPacketSize` | early resend path **5643–5657** |
| `ParseGameticFromBuffer` | **5669** |
| `ShouldAlignGametic` | **5670–5685** |
| `LateJoinSeat` | **5709–5710** |
| `SeatTicDup` | **5712** |
| `ClampDesiredLead` | **5726–5727** |
| `SeatClientTic` | **5728** |
| `SeatSequence` / `MaxSequence` | **5729–5735** |
| `ShouldClampPredictionLead` | **5754–5755** |
| `MaxClientTic` / `ClientTicAboveMax` | **5758–5766** |
| `PlayTic` / `AppliedSequenceAnchor` | **5775–5776** |
| `AckClient` | **5787** |
| `WaitingForStart` | guard **5643** (`LST_WAITING`) |
| `TryReleaseAfterAck` | **5791** (`TryReleaseLevelStart` seam) |
| `ConsolePlayerInRange` | **5730–5731**, **5755** |
| `GameticSnapSafeOnMapChange` / `PreserveSequenceOnMapChange` | comments **5672–5683** (policy only) |
| `AppliedSequenceDiagnosticFix` | **5769–5776** |
| `DebugLevelStartDelay` / `ResetAuthorityWatchdog` / `LogAuthorityStart` | **5667**, **5779**, **5780–5783** (diagnostic seams) |

### Out-of-body uncredit (522)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **211** |
| **Out-of-body removed** | **0** |
| **Honest** | **211** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 522 --to 522` (0 hard / 0 soft).

## Seams

`HCDEAppendFieldBytes`, `HCDEAppendCanonicalNullString`, and `HCDEReadBE16Field` are not hosted.

## Credit rationale

Credits canonical demo-event payload translation for SAY/cheat/ZSC/slot families from `d_net.cpp` (3747–3900).
