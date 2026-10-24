# Chunk 723 — Whole `P_DisconnectEffect` and `DVisualThinker` lifecycle/render helpers (`p_effect.cpp` 987–1376). VM glue `DEFINE_ACTION_FUNCTION_NATIVE` bands excluded.

## Scope

Whole `P_DisconnectEffect` and `DVisualThinker` lifecycle/render helpers (`p_effect.cpp` 987–1376). VM glue `DEFINE_ACTION_FUNCTION_NATIVE` bands excluded.

## Native ranges

- `src/playsim/p_effect.cpp` **P_DisconnectEffect** 987–1011 (25 lines)
- `src/playsim/p_effect.cpp` **DVisualThinker::Construct** 1021–1047 (27 lines)
- `src/playsim/p_effect.cpp` **DVisualThinker::OnDestroy** 1049–1060 (12 lines)
- `src/playsim/p_effect.cpp` **DVisualThinker::NewVisualThinker** 1067–1097 (31 lines)
- `src/playsim/p_effect.cpp` **DVisualThinker::UpdateSector** 1109–1122 (14 lines)
- `src/playsim/p_effect.cpp` **DVisualThinker::UpdateSpriteInfo** 1152–1161 (10 lines)
- `src/playsim/p_effect.cpp` **DVisualThinker::ValidTexture** 1175–1178 (4 lines)
- `src/playsim/p_effect.cpp` **DVisualThinker::Tick** 1181–1228 (48 lines)
- `src/playsim/p_effect.cpp` **DVisualThinker::GetLightLevel** 1230–1243 (14 lines)
- `src/playsim/p_effect.cpp` **DVisualThinker::InterpolatedPosition** 1245–1252 (8 lines)
- `src/playsim/p_effect.cpp` **DVisualThinker::InterpolatedRoll** 1254–1259 (6 lines)
- `src/playsim/p_effect.cpp` **DVisualThinker::SetTranslation** 1263–1279 (17 lines)
- `src/playsim/p_effect.cpp` **DVisualThinker::GetParticleType** 1295–1308 (14 lines)
- `src/playsim/p_effect.cpp` **DVisualThinker::isFrozen** 1326–1329 (4 lines)
- `src/playsim/p_effect.cpp` **DVisualThinker::SetRenderStyle** 1337–1343 (7 lines)
- `src/playsim/p_effect.cpp` **DVisualThinker::GetRenderStyle** 1354–1357 (4 lines)
- `src/playsim/p_effect.cpp` **DVisualThinker::GetOffset** 1370–1376 (7 lines)

**Credited native LOC:** 252 (tally `nativeLoc` 252).

## Managed

`VmPEffectVisualThinker.cs`
Managed LOC (tally): 238.
Conversion commit: `48bc668095c368adbb734caf1dd976a776e4058a`.

## Padding review (2026-10-21)

| Managed | Native (`p_effect.cpp`) |
|---------|------------------------|
| `DisconnectEffect` | `P_DisconnectEffect` 987–1011 |
| `Construct` / `OnDestroy` | `DVisualThinker::Construct` 1021–1047; `OnDestroy` 1049–1060 |
| `NewVisualThinker` / sector update | `NewVisualThinker` 1067–1097; `UpdateSector` 1109–1122 |
| `UpdateSpriteInfo` / `ValidTexture` / `Tick` | 1152–1161; 1175–1178; 1181–1228 |
| `GetLightLevel` / interpolate / translation / particle type | 1230–1308; 1263–1279; 1295–1308 |
| `isFrozen` / render style / offset | 1321–1329; 1337–1368; 1370–1376 |


**Post-review ratio:** 238/252 managed/native ≈ 94% (src file LOC at HEAD; see tally).

## Tests

- `VmPEffectVisualThinkerTests.DisconnectEffect_SpawnsUntilPoolEmpty`
- `VmPEffectVisualThinkerTests.Tick_DestroysWhenTextureInvalid`
- `VmPEffectVisualThinkerTests.InterpolatedPosition_BlendsPrevAndCurrent`
- `VmPEffectVisualThinkerTests.GetParticleType_MapsSquareFlag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 723 --to 723` (0 hard / 0 soft).

## Seams

`JitterParticle`, `Level->CreateThinker`, ZScript `BeginPlay`, `R_FindCustomTranslation`, and renderer I/O remain native.

## Credit rationale

Disconnect puff burst and client-side visual thinker motion/lighting helpers.
