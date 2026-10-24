# Chunk 208 — Sprite frame rotation resolve, R_InstallSpriteLump, R_InstallSprite finalize.

## Scope

Sprite frame rotation resolve, R_InstallSpriteLump, R_InstallSprite finalize.

## Native ranges

- `src/r_data/sprites.cpp` **spritedef_t::GetSpriteFrame** 62–95 (34 lines)
- `src/r_data/sprites.cpp` **R_InstallSpriteLump** 105–174 (70 lines)
- `src/r_data/sprites.cpp` **R_InstallSprite** 178–269 (92 lines)

**Credited native LOC:** 196 (tally `nativeLoc` 196).

## Managed

`VmSpriteFrameInstall.cs`, `VmSpriteFrameInstallTests.cs`
Managed LOC (tally): 108.
Conversion commit: `3d6420bfddee21fec233f8daada070db07036996`.

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
