# Chunk 1277 — Fog table, light level, light color, and the fog-boundary check from hw_lighting.cpp (1-271).

## Scope

Fog table, light level, light color, and the fog-boundary check from hw_lighting.cpp (1-271).

## Native ranges

- `src/rendering/hwrenderer/scene/hw_lighting.cpp` **CalcLightLevel** 1–271 (271 lines)

**Credited native LOC:** 271 (tally `nativeLoc` 271).

## Managed

`VmLighting1277Slice.cs`
Managed LOC (tally): 236.
Conversion commit: `0fd60d2c9edcb466e2c5ffc6b81af1aac535ef09`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Distance-fog callback (**36–61**) | yes |
| CalcLightLevel (**69–104**) | yes |
| CalcLightColor (**112–138**) | yes |
| GetFogDensity (**158–207**) | yes |
| CheckFog (**220–271**) | yes |
| Band hull (**1–271**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Sky flat number | host texture id |
| Saved previous mode | unused local |

Setting the distance-fog cvar rebuilds both 256-entry tables. The first row uses integer division, with a break at 164 and a zero at 230. The second row adds six below 128, then a float ramp through 215. Night vision and the weapon-light cvar do not enter that rebuild. A negative cvar keeps the arithmetic shift and toward-zero division. A non-positive light level returns zero before the final clamp. Doom and Doom-dark are dark modes, and software lighting with a positive blend is dark as well. Below 192, a dark non-weapon light above 100 uses the half-even curve. A curve result whose sum with the relative light is below 20 is divided by five after that floor. A weapon uses that curve only when the pure-light flag is set. Every other path adds the relative light, then clamps to 1 through 255. Software color with a zero blend returns the source, including its alpha. Any other color multiplies in integers and stores alpha 255, and a channel outside a byte is truncated. Legacy fog uses the level density, or 18 when that density is zero, before a sector density is considered. Black fog reads the standard row only for linear mode and the other row otherwise, after clamping the light into 0 through 255. Doom software with a zero blend, and the no-fade flag, store zero. Software lighting with a positive blend rewrites the mode to Doom before those cases. An outside color matches only when its alpha is not 255. Otherwise a level density wins, then a light below 248 clamps 255 minus that light into 30 through 255. The same sector object is not a fog boundary. A black front returns immediately. A front alpha, an outside match, and a level or legacy density are empty arms and do not reject a bright front. A black back is an empty arm. An outside match, a level or legacy density, or a back light below 248 rejects the back. The boundary remains when at least one ceiling is not the sky.

| Managed (`VmLighting1277Slice.cs`) | Native (`hw_lighting.cpp`) |
|------------------------------------|---------------------------|
| Fog table, light, and boundary | **1–271** |
| Band hull (exclusive slice) | **1–271** |

### Chunk **1277** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **271** |
| **Net headline** | **+271** vs post-**1276** |


**Post-review ratio:** 236/271 managed/native ≈ 87% (src file LOC at HEAD; see tally).

## Tests

- `VmLighting1277SliceTests.FogTable_DividesInIntegersAndIgnoresNightVision`
- `VmLighting1277SliceTests.Levels_ClampAfterTheCurveAndKeepTheEmptyFogArms`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1277 --to 1277` (0 hard / 0 soft).

## Seams

The sky flat number is a host input. The slice keeps the integer fog table, the half-even curve, the byte color truncate, and the empty fog-boundary arms.

## Credit rationale

UNCredited hw_lighting.cpp 1-271 (+271). A full tally line scan shows no overlap. The file ends at line 271.
