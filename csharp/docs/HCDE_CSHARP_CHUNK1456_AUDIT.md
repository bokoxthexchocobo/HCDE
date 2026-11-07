# Chunk 1456 — Invasion announcements and level-start status in d_net.cpp (1506-1738).

## Scope

Invasion announcements and level-start status in d_net.cpp (1506-1738).

## Native ranges

- `src/d_net.cpp` **Net_IsInvasionRoundActiveState** 1506–1738 (233 lines)

**Credited native LOC:** 233 (tally `nativeLoc` 233).

## Managed

`VmNet1456Slice.cs`
Managed LOC (tally): 251.
Conversion commit: `ebbe1c14b287fabeacaef50557b8230e1325526e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Announcements and invasion state (**1506–1738**) | yes |
| Band hull (**1506–1738**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| CVAR block | stays above this span |
| Prototype declarations | start at line 1740 |

A wave completion is announced only when combat gives way to intermission or victory. Countdown seconds are suppressed when they repeat. Leaving an active round clears the wipe grace. A level-start change to waiting resets the local baseline only for a non-authority net client.

| Managed (`VmNet1456Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| RoundActive through SetInvasionState | **1506–1738** |
| Band hull (exclusive slice) | **1506–1738** |

### Chunk **1456** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **233** |
| **Net headline** | **+233** vs the prior chunk |


**Post-review ratio:** 251/233 managed/native ≈ 108% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1456SliceTests.AnnouncementsAndOrdinals`
- `VmNet1456SliceTests.LevelStartHudAndState`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1456 --to 1456` (0 hard / 0 soft).

## Seams

Net_IsInvasionRoundActiveState through Net_SetInvasionState. The CVAR block ends before this span. D_ProcessEvents is declared at 1740.

## Credit rationale

UNCredited d_net.cpp 1506-1738 (+233). One contiguous span of the announcement ticker, level-start transition, lag HUD snapshot, and invasion state change. The span does not cross credited code.
