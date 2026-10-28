# Chunks 13–15 native LOC correction (addendum)

Earlier `nativeLoc` fields for chunks 13–15 (~1,500 each) did not match the credited `nativeRanges` in `loc_tally.json` (ranges summed to 277 / 115 / 64). Those headline numbers are corrected to equal the trimmed ranges below.

## Trims (partial native vs managed)

| Chunk | Adjustment |
|-------|------------|
| **13** | `Net_ResetClientState` credited only cursor/presentation resets (5395–5400), not full client struct zeroing. `DisconnectClient` only idempotent guard + bitmask clears + `ResetClientState` call. `Net_CheckCutsceneReady` omits VM `ScreenJobRunner` skip-type probe (4798–4804). `Net_StartCutscene` omits invasion-only tail (4711–4712). Vote path omits debug-only `Printf` block (4769–4773). `Net_AdvanceCutscene` omits invasion/DEM side (4841–4845). |
| **14** | `PCD_ENDHUDMESSAGE` only default branch (9170–9194), not fade/type-on variants. Added tested `PCD_PRINTNUMBER` / `PCD_PRINTHEX` / minimal `PCD_ENDPRINT` finish lines. Scope does not include char-array print, `SetFont`, or `EndLog` despite chunk 14 note. |
| **15** | `Net_ResetAuthorityWaitWatchdog` without trace logging (4866–4871). `Net_TickCutsceneClientRecovery` core timeout only (4675–4694). `Net_ModifyObjectFrac` remains implemented but uncredited (no dedicated test). |

## Revised credits

| Chunk | Old native | New native | Managed (unchanged) |
|-------|------------|------------|---------------------|
| 13 d_net cutscene/join | 1,562 | **137** | 420 |
| 14 ACS print/HUD | 1,548 | **56** | 380 |
| 15 d_net lag/wait | 1,512 | **37** | 120 |

`convertedBehaviorNative` decreases by **4,392**; cumulative totals in `loc_tally.json` were updated to match.
