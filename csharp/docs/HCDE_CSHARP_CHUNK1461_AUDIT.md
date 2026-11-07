# Chunk 1461 — InitSectorSpecial damage, scroll, and door tail in specials.cpp (459-614).

## Scope

InitSectorSpecial damage, scroll, and door tail in specials.cpp (459-614).

## Native ranges

- `src/maploader/specials.cpp` **MapLoader::InitSectorSpecial** 459–614 (156 lines)

**Credited native LOC:** 156 (tally `nativeLoc` 156).

## Managed

`VmSpecials1461Slice.cs`
Managed LOC (tally): 124.
Conversion commit: `a7eb69edddef4d29bbd457127f049129e9f7b8cd`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Sector special tail (**459–614**) | yes |
| Band hull (**459–614**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Kill-monsters flag | lines 456-458 are already credited |
| SpawnSpecials | starts at line 624 |

Without MBF21 the death bit still uses the Boom damage rows. With MBF21, damage 0x200 and 0x300 become exit flags. A matching Doom special clears the low byte unless the case keeps it. The default keeps the low byte and adds a Hexen or Heretic floor scroller when the value is in those ranges.

| Managed (`VmSpecials1461Slice.cs`) | Native (`specials.cpp`) |
|-------------------------------------|------------------------|
| InitTail | **459–614** |
| Band hull (exclusive slice) | **459–614** |

### Chunk **1461** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **156** |
| **Net headline** | **+156** vs the prior chunk |


**Post-review ratio:** 124/156 managed/native ≈ 79% (src file LOC at HEAD; see tally).

## Tests

- `VmSpecials1461SliceTests.BoomDamageAndDoomSpecials`
- `VmSpecials1461SliceTests.HexenScrollHereticCarryAndSkyKeepOrClear`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1461 --to 1461` (0 hard / 0 soft).

## Seams

The closing brace of the credited kill-monsters check through the end of MapLoader::InitSectorSpecial. Lines 456-458 are already credited. SpawnSpecials starts at 624.

## Credit rationale

UNCredited specials.cpp 459-614 (+156). One contiguous tail of InitSectorSpecial. The span does not cross credited code.
