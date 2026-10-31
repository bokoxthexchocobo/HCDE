# Chunk 1205 — String map, JSON round trip, and iterator from dictionary.cpp (1–241).

## Scope

String map, JSON round trip, and iterator from dictionary.cpp (1–241).

## Native ranges

- `src/common/scripting/core/dictionary.cpp` **Dictionary::Serialize** 1–241 (241 lines)

**Credited native LOC:** 241 (tally `nativeLoc` 241).

## Managed

`VmDictionary1205Slice.cs`
Managed LOC (tally): 305.
Conversion commit: `fc7483cee4048ffb9f650a0d04ceb80f7c93bcd0`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Dictionary::Serialize (**51–71**) | yes |
| DictCreate through DictRemove (**73–99**) | yes |
| Dictionary exports (**107–151**) | yes |
| DictionaryIterator constructor and Serialize (**159–172**) | yes |
| init through DictIteratorValue (**174–206**) | yes |
| DictionaryIterator exports (**214–241**) | yes |
| Band hull (**1–241**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

Insert adds a key or replaces its value in place. At returns an empty string when the key is absent. Remove of a missing key does nothing. ToJson writes a compact JSON object. FromJson of the exact text null returns a null pointer. An empty string returns an empty map. Any other text that is not a JSON object throws. A JSON value that is not a string throws. Serialize writes that JSON. Serialize reads it, and a null pointer is dereferenced. The iterator starts before the first pair. Next leaves the previous pair in place when it returns false. Key and Value are empty until the first successful step. Saving an iterator throws. Loading an iterator does nothing.

| Managed (`VmDictionary1205Slice.cs`) | Native (`dictionary.cpp`) |
|--------------------------------------|---------------------------|
| Dictionary | **1–241** |
| Band hull (exclusive slice) | **1–241** |

### Chunk **1205** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **241** |
| **Net headline** | **+241** vs post-**1204** |


**Post-review ratio:** 305/241 managed/native ≈ 127% (src file LOC at HEAD; see tally).

## Tests

- `VmDictionary1205SliceTests.Insert_ReplacesInPlaceAndIteratorStopsOnTheLastPair`
- `VmDictionary1205SliceTests.Json_RoundTripAndRejectsNullAndNonStrings`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1205 --to 1205` (0 hard / 0 soft).

## Seams

Pairs are kept in insertion order. TMap walks hash slots in an unspecified order. ToJson and FromJson are the rapidjson object-of-strings codec in serializer.cpp that these exports call. A null load crashes on the native pointer. Saving an iterator is the I_Error string.

## Credit rationale

UNCredited dictionary.cpp 1–241 (+241). The file ends at 241. A full tally line scan shows no overlap.
