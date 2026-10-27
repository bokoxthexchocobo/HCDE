# Chunk 895 — `ESetAnimationFlags`, animation object registration seams (`DAnimationFrame`–`DAnimationLayer`), and `FindFModel` (`p_actionfunctions.cpp` **6351–6505**). Excludes **6331–6349** `GetObjectToWorldMatrixRaw` (behavior on **585** `VmActionSetAnimationNatives`, native still uncredited). Stops before **517** `SetAnimationInternal` (**6507+**).

## Scope

`ESetAnimationFlags`, animation object registration seams (`DAnimationFrame`–`DAnimationLayer`), and `FindFModel` (`p_actionfunctions.cpp` **6351–6505**). Excludes **6331–6349** `GetObjectToWorldMatrixRaw` (behavior on **585** `VmActionSetAnimationNatives`, native still uncredited). Stops before **517** `SetAnimationInternal` (**6507+**).

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **ESetAnimationFlags** 6351–6505 (155 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmActionModelAnimation.cs`
Managed LOC (tally): 86.
Conversion commit: `88442c5340829bc47029562cbeef2d44504058a4`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `Saf*` / `SkipSameAnimation` | `ESetAnimationFlags` **6355–6360** |
| (seams) | `DAnimationFrame` … `DAnimationLayer` **6362–6471** |
| `UseModelDefForSpriteFrames` / `ResolvePrimaryAnimId` / `TryResolveFindFModelModelIndex` | `FindFModel` **6473–6505** |

### vs **585** / **517**

| | Lines |
|--|------:|
| **GetObjectToWorldMatrixRaw** (585 file, not this row) | **6331–6349** |
| **895** (this row) | **6351–6505** (**155**) |
| **517** (`SetAnimationInternal`) | **6507+** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **155** |
| Net behavior-ported | **155** |


**Post-review ratio:** 86/155 managed/native ≈ 55% (src file LOC at HEAD; see tally).

## Tests

- `VmActionModelAnimationTests.SkipSameAnimation_WhenNooverrideAndSameFrame`
- `VmActionModelAnimationTests.ResolvePrimaryAnimId_PrefersActorSlot`
- `VmActionModelAnimationTests.TryPickModelIndex_UsesAnimId`
- `VmActionModelAnimationTests.TryResolveFindFModel_UsesActorAnimId`
- `VmActionModelAnimationTests.TryResolveFindFModel_UsesModelDataSlotWhenAnimInvalid`
- `VmActionModelAnimationTests.TryResolveFindFModel_MissingBaseSpriteKey`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 895 --to 895` (0 hard / 0 soft).

## Seams

`IMPLEMENT_CLASS` / `DEFINE_FIELD` bodies, `Models[]`, `BaseSpriteModelFrames`, and `AActor::modelData` storage are not hosted.

## Credit rationale

SAF bit policy, `FindFModel` index resolution branches, and sprite-model key guard mirrored in `VmActionModelAnimation`; ZScript class registration credited as native inventory only.
