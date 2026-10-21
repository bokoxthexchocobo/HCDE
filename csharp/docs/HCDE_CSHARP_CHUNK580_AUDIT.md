# Chunk 580 — SetViewRollNative, GetVar/GetArrayVar, user var/array setter guards (3096–3223)

## Scope

SetViewRollNative, GetVar/GetArrayVar, user var/array setter guards (3096–3223)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **SetViewRollNative** 3096–3104 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **GetVar** 3123–3134 (12 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetUserVar** 3143–3148 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetUserVarFloat** 3157–3163 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **GetArrayVar** 3172–3190 (19 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetUserArray** 3199–3206 (8 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetUserArrayFloat** 3216–3223 (8 lines)

**Credited native LOC:** 69 (tally `nativeLoc` 69).

## Managed

`VmActionUserFieldAccess.cs`
Managed LOC (tally): 35.
Conversion commit: `64a52646a6266eba53ee1440b104b50d3d975d4a`.

## Padding review (2026-10-21)

**Fidelity review:** `SetViewRollNative` credits `3099–3102`. `GetVar`/`GetArrayVar` credit rejection branches. Setter credits mirror `if (var != nullptr)` blocks in `A_SetUserVar*` / `A_SetUserArray*`.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `SetViewRollOnRef` | `SetViewRollNative` 3099–3102 |
| `IsUserScalarField` | `GetVar` 3127–3133 |
| `IsUserArrayField` / `ArrayIndexInBounds` | `GetArrayVar` 3176–3188 |
| `ShouldSetUserScalar` | `A_SetUserVar` / `A_SetUserVarFloat` 3144–3147, 3159–3162 |
| `ShouldSetUserArrayElement` | `A_SetUserArray` / `A_SetUserArrayFloat` 3201–3205, 3218–3222 |

**Post-review ratio:** 35/69 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmActionUserFieldAccessTests.SetViewRollOnRef_RequiresActor`
- `VmActionUserFieldAccessTests.IsUserScalarField_AcceptsScalar`
- `VmActionUserFieldAccessTests.IsUserScalarField_RejectsNativeFlag`
- `VmActionUserFieldAccessTests.ShouldSetUserScalar_WhenFieldValid`
- `VmActionUserFieldAccessTests.IsUserArrayField_RejectsNonArray`
- `VmActionUserFieldAccessTests.ArrayIndexInBounds_RejectsNegative`
- `VmActionUserFieldAccessTests.ShouldSetUserArrayElement_RequiresIndexOk`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 580 --to 580` (0 hard / 0 soft).

## Seams

`COPY_AAPTR`, `FindSymbol`, `Printf`, `SetValue`, and `SetViewRoll` are not hosted.

## Credit rationale

Credits field lookup validation and non-null setter branches for user scalars/arrays.
