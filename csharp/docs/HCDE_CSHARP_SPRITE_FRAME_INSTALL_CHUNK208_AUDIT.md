# Chunk 208 — Sprite frame install / lookup

`VmSpriteFrameInstall` — `GetSpriteFrame` rotation selection, `R_InstallSpriteLump`, `R_InstallSprite` finalize passes.

**Seams:** Sprite definition lump scanning (`R_InitSpriteDefs`) and `SpriteFrames` heap allocation (`sprites[num].spriteframes = Reserve`) are not ported; `FinalizeSpriteFrames` covers rotation propagation only.

Tests: `VmSpriteFrameInstallTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
