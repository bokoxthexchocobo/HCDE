# Chunk 1455 — Mute commands and settings controllers in d_net.cpp (10387-10667).

## Scope

Mute commands and settings controllers in d_net.cpp (10387-10667).

## Native ranges

- `src/d_net.cpp` **mute** 10387–10667 (281 lines)

**Credited native LOC:** 281 (tally `nativeLoc` 281).

## Managed

`VmNet1455Slice.cs`
Managed LOC (tally): 214.
Conversion commit: `0bccb30132d41afc82d07e88184f1a043b5ac469`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Mute and settings controllers (**10387–10667**) | yes |
| Band hull (**10387–10667**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Ready and kick | lines 10127-10385 are already credited |
| Lines after 10667 | the translation unit ends here |

Mute skips the console player and duplicate numbers. Adding a controller writes opcode 48 and removing one writes opcode 49. The authority cannot change its own controller bit, and remove reports a bad player number where add reports a bad client number.

| Managed (`VmNet1455Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| Mute through ListControllers | **10387–10667** |
| Band hull (exclusive slice) | **10387–10667** |

### Chunk **1455** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **281** |
| **Net headline** | **+281** vs the prior chunk |


**Post-review ratio:** 214/281 managed/native ≈ 76% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1455SliceTests.MuteBitsAndList`
- `VmNet1455SliceTests.SettingsControllers`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1455 --to 1455` (0 hard / 0 soft).

## Seams

CCMD mute through CCMD listsettingscontrollers. CCMD kick ends at 10385. This is the end of d_net.cpp.

## Credit rationale

UNCredited d_net.cpp 10387-10667 (+281). One contiguous span of the mute commands and the settings-controller editor. The span does not cross credited code.
