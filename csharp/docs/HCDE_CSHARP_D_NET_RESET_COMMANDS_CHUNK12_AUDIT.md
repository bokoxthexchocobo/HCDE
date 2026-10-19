# d_net.cpp reset commands + cutscene ready — chunk 12 audit

## Scope

`NetRoomCommandResetSession` ports `Net_ResetCommands` / `Net_ClearStaleWaitingFlags` room adoption and per-client sequence re-anchor. `CutsceneReadyCoordinator` ports solo-human latch behavior from `Net_PlayerReadiedUp`. Gameplay envelope room validation remains recognition credit in `GameplayEnvelopeCodec`.

**Conversion native LOC credited:** 1,528.

## Tests

- `NetRoomCommandResetTests.ResetCommands_AdoptsPendingRoomAndReanchorsSequences`
- `NetRoomCommandResetTests.CutsceneReady_SoloHuman_LatchesReady`

## Verified

`dotnet build -warnaserror`; `HCDE.Net.Core.Tests`; full `HCDE.Playsim.Tests`.
