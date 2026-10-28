# Chunk 578 — PlayerSkin/CLOF trace and user field access (consolidated chunk578+580)

## Scope

PlayerSkin/CLOF trace and user field access (consolidated chunk578+580)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **PlayerSkinCheck** 2220–2226 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **CLOF_flags** 2239–2276 (38 lines)
- `src/playsim/p_actionfunctions.cpp` **CheckLOFTraceFunc** 2286–2356 (71 lines)
- `src/playsim/p_actionfunctions.cpp` **SetViewRollNative** 3096–3104 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **GetVar** 3123–3134 (12 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetUserVar** 3143–3148 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetUserVarFloat** 3157–3163 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **GetArrayVar** 3172–3190 (19 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetUserArray** 3199–3206 (8 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetUserArrayFloat** 3216–3223 (8 lines)

**Credited native LOC:** 185 (tally `nativeLoc` 185).

## Managed

`VmActionPlayerSkinCheck.cs`, `VmActionCheckLofTrace.cs`, `VmActionUserFieldAccess.cs`
Managed LOC (tally): 137.
Conversion commit: `9f3c4c1828c7d3dfc7239aa0a34770c64bb3bbca`.

## Padding review (2026-10-21)

**Superseded chunk numbers:** 580 (retired; credit lives only in chunk578.json / tally row `player-skin-userfield-chunk578`).

**Fidelity review:** `PlayerSkinCheck` excludes `Skins[...]` (`2224–2225`). `CheckLOFTraceFunc` credits `2286–2356` (full callback decision tree). `CLOF_flags` enum credited as constants used by the trace helper.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `PlayerSkinIsOtherGame` | `PlayerSkinCheck` 2224–2225 |
| `EvaluateHitActor` | `CheckLOFTraceFunc` 2291–2355 |

**Fidelity review:** `SetViewRollNative` credits `3099–3102`. `GetVar`/`GetArrayVar` credit rejection branches. Setter credits mirror `if (var != nullptr)` blocks in `A_SetUserVar*` / `A_SetUserArray*`.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `SetViewRollOnRef` | `SetViewRollNative` 3099–3102 |
| `IsUserScalarField` | `GetVar` 3127–3133 |
| `IsUserArrayField` / `ArrayIndexInBounds` | `GetArrayVar` 3176–3188 |
| `ShouldSetUserScalar` | `A_SetUserVar` / `A_SetUserVarFloat` 3144–3147, 3159–3162 |
| `ShouldSetUserArrayElement` | `A_SetUserArray` / `A_SetUserArrayFloat` 3201–3205, 3218–3222 |

**Post-review ratio:** 137/185 managed/native ≈ 74% (src file LOC at HEAD; see tally).

## Tests

- `VmActionPlayerSkinCheckTests.PlayerSkinIsOtherGame_WhenPlayerAndOtherSkin`
- `VmActionCheckLofTraceTests.EvaluateHitActor_StopsOnJumpEnemy`
- `VmActionCheckLofTraceTests.EvaluateHitActor_SkipsNonShootableWhenRequired`
- `VmActionCheckLofTraceTests.EvaluateHitActor_AbortsAndSetsBadActor`
- `VmActionUserFieldAccessTests.SetViewRollOnRef_RequiresActor`
- `VmActionUserFieldAccessTests.IsUserScalarField_AcceptsScalar`
- `VmActionUserFieldAccessTests.IsUserScalarField_RejectsNativeFlag`
- `VmActionUserFieldAccessTests.ShouldSetUserScalar_WhenFieldValid`
- `VmActionUserFieldAccessTests.IsUserArrayField_RejectsNonArray`
- `VmActionUserFieldAccessTests.ArrayIndexInBounds_RejectsNegative`
- `VmActionUserFieldAccessTests.ShouldSetUserArrayElement_RequiresIndexOk`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 578 --to 578` (0 hard / 0 soft).

## Seams

`Skins[]`, `P_CheckSight`-style hostility, and `Trace()` are not hosted. `COPY_AAPTR`, `FindSymbol`, `Printf`, `SetValue`, and `SetViewRoll` are not hosted.

## Credit rationale

Skin guard and CLOF trace callback branching are ported; table lookup and engine trace are seams. Credits field lookup validation and non-null setter branches for user scalars/arrays.
