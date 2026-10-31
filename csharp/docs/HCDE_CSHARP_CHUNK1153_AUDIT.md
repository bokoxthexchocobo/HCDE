# Chunk 1153 — Map-thing conversation, UDMF fields, dynamic-light overrides, begin-play, and spawn health from `p_mobj.cpp` (**6942–7094**).

## Scope

Map-thing conversation, UDMF fields, dynamic-light overrides, begin-play, and spawn health from `p_mobj.cpp` (**6942–7094**).

## Native ranges

- `src/playsim/p_mobj.cpp` **Map thing spawn tail** 6942–7094 (153 lines)

**Credited native LOC:** 153 (tally `nativeLoc` 153).

## Managed

`VmPMobjSpawn1153Slice.cs`
Managed LOC (tally): 308.
Conversion commit: `d512d9bd91d7fef71951c837aa51a241e3502dae`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Conversation and UDMF fields (**6943–6972**) | yes |
| Dynamic-light args, flags, and spot angles (**6974–7030**) | yes |
| `CallBeginPlay`, attached lights, `LevelSpawned` (**7032–7058**) | yes |
| Spawn health and `DieFromSpawn` (**7060–7074**) | yes |
| Indexed spawn registration (**7085–7093**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **6882–6941** | spawn compose already credited, including the yaw assignment |
| **7083** | `dumpspawnedthings` declaration; the printf is below |
| **7094** | null check whose body is already credited at **7095–7106** |
| **7095–7106** | indexed spawn body already credited |
| **1501–1685**, **2421–5075**, **5577–5726**, **7880–7900** | standing skips |

`STYLE_Count` is **19**. `LF_NOSHADOWMAP` is **16**, `LF_DONTLIGHTACTORS` is **32**, `LF_SPOT` is **64**, and `LF_DONTLIGHTMAP` is **256**. `OF_EuthanizeMe` is **1<<5**. A conversation id above **0** assigns the looked-up root only when that root is not **-1**. Alpha is applied at **0** and above. A render style equal to `STYLE_Count` is left alone. A scale axis of **0** is left alone; any other axis multiplies. Pitch, roll, and score change only when they are non-zero. A non-zero fill color keeps its low **24** bits and stores the palette index in the top byte. Dynamic lights with a color name write that RGB into args **0–2**. Otherwise a spot light splits `args[0]` with `RPART`, `GPART`, and `BPART`. Defined light booleans set or clear their flag; an undefined key leaves the class default. Soft radius, linearity, and minimum quality apply when they are **0** or positive. Spot angles then read the map thing's original args **1** and **2**, not the split color. `CallBeginPlay` always runs. Attached lights are updated only for a dynamic light, and a null light is skipped. `LevelSpawned` runs only when the actor is not already euthanized. Positive thing health truncates `health * Health` toward zero and, unless the multiplier is exactly **1**, copies that into `StartHealth`. Non-positive health stores the negated truncation. Health **0** looks up `DieFromSpawn` and sets that state with `nofunction` when it exists. Index **0** registers invasion and coop spawns, then always calls the inner spawn.

| Managed (`VmPMobjSpawn1153Slice.cs`) | Native (`p_mobj.cpp`) |
|--------------------------------------|------------------------|
| Spawn tail and index open | **6942–7094** |
| Band hull (exclusive slice) | **6942–7094** |

### Chunk **1153** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **153** |
| **Net headline** | **+153** vs post-**1152** |


**Post-review ratio:** 308/153 managed/native ≈ 201% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjSpawn1153SliceTests.ConversationAndUdmf_ApplyOnlyDefinedFields`
- `VmPMobjSpawn1153SliceTests.Light_SplitsSpotColorAndCopiesAttachedLights`
- `VmPMobjSpawn1153SliceTests.Health_ScalesPositiveValuesAndDiesAtZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1153 --to 1153` (0 hard / 0 soft).

## Seams

Conversation lookup, palette matching, color-name parsing, attached-light objects, and the inner spawn call stay host-supplied.

## Credit rationale

UNCredited **`p_mobj.cpp`** **6942–7094** (+153), inside the 9061-line file, with no conversion or recognition overlap. Last in-file hole of at least 150 lines outside the standing skips.
