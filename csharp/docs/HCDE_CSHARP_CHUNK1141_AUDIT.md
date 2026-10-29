# Chunk 1141 — Key marks and thing triangles from `am_map.cpp` (**3001–3150**); follows **1140**.

## Scope

Key marks and thing triangles from `am_map.cpp` (**3001–3150**); follows **1140**.

## Native ranges

- `src/am_map.cpp` **Automap keys and things** 3001–3150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapKeysThings1141Slice.cs`
Managed LOC (tally): 181.
Conversion commit: `e54cb07697b2db2b1fcd56b7065386a08212b8a3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Key class, angle, and easy-key color (**3001–3031**) | yes |
| Thing visibility and sprite frame choice (**3047–3094**) | yes |
| Triangle colors through the scale-16 draw (**3096–3150**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2851–3000** | player arrows already credited by **1140** |
| **3153+** | cheat bounding box |

`MF_SPECIAL` is **1**, `MF3_ISMONSTER` is **0x2000**, `MF6_NOTONAUTOMAP` is **0x40000000**, and `RF_INVISIBLE` is **0x8000**. A Dehacked pickup is a key only when its replacee is a key. Sprite passes run from `am_showthingsprites` down to **1**. The rotation index is `(int)(normalizedDegrees * 16 / 360)`. Thing positions are summed in single precision. A living friendly monster wins over the kill-count color. Keys already drawn by the easy-key or show-keys path set RGB **0** and skip the triangle. The triangle scale is **16**.

| Managed (`VmAmMapKeysThings1141Slice.cs`) | Native (`am_map.cpp`) |
|---------------------------------------------|---------------------|
| Keys and thing marks | **3001–3150** |
| Band hull (exclusive slice) | **3001–3150** |

### Chunk **1141** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1140** |


**Post-review ratio:** 181/150 managed/native ≈ 121% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapKeysThings1141SliceTests.Keys_FollowTheReplaceeAndUseAZeroScaleMark`
- `VmAmMapKeysThings1141SliceTests.Things_AreVisibleForCheatsOrWhenTheScannerFlagIsClear`
- `VmAmMapKeysThings1141SliceTests.SpriteAngle_AddsElevenPointTwoFiveWhenTheSideTexturesDiffer`
- `VmAmMapKeysThings1141SliceTests.ClassifyThing_PrefersLivingMonstersAndSuppressesKeysAlreadyDrawn`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1141 --to 1141` (0 hard / 0 soft).

## Seams

Class identity, portal position, and textures stay host-supplied. The cheat radius box begins at **3153**.

## Credit rationale

UNCredited **`am_map.cpp`** **3001–3150** (+150), inside the 3594-line file, with no conversion or recognition overlap.
