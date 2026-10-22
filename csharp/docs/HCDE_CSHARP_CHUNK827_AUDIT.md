# Chunk 827 — Decal animation thinkers (`DDecalFader`, `DDecalStretcher`, `DDecalSlider`, `DDecalColorer`) from `a_decalfx.cpp` (39–236).

## Scope

Decal animation thinkers (`DDecalFader`, `DDecalStretcher`, `DDecalSlider`, `DDecalColorer`) from `a_decalfx.cpp` (39–236).

## Native ranges

- `src/playsim/mapthinkers/a_decalfx.cpp` **DDecalThinker::Serialize** 39–43 (5 lines)
- `src/playsim/mapthinkers/a_decalfx.cpp` **DDecalFader::Serialize** 47–53 (7 lines)
- `src/playsim/mapthinkers/a_decalfx.cpp` **DDecalFader::Tick** 55–83 (29 lines)
- `src/playsim/mapthinkers/a_decalfx.cpp` **DDecalStretcher::Serialize** 87–99 (13 lines)
- `src/playsim/mapthinkers/a_decalfx.cpp` **DDecalStretcher::Tick** 101–142 (42 lines)
- `src/playsim/mapthinkers/a_decalfx.cpp` **DDecalSlider::Serialize** 146–154 (9 lines)
- `src/playsim/mapthinkers/a_decalfx.cpp` **DDecalSlider::Tick** 156–185 (30 lines)
- `src/playsim/mapthinkers/a_decalfx.cpp` **DDecalColorer::Serialize** 189–196 (8 lines)
- `src/playsim/mapthinkers/a_decalfx.cpp` **DDecalColorer::Tick** 198–236 (39 lines)

**Credited native LOC:** 182 (tally `nativeLoc` 182).

## Managed

`VmADecalFx.cs`
Managed LOC (tally): 223.
Conversion commit: `cfa366293924a32d30e71bcdca9a349c0fa381b3`.

## Padding review (2026-10-21)

| Managed | Native (`a_decalfx.cpp`) |
|---------|-------------------------|
| `TickFader` + serialize fields | `DDecalFader` **47–83** |
| `TickStretcher` | `DDecalStretcher` **87–142** |
| `TickSlider` | `DDecalSlider` **146–185** |
| `TickColorer` | `DDecalColorer` **189–236** |
| base serialize | `DDecalThinker::Serialize` **39–43** |

**Post-review ratio:** 223/182 managed/native ≈ 123% (src file LOC at HEAD; see tally).

## Tests

- `VmADecalFxTests.FaderTick_NullDecal_DestroySelf`
- `VmADecalFxTests.FaderTick_MidFade_SetsAlpha`
- `VmADecalFxTests.FaderTick_EndWindow_ExpiresAndDestroysDecal`
- `VmADecalFxTests.StretcherTick_AtEnd_AppliesGoals`
- `VmADecalFxTests.SliderTick_AtEnd_SetsZ`
- `VmADecalFxTests.ColorerTick_End_AppliesGoalShade`
- `VmADecalFxTests.ColorerTick_MissingFixedColorFlag_DestroySelf`
- `VmADecalFxTests.ColorerTick_MidBlend_InterpolatesRgb`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 827 --to 827` (0 hard / 0 soft).

## Seams

`Level->maptime`, `isFrozen`, and live decal fields (`Alpha`, `ScaleX/Y`, `Z`, `SetShade`) are injected via `DecalRef`; no forwarder to `VmADecalImpact`/`VmADecalBase`.

## Credit rationale

First tally credit for decal fade, stretch, slide, and color animation thinkers.
