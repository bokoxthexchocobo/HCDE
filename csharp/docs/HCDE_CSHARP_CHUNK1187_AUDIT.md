# Chunk 1187 — Hub exit totals and hub serialize from g_hub.cpp (1–183).

## Scope

Hub exit totals and hub serialize from g_hub.cpp (1–183).

## Native ranges

- `src/g_hub.cpp` **G_LeavingHub** 1–183 (183 lines)

**Credited native LOC:** 183 (tally `nativeLoc` 183).

## Managed

`VmGHub1187Slice.cs`
Managed LOC (tally): 285.
Conversion commit: `b65cea0f3038b0ab86bb200e2ccd858d44bf0ebb`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| FHubInfo assignment (**54–64**) | yes |
| G_LeavingHub (**70–136**) | yes |
| Serialize player (**144–157**) | yes |
| Serialize hub info (**159–173**) | yes |
| G_SerializeHub (**175–178**) | yes |
| G_ClearHubInfo (**180–183**) | yes |
| Band hull (**1–183**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A hub cluster stores one row per level number. The stored level number is the level, after the assignment copies finished_ep. Single-player, and only single-player, replaces that row's first player kills, items, and secrets with the level counters. Leaving the hub zeros those three counters on the intermission, then adds every stored row, including the row just written. Total kills come from the level, not from the sum. Max frags are not summed. A language cluster name is looked up, and a literal name also clears the author. Either name invalidates the level-name texture. The same hub does not rewrite the intermission and does not clear the rows. Any other finish clears the rows even when the cluster is not a hub. An empty hub list writes nothing. A present list writes hubinfo, then each hub, then 64 players, then 64 frags. A closed object writes no fields. A missing hubinfo array on read clears the list.

| Managed (`VmGHub1187Slice.cs`) | Native (`g_hub.cpp`) |
|---------------------------------|----------------------|
| Leaving hub and serialize | **1–183** |
| Band hull (exclusive slice) | **1–183** |

### Chunk **1187** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **183** |
| **Net headline** | **+183** vs post-**1186** |


**Post-review ratio:** 285/183 managed/native ≈ 156% (src file LOC at HEAD; see tally).

## Tests

- `VmGHub1187SliceTests.LeavingHub_SumsEveryMapThenClearsWhenTheHubEnds`
- `VmGHub1187SliceTests.SameHub_KeepsRowsAndDeathmatchKeepsThePassedCounters`
- `VmGHub1187SliceTests.SerializeHub_WritesPlayerFieldsAndDropsAMissingRead`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1187 --to 1187` (0 hard / 0 soft).

## Seams

Cluster flags, the level counters, and the string table are host fields. deathmatch is the int cvar. The archive records the write calls; a missing hubinfo array on read clears the list.

## Credit rationale

UNCredited g_hub.cpp 1–183 (+183). The file ends at 183. a_keys.cpp has no remaining hole of at least 150 lines: 164–343 is chunk 173 and 344–493 is chunk 1026. A full tally line scan shows no overlap.
