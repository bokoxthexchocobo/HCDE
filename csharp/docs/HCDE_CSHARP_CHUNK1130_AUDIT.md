# Chunk 1130 — startDisplay player center, clearMarks, LevelInit scale, min/max window, line thickness, NewResolution, and the follow-pan gate from `am_map.cpp` (**1351–1500**); follows **1129**.

## Scope

startDisplay player center, clearMarks, LevelInit scale, min/max window, line thickness, NewResolution, and the follow-pan gate from `am_map.cpp` (**1351–1500**); follows **1129**.

## Native ranges

- `src/am_map.cpp` **Automap display init and pan gate** 1351–1500 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapDisplayInit1130Slice.cs`
Managed LOC (tally): 114.
Conversion commit: `746b9a9e6e85c3c38e1baf18f1a9780813debdb3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Initial player scan and window save (**1351–1366**) | yes |
| `clearMarks` (**1375–1381**) | yes |
| `LevelInit` background and scale (**1389–1409**) | yes |
| `minOutWindowScale` / `maxOutWindowScale` (**1417–1433**) | yes |
| `NewUIScale` half-even thickness (**1441–1456**) | yes |
| `NewResolution` ratio clamp (**1464–1483**) | yes |
| Follow-mode `+am_pan` swallow (**1492–1500**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1337–1348** | display head already credited by **1129** |
| **1408**, **1481–1482** | host `UpdateShowAllLines` / `activateNewScale` |
| **1501+** | `C_DoKey` and the rest of `Responder` |

`MAXPLAYERS` is **64**. Initial scale is `min/0.7`, and when that exceeds max the code stores **min**, not max. `RoundHalfEven` is IEEE round-ties-to-even via `llrint`. Empty map background selects **AUTOPAGE**.

| Managed (`VmAmMapDisplayInit1130Slice.cs`) | Native (`am_map.cpp`) |
|--------------------------------------------|---------------------|
| Display init, thickness, pan gate | **1351–1500** |
| Band hull (exclusive slice) | **1351–1500** |

### Chunk **1130** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1129** |


**Post-review ratio:** 114/150 managed/native ≈ 76% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapDisplayInit1130SliceTests.FindInitialPlayer_SkipsConsoleWhenAbsentAndCentersCamera`
- `VmAmMapDisplayInit1130SliceTests.ClearMarks_EmptiesSlotsAndLevelScaleFallsBackToMinimum`
- `VmAmMapDisplayInit1130SliceTests.LineThickness_RoundsHalfEvenAndResolutionKeepsRatio`
- `VmAmMapDisplayInit1130SliceTests.SuppressesFollowPan_OnlyForAutomapPanBinds`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1130 --to 1130` (0 hard / 0 soft).

## Seams

`TexMan`, `UpdateShowAllLines`, and `C_DoKey` stay host-injected. Responder continues at **1501**.

## Credit rationale

UNCredited **`am_map.cpp`** **1351–1500** (+150), inside the 3594-line file, with no conversion or recognition overlap.
