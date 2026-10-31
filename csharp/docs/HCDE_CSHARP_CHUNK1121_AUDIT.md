# Chunk 1121 — Automap mark/zoom constants and `am_cheat` / `am_overlay` gates from `am_map.cpp` (**1–150**). `p_mobj.cpp` has no further in-file ≥150 window clear of tally overlap.

## Scope

Automap mark/zoom constants and `am_cheat` / `am_overlay` gates from `am_map.cpp` (**1–150**). `p_mobj.cpp` has no further in-file ≥150 window clear of tally overlap.

## Native ranges

- `src/am_map.cpp` **Automap constants and cheat overlay gates** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapHead1121Slice.cs`
Managed LOC (tally): 91.
Conversion commit: `ef9f674ff4a12fe15fb3d695ed6508ec97e0bf05`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Mark count and player radius (**68–72**) | yes |
| Zoom in/out and legacy factors (**73–75**) | yes |
| `am_cheat` net clamp (**136–139**) | yes |
| `am_overlay` clears active map (**146–150**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–58** | license and includes |
| **118–131** | host CVAR callbacks |
| **151+** | later CVARs outside hull |

| Managed (`VmAmMapHead1121Slice.cs`) | Native (`am_map.cpp`) |
|-------------------------------------|---------------------|
| Zoom constants + cheat/overlay gates | **1–150** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **1121** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1120** |


**Post-review ratio:** 91/150 managed/native ≈ 61% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapHead1121SliceTests.ZoomIn_DoublesScaleEachSecond`
- `VmAmMapHead1121SliceTests.ApplyLegacyZoom_UsesOldFactors`
- `VmAmMapHead1121SliceTests.ClampAutomapCheat_BlocksNetWithoutCheats`
- `VmAmMapHead1121SliceTests.ApplyOverlayChange_TurnsAutomapOffWhenOverlayCleared`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1121 --to 1121` (0 hard / 0 soft).

## Seams

`UpdateShowAllLines` and CVAR registration remain host-injected; overlay body past **150** continues at **151**.

## Credit rationale

UNCredited **`am_map.cpp`** **1–150** (+150), inside the 3594-line file, with no conversion or recognition overlap. Past-EOF `p_mobj` windows are not used.
