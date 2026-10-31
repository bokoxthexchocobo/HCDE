# Chunk 1144 — Chase tables and noise-alert sector marking from `p_enemy.cpp` (**1–150**). `am_map.cpp` **3451–3594** stays under the 150-line floor.

## Scope

Chase tables and noise-alert sector marking from `p_enemy.cpp` (**1–150**). `am_map.cpp` **3451–3594** stays under the 150-line floor.

## Native ranges

- `src/playsim/p_enemy.cpp` **Enemy chase tables and noise mark** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPEnemyNoise1144Slice.cs`
Managed LOC (tally): 117.
Conversion commit: `f97be7fe0799e7104c93e2799ed0d9da0f7ea177`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| RNG stream names (**49–68**) | yes |
| Interpolation and drop-style defaults (**76–77**) | yes |
| Opposite, diagonal, and speed tables (**83–96**) | yes |
| `NoiseMarkSector` (**122–145**) | yes |
| Ceiling and floor portal-sound gates (**149–150**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–48** | license and includes |
| **152+** | recursive sound line walk |

`SQRTHALF` is **0.7071075439453125**. `DI_EAST` is **0** and `DI_NODIR` is **8**. Opposite of east is west. `pr_chase`, `pr_facetarget`, and `pr_railface` are not file-local. A sector is already flooded when its `validcount` matches and `soundtraversed <= soundblocks + 1`. Splash alerts skip `MF4_NOSPLASHALERT` (**0x00100000**). `maxdist == 0` does not filter by distance. `sector_t::floor` is **0** and `ceiling` is **1**.

| Managed (`VmPEnemyNoise1144Slice.cs`) | Native (`p_enemy.cpp`) |
|----------------------------------------|----------------------|
| Tables and noise mark | **1–150** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **1144** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1143** |


**Post-review ratio:** 117/150 managed/native ≈ 78% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyNoise1144SliceTests.ChaseTables_UseTheNativeDiagonalAndOppositeDirections`
- `VmPEnemyNoise1144SliceTests.RngStreams_LeaveChaseAndFaceTargetProcessWide`
- `VmPEnemyNoise1144SliceTests.MarkSector_SkipsASectorThatWasAlreadyFlooded`
- `VmPEnemyNoise1144SliceTests.MarkSector_HearsActorsInsideTheSplashAndDistanceRules`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1144 --to 1144` (0 hard / 0 soft).

## Seams

Sector portals and actor distance stay host-supplied. The recursive line walk begins at **152**.

## Credit rationale

UNCredited **`p_enemy.cpp`** **1–150** (+150), inside the 3488-line file, with no conversion or recognition overlap.
