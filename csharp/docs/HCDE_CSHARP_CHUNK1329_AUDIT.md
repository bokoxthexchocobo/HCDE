# Chunk 1329 — Deprecated flags, string match, pointer check, and the early actor info properties from thingdef_properties.cpp (249-616).

## Scope

Deprecated flags, string match, pointer check, and the early actor info properties from thingdef_properties.cpp (249-616).

## Native ranges

- `src/scripting/thingdef_properties.cpp` **HandleDeprecatedFlags** 249–616 (368 lines)

**Credited native LOC:** 368 (tally `nativeLoc` 368).

## Managed

`VmDeprecatedFlags1329Slice.cs`
Managed LOC (tally): 413.
Conversion commit: `99f5ee9ddc6ab5f3adb4cc8cd29748815bb33ba3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Deprecated flag setters (**260–374**) | yes |
| Deprecated flag checks (**395–457**) | yes |
| String match and pointer check (**471–497**) | yes |
| Game, spawn id, conversation, skip_super, and state usage (**509–613**) | yes |
| Band hull (**249–616**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| CheckActorFlag | ends at line 248, already credited |
| DEFINE_PROPERTY(tag) | starts at line 617, already credited |
| Class search and baggage reset | callees |

A nonzero set value enables the flag. Fire and ice damage store those names, and a cleared flag stores None. Gravity uses one eighth or one quarter, and the clear value is 1. Bounce clears the type mask and the see-sound bit, keeps bits outside that mask, then ors the compatibility mode. Fire resistance writes the Fire factor and the check returns on the first Fire entry. Pickup flash compares the tentative class with FindClass, so two nulls count as set. Inter-hub strip stores 0 when set and 1 when cleared. Higher missile probability checks `<= 160`. Missile-more and missile-even-more fold 0.125, 0.0625, and 0.5. Full-volume sounds set three flags and the check only requires see plus death. An unknown index changes nothing and checks false. MatchString is case-insensitive and stops at a null entry. Pointer check requires both class pointers and a descendant restriction, including the same class. Game names accumulate, Any replaces the mask, and an unknown name errors without changing the mask. Spawn ids outside 0 through 65535 error and leave the previous id. Conversation ids outside 1 through 65535 are read and ignored. skip_super rejects a larger class before it rejects a state block, then copies the default tail and marks the skip. Default state usage stores the integer as given.

| Managed (`VmDeprecatedFlags1329Slice.cs`) | Native (`thingdef_properties.cpp`) |
|-------------------------------------------|-------------------------------------|
| Deprecated flags and the early info properties | **249–616** |
| Band hull (exclusive slice) | **249–616** |

### Chunk **1329** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **368** |
| **Net headline** | **+368** vs post-**1328** |


**Post-review ratio:** 413/368 managed/native ≈ 112% (src file LOC at HEAD; see tally).

## Tests

- `VmDeprecatedFlags1329SliceTests.Handle_SetsTheDeprecatedPropertiesAndChecksThem`
- `VmDeprecatedFlags1329SliceTests.Properties_MatchStringsAndRejectBadIds`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1329 --to 1329` (0 hard / 0 soft).

## Seams

FindClassTentative, FindClass, and ResetBaggage stay callees. The default-actor tail is copied as the bytes supplied for GetDefault. tag at line 617 stays on the later property chunk.

## Credit rationale

UNCredited thingdef_properties.cpp 249-616 (+368). A full tally line scan shows no overlap. CheckActorFlag ends at line 248 and DEFINE_PROPERTY(tag) starts at line 617.
