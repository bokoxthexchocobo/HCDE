# d_net.cpp lag fractions and authority wait watchdog — chunk 15 audit

## Scope

`NetLagPresentationPolicy` ports `Net_IsWaiting`, `Net_ModifyFrac`, `Net_ModifyObjectFrac`, and `Net_ModifyParticleFrac`. `AuthorityWaitWatchdogSession` ports `Net_ResetAuthorityWaitWatchdog` grace timing and waiting-flag clear. `CutsceneSoloClientRecoverySession` ports guest solo `Net_TickCutsceneClientRecovery` timeout.

**Conversion native LOC credited:** 1,512.

## Tests

- `NetLagAndCutsceneRecoveryTests.ModifyFrac_WaitingState_ForcesUnity`
- `NetLagAndCutsceneRecoveryTests.ModifyParticleFrac_WaitingState_ZerosParticles`
- `NetLagAndCutsceneRecoveryTests.AuthorityWaitWatchdog_ClearsStaleWaitingFlags`
- `NetLagAndCutsceneRecoveryTests.SoloClientRecovery_ForcesEndAfterDelay`

## Verified

`dotnet build -warnaserror`; full `dotnet test`.
