# Chunk 709 — Whole `P_TryMove` body (`p_map.cpp` 2368–2882). Wrapper overload 2884–2890 excluded. Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

Whole `P_TryMove` body (`p_map.cpp` 2368–2882). Wrapper overload 2884–2890 excluded. Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `6040a4c9495c09147bd499fa1580b0c75cc7c863`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `TryMove` (portal loop, commit, spechit cross, heightsec, `portalstep` relink) | `P_TryMove` 2368–2882 |
| `ShouldCollideWhenBlocked` / `PassMobjBlocked` | 2391–2420 |
| `HeightCrush` / `MustLowerToFit` / `FlyCeilingBlocks` / `MissileStepBlocked` / `StepUpTooHigh` | 2437–2491 |
| `DropoffBlocked` / `DropoffCompatDisables` / `LargeDogDropoffCancel` | 2514–2547 |
| `CantLeaveFloorPic` / `BotDangerMoveBlocked` / `MbfBouncerStepBlocked` | 2561–2610 |
| pushline spechit / blasted damage | 2854–2880 |

### Out-of-body uncredit (709)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **515** |
| **Out-of-body removed** | **515** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 709 --to 709` (0 hard / 0 soft).

## Seams

`P_CheckPosition`, `P_TestMobjZ`, `P_CollidedWith`, portal translation, `P_ActivateLine`, `CheckForPushSpecial`, and sector actions are injected.

## Credit rationale

Single-function oversize exception (515 native LOC): must not split across chunks.
