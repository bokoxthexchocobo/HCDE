# Chunk 582 — A_Weave index masking and XY/Z weave guards (3484–3524)

## Scope

A_Weave index masking and XY/Z weave guards (3484–3524)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_Weave** 3484–3525 (42 lines)

**Credited native LOC:** 42 (tally `nativeLoc` 42).

## Managed

`VmActionWeave.cs`
Managed LOC (tally): 22.
Conversion commit: `74ac072e39c124a628815acdc0df095c03174229`.

## Padding review (2026-10-21)

**Fidelity review:** Credits `3491–3492`, `3495–3496`, `3502–3515`, `3518–3523`. `DEFINE_ACTION_FUNCTION` wrapper is a seam.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `WeaveIndexXY` / `WeaveIndexZ` | 3491–3492 |
| `ShouldWeaveXY` / `ShouldWeaveZ` | 3495–3496, 3518–3519 |
| `AdvanceWeaveIndex` | 3499–3500, 3521–3522 |
| `UseTryMoveWhenInteractive` | 3502–3515 |

**Post-review ratio:** 22/42 managed/native ≈ 52% (src file LOC at HEAD; see tally).

## Tests

- `VmActionWeaveTests.WeaveIndexXY_MasksToSixBits`
- `VmActionWeaveTests.ShouldWeaveXY_WhenDistAndSpeedNonZero`
- `VmActionWeaveTests.AdvanceWeaveIndex_WrapsAt64`
- `VmActionWeaveTests.UseTryMoveWhenInteractive_WithoutNointeraction`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 582 --to 582` (0 hard / 0 soft).

## Seams

`BobSin`, `P_TryMove`, `UnlinkFromWorld`, and `SetXY` are not hosted.

## Credit rationale

Credits weave index masking, non-zero dist/speed gates, index advance, and MF5_NOINTERACTION branch selection.
