# Chunk 1382 — Compatibility option tables, side names, and the MD5 map traits from compatibility.cpp (1-203).

## Scope

Compatibility option tables, side names, and the MD5 map traits from compatibility.cpp (1-203).

## Native ranges

- `src/maploader/compatibility.cpp` **FMD5HashTraits::Hash** 1–203 (203 lines)

**Credited native LOC:** 203 (tally `nativeLoc` 203).

## Managed

`VmCompatTables1382Slice.cs`
Managed LOC (tally): 113.
Conversion commit: `a64ac6f9054ee5ad242ecdefe61fde30f8ed775c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Preamble (**1–47**) | yes |
| `FMD5Holder` and `FCompatValues` (**49–62**) | yes |
| `FMD5HashTraits` (**64–77**) | yes |
| `FCompatOption` and slots (**79–92**) | yes |
| `BCompatMap` declaration (**104**) | yes |
| `Options` (**108–173**) | yes |
| `LineSides` (**175–178**) | yes |
| `WallTiers` (**181–184**) | yes |
| `SectorPlanes` (**186–189**) | yes |
| `ZandronumCompatCommands` (**191–202**) | yes |
| Band close (**203**) | yes |
| Band hull (**1–203**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| `SkipZandronumCompatCommand` **204** onward | already credited |

`Hash` returns the first 32-bit word of the 16-byte digest. On this little-endian layout, bytes `11 22 33 44` are `0x44332211`. A digest shorter than 16 bytes is rejected. `Compare` returns 0 when all four words match and 1 when any word differs, including a difference in only the last word. `SLOT_COMPAT` is 0, `SLOT_COMPAT2` is 1, `SLOT_BCOMPAT` is 2, and `COMPATSLOT_COUNT` is 3. The option table keeps source order. `setslopeoverflow` is bug-compat bit 0. `ignoreteleporttags` is bug-compat bit 3, not bit 2. `vileghosts` is compat bit 25. `maskedmidtex` is compat bit 31. `nombf21` is compat2 bit 14 and `noid24` is compat2 bit 24. The option list ends with a null name, flags 0, and slot 0. `LineSides` is Front, Back, and null. `WallTiers` is Top, Mid, Bot, and null. `SectorPlanes` is floor, ceil, and null. `ZandronumCompatCommands` ends with `corpsegibs` and a null sentinel.

| Managed (`VmCompatTables1382Slice.cs`) | Native (`compatibility.cpp`) |
|----------------------------------------|-----------------------------|
| Tables, slots, and MD5 traits | **1–203** |
| Band hull (exclusive slice) | **1–203** |

### Chunk **1382** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **203** |
| **Net headline** | **+203** vs the prior chunk |


**Post-review ratio:** 113/203 managed/native ≈ 56% (src file LOC at HEAD; see tally).

## Tests

- `VmCompatTables1382SliceTests.OptionTableKeepsNativeFlagsAndTheNullSentinel`
- `VmCompatTables1382SliceTests.HashUsesTheFirstWordAndCompareIsUnequal`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1382 --to 1382` (0 hard / 0 soft).

## Seams

SkipZandronumCompatCommand at 204 stays outside this band and is already credited. hash_t is unsigned int. The FMD5Holder union overlays Hash on the first digest word, little-endian. Compare returns 1 when any of the four words differ and 0 when they match. SLOT_COMPAT is 0, SLOT_COMPAT2 is 1, SLOT_BCOMPAT is 2, and COMPATSLOT_COUNT is 3. The null option and the null string sentinels are part of the tables. VmMapLoaderCompatibility is the later parser and is not edited.

## Credit rationale

UNCredited compatibility.cpp 1-203 (+203). After chunk 1381 this is the unique largest hole. c_cvars.cpp 91-292 is 202. Gaps longer than 400 lines stay whole and are not sliced.
