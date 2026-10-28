# Chunk 979 — `AddToList` precache marking and `PrecacheLevel` header from `p_setup.cpp` (**1–150**). **Pivot:** `p_spec.cpp` **1–150** aborted — existing `VmPSpecActivateLine` (74–223) must not be re-credited.

## Scope

`AddToList` precache marking and `PrecacheLevel` header from `p_setup.cpp` (**1–150**). **Pivot:** `p_spec.cpp` **1–150** aborted — existing `VmPSpecActivateLine` (74–223) must not be re-credited.

## Native ranges

- `src/p_setup.cpp` **AddToList** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPSetupPrecache979Slice.cs`
Managed LOC (tally): 78.
Conversion commit: `e52b5e21a95c3701503f6bad20cfd0efc38c0b53`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `AddToList` idempotent merge (**94–97**) | yes |
| Animation expansion (**99–111**) | yes |
| Switch frame marking (**115–136**) | yes |
| Animated door frames (**139–146**) | yes |
| `PrecacheLevel` header (**149–150**) | seam (function entry only) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–93** | includes / comments / forward decls |
| **151+** | outside hull (**151–152** dedicated skip deferred) |

**Aborted target (979 primary)**

| TU | Reason |
|----|--------|
| `p_spec.cpp` **1–150** | `VmPSpecActivateLine` already ports **74–223** — no second credit |

| Managed (`VmPSetupPrecache979Slice.cs`) | Native (`p_setup.cpp`) |
|-------------------------------------------|----------------------|
| `ShouldSkipAddToList` / `TryAddToHitlist` | **94–97** |
| `ShouldExpandAnimationFrames` | **101–108** |
| `ShouldMarkSwitchFrames` / `ShouldUseBoomAnimatedSwitchFallback` | **115–136** |
| `ShouldMarkAnimatedDoorFrames` / `MarkHitlistRange` | **139–146** |
| `ApplyAddToListPolicy` | **94–147** policy |
| `PrecacheLevelHeaderInBand` | **149–150** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **979** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**978** |


**Post-review ratio:** 78/150 managed/native ≈ 52% (src file LOC at HEAD; see tally).

## Tests

- `VmPSetupPrecache979SliceTests.HitlistMerge_SkipsWhenBitSet`
- `VmPSetupPrecache979SliceTests.AnimationExpansion_RespectsDiscreteFlag`
- `VmPSetupPrecache979SliceTests.SwitchAndDoorPredicates`
- `VmPSetupPrecache979SliceTests.MarkHitlistRange_MarksUnsetEntries`
- `VmPSetupPrecache979SliceTests.ApplyAddToListPolicy_ReturnsOneWhenMerged`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 979 --to 979` (0 hard / 0 soft).

## Seams

`TexAnim`, `TexMan`, `memset`, thinker iteration, and `hw_PrecacheTexture` remain host-injected; `PrecacheLevel` body past **150** deferred.

## Credit rationale

First UNCredited `p_setup.cpp` band; no tally row on **1–150**. No overlap with retired **437** / off-tally `VmPSpecActivateLine`.
