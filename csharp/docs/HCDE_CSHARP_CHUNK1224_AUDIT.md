# Chunk 1224 — Stat registration, prefix lookup, and the bottom-up print from stats.cpp (1-150).

## Scope

Stat registration, prefix lookup, and the bottom-up print from stats.cpp (1-150).

## Native ranges

- `src/common/engine/stats.cpp` **FStat::FindStat** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmStats1224Slice.cs`
Managed LOC (tally): 221.
Conversion commit: `0ab7633200252e795911b06d43dfb354c23a437e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Constructor (**34–40**) | yes |
| Destructor (**42–51**) | yes |
| FindStat (**53–70**) | yes |
| ToggleStat name (**72–79**) | yes |
| EnableStat (**81–88**) | yes |
| ToggleStat (**90–93**) | yes |
| PrintStat (**95–125**) | yes |
| DumpRegisteredStats (**127–137**) | yes |
| stat command (**139–150**) | yes |
| Band hull (**1–150**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

Two case-sensitive prefixes cancel. A steady exact match still wins over a prefix. The middle node of a three-node list is not removed.

| Managed (`VmStats1224Slice.cs`) | Native (`stats.cpp`) |
|---------------------------------|---------------------|
| Stat list, lookup, and print | **1–150** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **1224** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1223** |


**Post-review ratio:** 221/150 managed/native ≈ 147% (src file LOC at HEAD; see tally).

## Tests

- `VmStats1224SliceTests.Find_PrefersCaseInsensitiveExactAndRejectsTwoPrefixes`
- `VmStats1224SliceTests.Destroy_UnlinksEvenSlotsAndAMiddleOfThreeThrows`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1224 --to 1224` (0 hard / 0 soft).

## Seams

Exact lookup uses strcasecmp. A prefix match uses strncmp, so the prefix is case-sensitive. A second prefix clears the partial and the lookup returns null when nothing matched exactly. An empty query matches every name as a prefix. The destructor steps to the next node's next link, so only even positions from the head are unlinked. Deleting the middle node of a three-node list follows a null next pointer. Print subtracts the font height plus one, ignores a newline that is the last character, and skips an empty string after GetStats. The virtual size uses the 2D drawer, and the vertical start uses the passed drawer. x is 5 divided by the console scale. The stat command treats argc 2 as the only toggle form.

## Credit rationale

UNCredited stats.cpp 1-150 (+150). The file ends at 150. A full tally line scan shows no overlap.
