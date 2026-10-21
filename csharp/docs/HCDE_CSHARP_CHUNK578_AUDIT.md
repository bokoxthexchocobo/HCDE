# Chunk 578 — PlayerSkinCheck, CLOF_flags, CheckLOFTraceFunc (2220–2356)

## Scope

PlayerSkinCheck, CLOF_flags, CheckLOFTraceFunc (2220–2356)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **PlayerSkinCheck** 2220–2226 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **CLOF_flags** 2239–2276 (38 lines)
- `src/playsim/p_actionfunctions.cpp` **CheckLOFTraceFunc** 2286–2356 (71 lines)

**Credited native LOC:** 116 (tally `nativeLoc` 116).

## Managed

`VmActionPlayerSkinCheck.cs`, `VmActionCheckLofTrace.cs`
Managed LOC (tally): 102.
Conversion commit: `9f3c4c1828c7d3dfc7239aa0a34770c64bb3bbca`.

## Padding review (2026-10-21)

**Fidelity review:** `PlayerSkinCheck` excludes `Skins[...]` (`2224–2225`). `CheckLOFTraceFunc` credits `2286–2356` (full callback decision tree). `CLOF_flags` enum credited as constants used by the trace helper.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `PlayerSkinIsOtherGame` | `PlayerSkinCheck` 2224–2225 |
| `EvaluateHitActor` | `CheckLOFTraceFunc` 2291–2355 |

**Post-review ratio:** 102/116 managed/native ≈ 88% (src file LOC at HEAD; see tally).

## Tests

- `VmActionPlayerSkinCheckTests.PlayerSkinIsOtherGame_WhenPlayerAndOtherSkin`
- `VmActionCheckLofTraceTests.EvaluateHitActor_StopsOnJumpEnemy`
- `VmActionCheckLofTraceTests.EvaluateHitActor_SkipsNonShootableWhenRequired`
- `VmActionCheckLofTraceTests.EvaluateHitActor_AbortsAndSetsBadActor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 578 --to 578` (0 hard / 0 soft).

## Seams

`Skins[]`, `P_CheckSight`-style hostility, and `Trace()` are not hosted.

## Credit rationale

Skin guard and CLOF trace callback branching are ported; table lookup and engine trace are seams.
