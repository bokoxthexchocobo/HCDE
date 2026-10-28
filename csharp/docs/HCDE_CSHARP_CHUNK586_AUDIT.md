# Chunk 586 — ESetAnimationFlags, model animation DObject shells, FindFModel (p_actionfunctions.cpp 6351–6505)

## Scope

ESetAnimationFlags, model animation DObject shells, FindFModel (p_actionfunctions.cpp 6351–6505)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **ESetAnimationFlags** 6351–6360 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **DAnimationFrame** 6362–6471 (110 lines)
- `src/playsim/p_actionfunctions.cpp` **FindFModel** 6473–6505 (33 lines)

**Credited native LOC:** 153 (tally `nativeLoc` 153).

## Managed

`VmActionModelAnimation.cs`
Managed LOC (tally): 79.
Conversion commit: `987f69cd57cf0c8a4ac958931189ee854d3505b6`.

## Padding review (2026-10-21)

**Fidelity review:** `6362–6471` is credited as native inventory only (`IMPLEMENT_CLASS` / `DEFINE_FIELD` seams); managed code mirrors `FindFModel` and SAF bits, not ZScript registration.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `Saf*` / `SkipSameAnimation` | `ESetAnimationFlags` 6355–6360 |
| (seams) | `DAnimationFrame` … `DAnimationLayer` 6362–6471 |
| `HasBaseSpriteModelFrames` / `UseModelDefForSpriteFrames` / `ResolvePrimaryAnimId` / `TryPickModelIndex` | `FindFModel` 6473–6505 |

**Post-review ratio:** 79/153 managed/native ≈ 52% (src file LOC at HEAD; see tally).

## Tests

- `VmActionModelAnimationTests.SkipSameAnimation_WhenNooverrideAndSameFrame`
- `VmActionModelAnimationTests.AnimationLayerHasPointerFields`
- `VmActionModelAnimationTests.AnimationSequenceTracksLoopFrame`
- `VmActionModelAnimationTests.HasBaseSpriteModelFrames_WhenKeyPresent`
- `VmActionModelAnimationTests.ResolvePrimaryAnimId_PrefersActorSlot`
- `VmActionModelAnimationTests.TryPickModelIndex_UsesAnimId`
- `VmActionModelAnimationTests.TryPickModelIndex_FallsBackToModelId`
- `VmActionModelAnimationTests.TryPickModelIndex_UsesBaseSpriteModelId`
- `VmActionModelAnimationTests.ResolveFallbackModelId_PrefersModelDataSlot`
- `VmActionModelAnimationTests.FindFModelMissingFrames_WhenNoBaseSpriteKey`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 586 --to 586` (0 hard / 0 soft).

## Seams

`BaseSpriteModelFrames`, `Models`, `FModel`, `AActor` model data, and `IMPLEMENT_CLASS` / serializer bodies are not hosted.

## Credit rationale

Credits SAF flag enum, animation layer type declarations (seams only), and `FindFModel` model-index selection policy ahead of `SetAnimationInternal` (chunk 517).
