# Chunk 770 — Overlay translation/position/alpha, clear/bullet/setup, serialize/safe-flash lifecycle (`p_pspr.cpp` 1092–1569; excludes `ADD_STAT`; merged former 771/772).

## Scope

Overlay translation/position/alpha, clear/bullet/setup, serialize/safe-flash lifecycle (`p_pspr.cpp` 1092–1569; excludes `ADD_STAT`; merged former 771/772).

## Native ranges

- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_OverlayTranslation)** 1092–1122 (31 lines)
- `src/playsim/p_pspr.cpp` **GetOverlayPosition** 1130–1142 (13 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, OverlayX)** 1144–1155 (12 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, OverlayY)** 1157–1168 (12 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, OverlayID)** 1176–1185 (10 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_OverlayAlpha)** 1193–1207 (15 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, OverlayAlpha)** 1210–1225 (16 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_OverlayRenderStyle)** 1233–1249 (17 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_Overlay)** 1257–1275 (19 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_ClearOverlays)** 1277–1318 (42 lines)
- `src/playsim/p_pspr.cpp` **P_BulletSlope** 1330–1356 (27 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, BulletSlope)** 1358–1364 (7 lines)
- `src/playsim/p_pspr.cpp` **P_SetupPsprites** 1374–1387 (14 lines)
- `src/playsim/p_pspr.cpp` **DPSprite::Serialize** 1395–1421 (27 lines)
- `src/playsim/p_pspr.cpp` **player_t::DestroyPSprites** 1429–1440 (12 lines)
- `src/playsim/p_pspr.cpp` **P_SetSafeFlash** 1451–1500 (50 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(_PlayerInfo, SetSafeFlash)** 1502–1510 (9 lines)
- `src/playsim/p_pspr.cpp` **DPSprite::OnDestroy** 1518–1542 (25 lines)
- `src/playsim/p_pspr.cpp` **DPSprite::GetYAdjust** 1550–1569 (20 lines)

**Credited native LOC:** 378 (tally `nativeLoc` 378).

## Managed

`VmPPSpriteOverlayTranslation.cs`, `VmPPSpriteClearBulletSetup.cs`, `VmPPSpriteSerializeLifecycle.cs`
Managed LOC (tally): 475.
Conversion commit: `23c1ff467f334bec338974b0aa44c916ba90ecab`.

## Padding review (2026-10-21)

| Managed | Native (`p_pspr.cpp`) |
|---------|----------------------|
| `OverlayTranslation` … `Overlay` | **1092–1275** (chunk769 successor) |
| `ClearOverlays` / `BulletSlope` / `SetupPsprites` | **1277–1387** |
| `SerializePsprite` / `DestroyPsprites` / `SetSafeFlash` / `OnDestroy` / `GetYAdjust` | **1395–1569** |

### Out-of-body uncredit (770)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **378** |
| **Out-of-body removed** | **0** |
| **Honest** | **378** |


**Post-review ratio:** 475/378 managed/native ≈ 126% (src file LOC at HEAD; see tally).

## Tests

- `VmPPSpriteOverlayTranslationTests.OverlayTranslation_EmptyNameResetsTranslation`
- `VmPPSpriteOverlayTranslationTests.OverlayTranslation_SkipsWhenNotFromPsprite`
- `VmPPSpriteOverlayTranslationTests.OverlayX_ReturnsPositionFromPspIndex`
- `VmPPSpriteOverlayTranslationTests.OverlayAlpha_ClampsAndOverlayCreatesLayer`
- `VmPPSpriteClearBulletSetupTests.ClearOverlays_SafetySkipsWeaponLayers`
- `VmPPSpriteClearBulletSetupTests.BulletSlope_TriesAngledShotsUntilTarget`
- `VmPPSpriteClearBulletSetupTests.SetupPsprites_DestroyAndBringUp`
- `VmPPSpriteSerializeLifecycleTests.SerializePsprite_WritesFieldOrder`
- `VmPPSpriteSerializeLifecycleTests.DestroyPsprites_UnlinksAll`
- `VmPPSpriteSerializeLifecycleTests.SetSafeFlash_UsesOffsetWhenOwned`
- `VmPPSpriteSerializeLifecycleTests.OnDestroy_RemovesFromOwnerList`
- `VmPPSpriteSerializeLifecycleTests.GetYAdjust_UsesStatusBarWhenNotFullscreen`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 770 --to 770` (0 hard / 0 soft).

## Seams

`ACTION_CALL_FROM_PSPRITE`, `FindPSprite`, translation lookup, `CreatePsprite`+`SetState`, `P_AimLineAttack`, serializer/GC, `P_SetPsprite`, and status-bar displacement are injected.

## Credit rationale

Closes uncredited `p_pspr.cpp` tail after chunk769 overlay geometry through `GetYAdjust` (merged sub-150 bands 771/772).
