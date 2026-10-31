# Chunk 1278 — Light-probe grid, smoothstep falloff, and dynamic sprite light from hw_spritelight.cpp (1-278).

## Scope

Light-probe grid, smoothstep falloff, and dynamic sprite light from hw_spritelight.cpp (1-278).

## Native ranges

- `src/rendering/hwrenderer/scene/hw_spritelight.cpp` **GetDynSpriteLight** 1–278 (278 lines)

**Credited native LOC:** 278 (tally `nativeLoc` 278).

## Managed

`VmSpriteLight1278Slice.cs`
Managed LOC (tally): 412.
Conversion commit: `9f2a2301c93170801a78f1efa03c8adf65c13dc6`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| smoothstep (**32–37**) | yes |
| FindLightProbe (**39–92**) | yes |
| GetDynSpriteLight (**100–207**) | yes |
| Thing and particle overload (**209–219**) | yes |
| hw_GetDynModelLight (**225–278**) | yes |
| Band hull (**1–278**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Linear probe scan | compiled out of the live grid path |
| Portal offset table | host displacement lookup |
| ShouldLightActor | host predicate |
| ShadowTest | host shadow map |
| PosRelative | host relative position |
| AddLightToList | host model-light sink |

An empty probe list returns null and does not read cells. The live path floors the position over the cell size, then walks the three-by-three neighborhood and keeps a probe only when it is strictly closer. A tie keeps the earlier probe. The linear scan over the master list is compiled out, so a nearer probe outside that neighborhood is ignored. Probe color is assigned, then each accepted dynamic light adds. A displacement is applied only when the table is present and the two portal groups are nonzero and different. A matching group, a zero group, or an empty table uses the light position directly. The distance test is strict against the squared radius, and the square root runs only after that test. A spot inverts the vector by the distance, including a zero distance, then multiplies the fraction by the smoothstep of the outer and inner cosines. A non-positive fraction skips the color. A shadow test runs only when the light is shadow mapped and a second radius read is still positive. Alpha scales the color when the target carries the multiply flag. Each channel reads the definition intensity on its own. Subtractive color stores the negated difference from the vector length. A flagged actor does not take the actor branch, and the particle branch then runs when the particle is present and unflagged. A null actor is passed for that particle. Both missing, or a flagged particle with no usable actor, leaves the output untouched. Model data is cleared even when the actor is null. A present actor clears the added-light list, casts the position and radius to float, increments the valid counter, and walks the supplied subsectors. A section whose valid count already matches is skipped before the light list is read, and the field is not written. A light already added from an earlier subsector is not added again. The radius compare is strict in double, and the list records the portal group with attenuation forced on.

| Managed (`VmSpriteLight1278Slice.cs`) | Native (`hw_spritelight.cpp`) |
|---------------------------------------|------------------------------|
| Probe grid, sprite light, and model light | **1–278** |
| Band hull (exclusive slice) | **1–278** |

### Chunk **1278** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **278** |
| **Net headline** | **+278** vs post-**1277** |


**Post-review ratio:** 412/278 managed/native ≈ 148% (src file LOC at HEAD; see tally).

## Tests

- `VmSpriteLight1278SliceTests.Grid_FloorsNegativeCellsAndLeavesTheCompiledScan`
- `VmSpriteLight1278SliceTests.Lights_FallOffAcrossTheSpotAndSkipARepeatedModelLight`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1278 --to 1278` (0 hard / 0 soft).

## Seams

Portal offsets, actor acceptance, shadow tests, relative light positions, and the model-light list are host inputs. The slice keeps the compiled-out probe scan, the strict radius tests, the spot divide, and the validcount compare that does not store.

## Credit rationale

UNCredited hw_spritelight.cpp 1-278 (+278). A full tally line scan shows no overlap. The file ends at line 278.
