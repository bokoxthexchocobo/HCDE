# Chunk 742 — Local view, render visibility, conversation anim, touch VM gate, and massacre cheat (`p_mobj.cpp` 1321–1542, 1715–1741). Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-grind-chunk734 src/playsim/p_mobj.cpp 1554-1685).

## Scope

Local view, render visibility, conversation anim, touch VM gate, and massacre cheat (`p_mobj.cpp` 1321–1542, 1715–1741). Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-grind-chunk734 src/playsim/p_mobj.cpp 1554-1685).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppLumpGate.cs`
Managed LOC (tally): 0.
Conversion commit: `3025bdb8f38e6031c222bf6068dbc14f49723562`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `CheckLocalView` | `AActor::CheckLocalView` 1321–1342 |
| `DisableLocalRendering` | `AActor::DisableLocalRendering` 1351–1355 |
| `ShouldRenderLocally` | `AActor::ShouldRenderLocally` 1372–1375 |
| `IsInsideVisibleAngles` | `AActor::IsInsideVisibleAngles` 1397–1450 |
| `IsVisibleToPlayer` | `AActor::IsVisibleToPlayer` 1462–1495 |
| `ConversationAnimation` | `AActor::ConversationAnimation` 1508–1524 |
| `CallTouch` | `AActor::CallTouch` 1535–1542 |
| `Massacre` | `AActor::Massacre` 1715–1741 |

### Out-of-body uncredit (742)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **171** |
| **Out-of-body removed** | **0** |
| **Honest** | **171** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 742 --to 742` (0 hard / 0 soft).

## Seams

`Level->GetConsolePlayer`, angle math, `FindState`/`SetState`, `VMCall`, and `P_DamageMobj` are injected.

## Credit rationale

Whole actor methods; merged `AActor::Massacre` (uncredited, same TU) instead of crediting `DEFINE_ACTION_FUNCTION(AActor, CheckLocalView)` as a 150-LOC floor top-up.
