# Chunk 601 — Net_DoCommand whole `case` blocks: SAY through SUICIDE (8553–8741); CENTERVIEW credited on chunk616 Merged: [602, 604].

## Scope

Net_DoCommand whole `case` blocks: SAY through SUICIDE (8553–8741); CENTERVIEW credited on chunk616 Merged: [602, 604].

## Native ranges

- `src/d_net.cpp` **Net_DoCommand** 8553–8598 (46 lines)
- `src/d_net.cpp` **Net_DoCommand** 8600–8620 (21 lines)
- `src/d_net.cpp` **Net_DoCommand** 8646–8671 (26 lines)
- `src/d_net.cpp` **Net_DoCommand** 8673–8680 (8 lines)
- `src/d_net.cpp` **Net_DoCommand** 8682–8693 (12 lines)
- `src/d_net.cpp` **Net_DoCommand** 8695–8705 (11 lines)
- `src/d_net.cpp` **Net_DoCommand** 8707–8725 (19 lines)
- `src/d_net.cpp` **Net_DoCommand** 8727–8737 (11 lines)
- `src/d_net.cpp` **Net_DoCommand** 8739–8741 (3 lines)

**Credited native LOC:** 157 (tally `nativeLoc` 157).

## Managed

`VmDNetDoCommandChat.cs`
Managed LOC (tally): 83.
Conversion commit: `5d5cc3fed32c0870969dded44e24ca2550c56dd3`.

## Padding review (2026-10-21)

Case-boundary credit only (not tuned to 150). `MustConsumeSetInvPaddingWhenRejectedNative` / `ShouldBreakAfterSayWhenDisabledNative` mirror native `if`/`break` at 8560–8561 and 8689–8691.

| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `ShouldDropSayNative` / chat visibility helpers | `DEM_SAY` 8553–8598 |
| `FormatTauntSoundNative` / `ShouldPlayTauntOnPawnNative` | `DEM_TAUNT` 8600–8620 |
| `ShouldExecuteGiveCheatNative` / `ShouldLogRemoteGiveNative` | `DEM_GIVECHEAT` 8646–8671 |
| `ShouldExecuteTakeCheatNative` | `DEM_TAKECHEAT` 8673–8680 |
| `ShouldExecuteSetInvCheatNative` / `MustConsumeSetInvPaddingWhenRejectedNative` | `DEM_SETINV` 8682–8693 |
| `ShouldExecuteWarpCheatNative` | `DEM_WARPCHEAT` 8695–8705 |
| `ShouldExecuteGenericCheatNative` | `DEM_GENERICCHEAT` 8707–8725 |
| `ApplyChangemapCheatFlagNative` | `DEM_CHANGEMAP`/`DEM_CHANGEMAP2` 8727–8737 |
| `ShouldRunSuicideCommandNative` | `DEM_SUICIDE` 8739–8741 |

### Out-of-body uncredit (601)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **157** |
| **Out-of-body removed** | **0** |
| **Honest** | **157** |


**Post-review ratio:** 83/157 managed/native ≈ 53% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetDoCommandChatTests.ShouldDropSayNative_WhenChatDisabled`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 601 --to 601` (0 hard / 0 soft).

## Seams

`Printf`, `S_Sound`, `P_TeleportMove`, `ChangeLevel`, and `cht_*` are not hosted.

## Credit rationale

Each native range is a full `case DEM_*` block (or cheat guard slice) chosen before measuring; gaps 8622–8644 are unported print/music/info cases.
