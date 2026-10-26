# Chunk 208 — Sprite frame rotation resolve, R_InstallSpriteLump, R_InstallSprite finalize.

## Scope

Sprite frame rotation resolve, R_InstallSpriteLump, R_InstallSprite finalize.

## Native ranges

- `src/r_data/sprites.cpp` **spritedef_t::GetSpriteFrame** 62–95 (34 lines)
- `src/r_data/sprites.cpp` **R_InstallSpriteLump** 105–174 (70 lines)
- `src/r_data/sprites.cpp` **R_InstallSprite** 178–269 (92 lines)

**Credited native LOC:** 196 (tally `nativeLoc` 196).

## Managed

`VmDehackedDehSuppLumpGate.cs`
Managed LOC (tally): 112.
Conversion commit: `650909e972162e2068ac042dd11922bafe9baf29`.

## Padding review (2026-10-21)



### Out-of-body uncredit (208)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **196** |
| **Out-of-body removed** | **0** |
| **Honest** | **196** |


**Post-review ratio:** 112/196 managed/native ≈ 57% (src file LOC at HEAD; see tally).

## Tests

- `VmSpriteFrameInstallTests.TryGetSpriteTextureId_UsesRequestedRotationAndMirror`
- `VmSpriteFrameInstallTests.InstallSpriteLump_FillsAllRotationsForZeroRotation`
- `VmSpriteFrameInstallTests.InstallSpriteLump_AssignsSpecificRotation`
- `VmSpriteFrameInstallTests.FinalizeSpriteFrames_PropagatesSingleRotation`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 208 --to 208` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
