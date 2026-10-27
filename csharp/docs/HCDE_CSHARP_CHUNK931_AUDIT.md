# Chunk 931 — `LS_Line_SetTextureScale` close through `LS_ChangeCamera` head (`p_lnspec.cpp` **2719–2868**). Follows **930** (**≤2718**); `ChangeCamera` tail and **2882+** deferred.

## Scope

`LS_Line_SetTextureScale` close through `LS_ChangeCamera` head (`p_lnspec.cpp` **2719–2868**). Follows **930** (**≤2718**); `ChangeCamera` tail and **2882+** deferred.

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Line_SetBlocking** 2719–2868 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPLnspecBlockingAutomapCamera931Slice.cs`
Managed LOC (tally): 114.
Conversion commit: `f30eea47e13ed88e2acbbdd1710f967f75c23bf1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_lnspec.cpp`) |
|---------|-------------------------|
| Texture scale close | **2715–2719** |
| `BlockingRejectZeroId` / `DecodeBlockingArgs` / `ApplyBlocking` | `LS_Line_SetBlocking` **2721–2765** |
| `AutomapFlagsRejectZeroId` / `DecodeAutomapFlags` / `ApplyAutomapFlags` | `LS_Line_SetAutomapFlags` **2767–2798** |
| `AutomapStyleInRange` | `LS_Line_SetAutomapStyle` **2800–2815** |
| `BeginChangeCamera` / `ShouldIterateAllPlayers` | `LS_ChangeCamera` **2817–2858** |
| `SelectCameraForPlayer` / `UpdateCheatsForCamera` | **2842–2871** (through **2868** in band) |
| `ShouldClearViewerOnCameraChange` | **2853–2856**, **2873–2876** (seam) |
| `ResolveCameraFromTid` | **2821–2829** |

### Chunk **931** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**930** |


**Post-review ratio:** 114/150 managed/native ≈ 76% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecBlockingAutomapCamera931SliceTests.Blocking_RejectsZeroLineId`
- `VmPLnspecBlockingAutomapCamera931SliceTests.Blocking_DecodesSetAndClearWords`
- `VmPLnspecBlockingAutomapCamera931SliceTests.AutomapFlags_RejectsZeroId`
- `VmPLnspecBlockingAutomapCamera931SliceTests.AutomapStyle_RejectsOutOfRange`
- `VmPLnspecBlockingAutomapCamera931SliceTests.ChangeCamera_SetsRevertCheatWhenArg2`
- `VmPLnspecBlockingAutomapCamera931SliceTests.ChangeCamera_IteratesAllPlayersWhenWhoSet`
- `VmPLnspecBlockingAutomapCamera931SliceTests.ApplyBlocking_MergesFlags`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 931 --to 931` (0 hard / 0 soft).

## Seams

Line iterators, `line_t` flag mutation, player iteration, `R_ClearPastViewer`, and `AMLS_COUNT` remain host seams.

## Credit rationale

Honest **150**-line slice in **2269–3129** corridor; exclusive `VmPLnspecBlockingAutomapCamera931Slice.cs`.
