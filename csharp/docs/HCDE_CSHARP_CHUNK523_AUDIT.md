# Chunk 523 — CheckLevelStart

## Scope

CheckLevelStart

## Native ranges

- `src/d_net.cpp` **CheckLevelStart** 5641–5792 (152 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Managed

`VmDNetCheckLevelStart.cs`, `VmDNetCheckLevelStartTests.cs`
Managed LOC (tally): 82.
Conversion commit: `229b6e72f7de12b28435ce1cb17d17e7d65f70f4`.

## Tests

- `VmDNetCheckLevelStartTests.ParseGameticFromBuffer_BigEndian`
- `VmDNetCheckLevelStartTests.LateJoinSeat_WhenGameticAhead`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 523 --to 523` (0 hard / 0 soft).

## Seams

`HSendPacket`, `TryReleaseLevelStart`, `P_ClearPredictionData`, and `Net_SetLevelStartStatus` are not hosted.

## Credit rationale

Credits level-start ack/resend, gametic alignment, late-join seat fast-forward, and prediction-lead clamp from `d_net.cpp` (5641–5792).
