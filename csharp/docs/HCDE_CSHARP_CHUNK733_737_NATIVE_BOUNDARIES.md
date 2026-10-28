# Native function boundaries (chunks 733–737)

Verified against `src/playsim/p_interaction.cpp` and `src/playsim/p_mobj.cpp` at trim commit. Line counts are inclusive endpoints on function signature through closing `}` (ZScript `DEFINE_ACTION_FUNCTION*` blocks listed separately where credited).

## Chunk 730 (unchanged; audit flag)

`FBehavior::Init` **2281–2792** is one function (`bool FBehavior::Init(...)` through closing brace before `~FBehavior`). Row carries **`auditSingleFunctionOversizeException`: true** in `chunk730.json` and `loc_tally.json`.

## Chunk 733 — `p_interaction.cpp` (152 native)

| Function | Lines | LOC |
|----------|------:|----:|
| `static int DoDamageMobj` | 1573–1603 | 31 |
| `int P_DamageMobj` | 1617–1657 | 41 |
| `bool AActor::OkayToSwitchTarget` | 1734–1801 | 68 |
| `bool AActor::CallOkayToSwitchTarget` | 1810–1821 | 12 |

Gaps (not credited): `DEFINE_ACTION_FUNCTION(AActor, DamageMobj)` 1605–1615; `P_PoisonMobj` 1660–1713; `DEFINE_ACTION_FUNCTION(AActor, PoisonMobj)` 1715–1726; comment/header 1728–1732; `DEFINE_ACTION_FUNCTION(AActor, OkayToSwitchTarget)` 1803–1808.

## Chunk 734 — `p_mobj.cpp` (150 native)

| Function | Lines | LOC |
|----------|------:|----:|
| `static int Grind` | 1554–1685 | 132 |
| `DEFINE_ACTION_FUNCTION_NATIVE(AActor, Grind, Grind)` | 1687–1692 | 6 |
| `bool AActor::CallGrind` | 1694–1705 | 12 |

No managed-only glue is counted. Blank line 1686 between `Grind` and the DEFINE is inside the credited span only as the single separator line between adjacent native symbols (not extra logic).

## Chunk 735 — `p_mobj.cpp` (171 native)

| Function | Lines | LOC |
|----------|------:|----:|
| `void AActor::FallAndSink` | 3355–3432 | 78 |
| `void P_NightmareRespawn` | 3459–3551 | 93 |

Not credited (adjacent): `DEFINE_ACTION_FUNCTION(AActor, FallAndSink)` 3434–3441; `AActor::CallFallAndSink` 3443–3454.

## Chunk 736 — `p_mobj.cpp` (170 native)

| Function | Lines | LOC |
|----------|------:|----:|
| `void SerializeModelID` | 1749–1772 | 24 |
| `SerializeBoneOverrideComponent` (template) | 1774–1786 | 13 |
| `FSerializer &Serialize(..., BoneOverride &)` | 1788–1796 | 9 |
| `FSerializer &Serialize(..., TRS &)` | 1798–1806 | 9 |
| `FSerializer &Serialize(..., ModelOverride &)` | 1808–1815 | 8 |
| `FSerializer &Serialize(..., AnimModelOverride &)` | 1817–1830 | 14 |
| `FSerializer &Serialize(..., ModelAnim &)` | 1832–1845 | 14 |
| `FSerializer &Serialize(..., ModelAnimFrame &)` | 1847–1918 | 72 |

`DActorModelData::Serialize` begins at **1920** and is excluded.

## Chunk 737 — `p_mobj.cpp` (159 native after trim)

| Function | Lines | LOC |
|----------|------:|----:|
| `bool AActor::SetState` | 856–961 | 106 |
| `DEFINE_ACTION_FUNCTION(AActor, SetState)` | 963–969 | 7 |
| `static void DestroyAllInventory` | 978–1005 | 28 |
| `DEFINE_ACTION_FUNCTION_NATIVE(AActor, DestroyAllInventory, ...)` | 1007–1012 | 6 |
| `bool AActor::UseInventory` | 1024–1035 | 12 |

Prior row incorrectly ended `SetState` at **969** without splitting the method (961) from the DEFINE wrapper (963–969). After trim, total **159** native (not floor-padded).
